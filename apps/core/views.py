from django.http import HttpResponse
from django.utils.translation import gettext as _
from django.views.generic import TemplateView

from apps.blog.models import Post
from apps.navigation.mixins import section_is_visible
from apps.offerings.models import Offering
from apps.testimonials.models import Testimonial

from .mixins import SeoContextMixin


class HomeView(SeoContextMixin, TemplateView):
    """Homepage: latest posts + offerings + testimonials, each section only
    shown once its NavItem has been toggled visible (phased rollout)."""

    template_name = "core/home.html"

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["latest_posts"] = (
            Post.objects.filter(status=Post.STATUS_PUBLISHED)
            .select_related("category")[:4]
        )
        context["offerings_visible"] = section_is_visible("offerings:list")
        if context["offerings_visible"]:
            context["offerings"] = Offering.objects.filter(
                status=Offering.STATUS_PUBLISHED
            )[:3]
        context["testimonials_visible"] = section_is_visible("testimonials:list")
        if context["testimonials_visible"]:
            context["testimonials"] = Testimonial.objects.filter(is_approved=True)[:3]
        context["leads_visible"] = section_is_visible("leads:list")
        return context


def ratelimited(request, exception):
    return HttpResponse(_("Too many requests. Please try again later."), status=429)
