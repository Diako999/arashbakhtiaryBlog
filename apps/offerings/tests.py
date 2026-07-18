from django.test import TestCase
from django.urls import reverse

from apps.navigation.models import NavItem


class OfferingPhasedVisibilityTests(TestCase):
    """The offerings section must be 404 until its NavItem is toggled visible —
    the core phased-rollout guarantee this project is built around."""

    def setUp(self):
        NavItem.objects.filter(url_name="offerings:list").update(is_visible=False)

    def test_hidden_by_default(self):
        response = self.client.get(reverse("offerings:list"))
        self.assertEqual(response.status_code, 404)

    def test_visible_after_toggle(self):
        NavItem.objects.filter(url_name="offerings:list").update(is_visible=True)
        response = self.client.get(reverse("offerings:list"))
        self.assertEqual(response.status_code, 200)
