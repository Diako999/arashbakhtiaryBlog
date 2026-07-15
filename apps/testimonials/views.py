from django.views.generic import ListView

from apps.core.mixins import SeoContextMixin
from apps.navigation.mixins import SectionVisibleRequiredMixin

from .models import Testimonial


class TestimonialListView(SectionVisibleRequiredMixin, SeoContextMixin, ListView):
    visibility_url_name = "testimonials:list"
    template_name = "testimonials/list.html"
    context_object_name = "testimonials"

    def get_queryset(self):
        return Testimonial.objects.filter(is_approved=True)
