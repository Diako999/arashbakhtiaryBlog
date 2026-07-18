from django.test import TestCase
from django.urls import reverse

from apps.navigation.models import NavItem

from .models import FlatPage


class FlatPagePhasedVisibilityTests(TestCase):
    def setUp(self):
        NavItem.objects.filter(url_name="pages:about").update(is_visible=False)
        FlatPage.objects.create(title="About", slug="about", body="About us.")

    def test_hidden_by_default(self):
        response = self.client.get(reverse("pages:about"))
        self.assertEqual(response.status_code, 404)

    def test_visible_after_toggle(self):
        NavItem.objects.filter(url_name="pages:about").update(is_visible=True)
        response = self.client.get(reverse("pages:about"))
        self.assertEqual(response.status_code, 200)
