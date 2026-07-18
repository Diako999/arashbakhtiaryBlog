from django.contrib.auth import get_user_model
from django.core.cache import cache
from django.test import TestCase
from django.urls import reverse
from django_otp.oath import TOTP
from django_otp.plugins.otp_static.models import StaticDevice, StaticToken
from django_otp.plugins.otp_totp.models import TOTPDevice

from apps.blog.models import Comment, Post

User = get_user_model()


def _login_and_verify(client, user, password):
    """Full login + 2FA cycle, for tests that need past-the-gate access."""
    device = TOTPDevice.objects.create(user=user, name="default", confirmed=True)
    client.login(username=user.username, password=password)
    token = str(TOTP(device.bin_key, device.step, device.t0, device.digits).token()).zfill(
        device.digits
    )
    client.post(reverse("dashboard:otp_verify"), {"token": token})


class TwoFactorGateTests(TestCase):
    """The dashboard's core safety property: login alone is never enough —
    every view behind OTPRequiredMixin also requires a verified TOTP device."""

    def setUp(self):
        cache.clear()  # otp_verify is rate-limited; don't let tests trip each other's limit
        self.user = User.objects.create_user(username="admin", password="s3cret-pw!")
        self.overview_url = reverse("dashboard:overview")

    def test_anonymous_redirects_to_login(self):
        response = self.client.get(self.overview_url)
        self.assertRedirects(
            response, f"{reverse('dashboard:login')}?next={self.overview_url}"
        )

    def test_logged_in_without_device_redirects_to_otp_setup(self):
        self.client.login(username="admin", password="s3cret-pw!")
        response = self.client.get(self.overview_url)
        self.assertRedirects(response, reverse("dashboard:otp_setup"))

    def test_logged_in_with_unverified_device_redirects_to_otp_verify(self):
        TOTPDevice.objects.create(user=self.user, name="default", confirmed=True)
        self.client.login(username="admin", password="s3cret-pw!")
        response = self.client.get(self.overview_url)
        self.assertRedirects(response, reverse("dashboard:otp_verify"))

    def test_valid_totp_token_grants_access(self):
        device = TOTPDevice.objects.create(user=self.user, name="default", confirmed=True)
        self.client.login(username="admin", password="s3cret-pw!")
        token = str(
            TOTP(device.bin_key, device.step, device.t0, device.digits).token()
        ).zfill(device.digits)

        response = self.client.post(reverse("dashboard:otp_verify"), {"token": token})
        self.assertRedirects(response, self.overview_url)
        self.assertEqual(self.client.get(self.overview_url).status_code, 200)

    def test_recovery_code_grants_one_time_access(self):
        TOTPDevice.objects.create(user=self.user, name="default", confirmed=True)
        static_device = StaticDevice.objects.create(user=self.user, name="backup")
        StaticToken.objects.create(device=static_device, token="recovery1")
        self.client.login(username="admin", password="s3cret-pw!")

        response = self.client.post(
            reverse("dashboard:otp_verify"), {"token": "recovery1"}
        )
        self.assertRedirects(response, self.overview_url)
        self.assertFalse(
            StaticToken.objects.filter(device=static_device, token="recovery1").exists()
        )


class CommentModerationTests(TestCase):
    """The custom Dashboard's own moderation screen — so admins don't need
    to fall back to /manage-portal/ (the stock Django admin) just to approve
    or delete a comment."""

    def setUp(self):
        cache.clear()  # otp_verify is rate-limited; don't let tests trip each other's limit
        self.user = User.objects.create_user(username="admin", password="s3cret-pw!")
        self.author = User.objects.create_user(username="author", password="pw")
        self.post = Post.objects.create(title="Hello", author=self.author, body="Body")
        self.comment = Comment.objects.create(
            post=self.post, name="Spammer", email="spam@example.com", body="Buy now!"
        )
        _login_and_verify(self.client, self.user, "s3cret-pw!")

    def test_anonymous_cannot_reach_moderation_screen(self):
        self.client.logout()
        response = self.client.get(reverse("dashboard:comments"))
        self.assertNotEqual(response.status_code, 200)

    def test_list_shows_pending_comment(self):
        response = self.client.get(reverse("dashboard:comments"))
        self.assertEqual(response.status_code, 200)
        self.assertContains(response, "Spammer")

    def test_toggle_approves_comment(self):
        self.client.post(reverse("dashboard:comment_toggle", args=[self.comment.pk]))
        self.comment.refresh_from_db()
        self.assertTrue(self.comment.is_approved)

    def test_delete_removes_comment(self):
        self.client.post(reverse("dashboard:comment_delete", args=[self.comment.pk]))
        self.assertFalse(Comment.objects.filter(pk=self.comment.pk).exists())


class AnalyticsTests(TestCase):
    """The Dashboard's analytics screen — surfaces which posts/categories get
    the most views, ranked by the real view_count column."""

    def setUp(self):
        cache.clear()  # otp_verify is rate-limited; don't let tests trip each other's limit
        self.user = User.objects.create_user(username="admin", password="s3cret-pw!")
        self.author = User.objects.create_user(username="author", password="pw")
        self.popular = Post.objects.create(
            title="Popular post", author=self.author, body="Body",
            status=Post.STATUS_PUBLISHED, view_count=50,
        )
        self.quiet = Post.objects.create(
            title="Quiet post", author=self.author, body="Body",
            status=Post.STATUS_PUBLISHED, view_count=2,
        )
        _login_and_verify(self.client, self.user, "s3cret-pw!")

    def test_anonymous_cannot_reach_analytics(self):
        self.client.logout()
        response = self.client.get(reverse("dashboard:analytics"))
        self.assertNotEqual(response.status_code, 200)

    def test_ranks_posts_by_view_count(self):
        response = self.client.get(reverse("dashboard:analytics"))
        self.assertEqual(response.status_code, 200)
        content = response.content.decode()
        self.assertLess(content.index("Popular post"), content.index("Quiet post"))

    def test_total_views_stat(self):
        response = self.client.get(reverse("dashboard:analytics"))
        self.assertEqual(response.context["total_views"], 52)
