from django.contrib.auth import get_user_model
from django.test import TestCase
from django.urls import reverse
from django.utils import translation

from .models import Post

User = get_user_model()


class PostBodySanitizationTests(TestCase):
    """post.body is rendered with |safe in blog/detail.html — sanitizing on
    save (not just trusting the TinyMCE editor's own toolbar) is the real
    defense against a compromised admin session posting stored XSS."""

    def setUp(self):
        self.author = User.objects.create_user(username="author", password="pw")

    def test_script_tags_and_event_handlers_are_stripped_on_save(self):
        post = Post.objects.create(
            title="Test post",
            author=self.author,
            body='<p>Hello</p><script>alert(1)</script><img src=x onerror=alert(2)>',
        )
        self.assertNotIn("<script", post.body)
        self.assertNotIn("onerror", post.body)
        self.assertIn("<p>Hello</p>", post.body)

    def test_both_language_fields_are_sanitized_regardless_of_active_language(self):
        """Regression test: `self.body = sanitize_html(self.body)` only ever
        touched whichever language modeltranslation currently has active,
        leaving the other language's raw HTML (including <script>) stored
        and later rendered with |safe — a live stored-XSS path caught by
        the round-2 QA re-audit. Both fa and ckb must be sanitized on every
        save, regardless of which language is active at save time."""
        translation.activate("fa")
        post = Post.objects.create(
            title="XSS regression check",
            author=self.author,
            body_fa='<p>سلام</p><script>alert("fa-xss")</script>',
            body_ckb='<p>سلاو</p><script>alert("ckb-xss")</script>',
        )
        post.refresh_from_db()
        self.assertNotIn("<script", post.body_fa)
        self.assertNotIn("<script", post.body_ckb)


class SearchVectorTests(TestCase):
    """Search vectors are stored per language (title/excerpt/body are
    separate fa/ckb columns under modeltranslation) — one language's search
    must never match content that only exists in the other."""

    def setUp(self):
        self.author = User.objects.create_user(username="author2", password="pw")
        self.post = Post.objects.create(
            title_fa="عنوان فارسی",
            title_ckb="ناونیشانی کوردی",
            body_fa="این یک آزمایش است",
            body_ckb="ئەمە تاقیکردنەوەیەکە",
            author=self.author,
            status=Post.STATUS_PUBLISHED,
        )

    def test_vectors_populated_on_save(self):
        self.post.refresh_from_db()
        self.assertIsNotNone(self.post.search_vector_fa)
        self.assertIsNotNone(self.post.search_vector_ckb)

    def test_fa_search_finds_fa_content_only(self):
        response = self.client.get(reverse("blog:list"), {"q": "آزمایش"})
        self.assertContains(response, "عنوان فارسی")

    def test_fa_search_does_not_match_ckb_only_content(self):
        response = self.client.get(reverse("blog:list"), {"q": "تاقیکردنەوەیەکە"})
        self.assertNotContains(response, "عنوان فارسی")


class ViewCountTests(TestCase):
    """view_count backs the Dashboard's analytics screen — it must increment
    once per real page view, and not double-count on a comment POST."""

    def setUp(self):
        self.author = User.objects.create_user(username="author3", password="pw")
        self.post = Post.objects.create(
            title="Counted post",
            slug="counted-post",
            author=self.author,
            body="Body",
            status=Post.STATUS_PUBLISHED,
        )

    def test_view_increments_on_get(self):
        self.client.get(reverse("blog:detail", kwargs={"slug": self.post.slug}))
        self.post.refresh_from_db()
        self.assertEqual(self.post.view_count, 1)

    def test_two_views_increment_twice(self):
        url = reverse("blog:detail", kwargs={"slug": self.post.slug})
        self.client.get(url)
        self.client.get(url)
        self.post.refresh_from_db()
        self.assertEqual(self.post.view_count, 2)
