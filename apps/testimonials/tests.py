from django.test import TestCase
from django.urls import reverse

from apps.navigation.models import NavItem


class TestimonialPhasedVisibilityTests(TestCase):
    def setUp(self):
        NavItem.objects.filter(url_name="testimonials:list").update(is_visible=False)

    def test_hidden_by_default(self):
        response = self.client.get(reverse("testimonials:list"))
        self.assertEqual(response.status_code, 404)

    def test_visible_after_toggle(self):
        NavItem.objects.filter(url_name="testimonials:list").update(is_visible=True)
        response = self.client.get(reverse("testimonials:list"))
        self.assertEqual(response.status_code, 200)
