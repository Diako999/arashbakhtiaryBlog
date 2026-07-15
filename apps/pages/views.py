from django.shortcuts import get_object_or_404
from django.views.generic import DetailView

from apps.core.mixins import SeoContextMixin
from apps.navigation.mixins import SectionVisibleRequiredMixin

from .models import FlatPage


class FlatPageDetailView(SectionVisibleRequiredMixin, SeoContextMixin, DetailView):
    # The whole pages app is one section — gated by a single NavItem, same
    # as offerings/testimonials/leads (see progress.md).
    visibility_url_name = "pages:about"
    template_name = "pages/detail.html"
    context_object_name = "page"

    def get_object(self, queryset=None):
        return get_object_or_404(FlatPage, slug=self.kwargs["slug"])
