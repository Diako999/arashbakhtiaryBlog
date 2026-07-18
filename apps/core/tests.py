from django.test import Client, TestCase, override_settings


class CsrfFailurePageTests(TestCase):
    """CSRF failures go through Django's separate CSRF_FAILURE_VIEW path, not
    the generic 403 handler — templates/403_csrf.html must exist and be
    fully self-contained (rendered with no request/context processors),
    otherwise this falls back to Django's stock English page."""

    def test_csrf_failure_renders_branded_persian_page(self):
        with override_settings(DEBUG=False, ALLOWED_HOSTS=["testserver"]):
            client = Client(enforce_csrf_checks=True)
            response = client.post("/dashboard/login/", {"username": "x", "password": "y"})
        self.assertEqual(response.status_code, 403)
        body = response.content.decode()
        self.assertNotIn("Forbidden", body)
        self.assertNotIn("CSRF verification failed. Request aborted.", body)
        self.assertIn("CSRF", body)  # Persian sentence still contains the literal token
