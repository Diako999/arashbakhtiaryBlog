from django.http import HttpResponse
from django.views.generic import TemplateView

from apps.blog.models import Post

from .mixins import SeoContextMixin


class HomeView(SeoContextMixin, TemplateView):
    """Homepage: latest posts + featured offering (offering shown only if
    the offerings section is published)."""

    template_name = "core/home.html"

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["latest_posts"] = (
            Post.objects.filter(status=Post.STATUS_PUBLISHED)
            .select_related("category")[:6]
        )
        return context


def ratelimited(request, exception):
    return HttpResponse("Too many requests. Please try again later.", status=429)
