from django.contrib.postgres.search import SearchVector
from django.views.generic import DetailView, ListView

from apps.core.mixins import SeoContextMixin

from .models import Category, Post


class PublishedPostQuerysetMixin:
    def get_base_queryset(self):
        return (
            Post.objects.filter(status=Post.STATUS_PUBLISHED)
            .select_related("author", "category")
            .prefetch_related("tags")
        )


class PostListView(PublishedPostQuerysetMixin, SeoContextMixin, ListView):
    template_name = "blog/list.html"
    context_object_name = "posts"
    paginate_by = 10

    def get_queryset(self):
        qs = self.get_base_queryset()
        category_slug = self.kwargs.get("category_slug")
        if category_slug:
            qs = qs.filter(category__slug=category_slug)
        tag_slug = self.kwargs.get("tag_slug")
        if tag_slug:
            qs = qs.filter(tags__slug=tag_slug)
        query = self.request.GET.get("q")
        if query:
            qs = qs.annotate(search=SearchVector("title", "excerpt", "body")).filter(
                search=query
            )
        return qs.distinct()

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["categories"] = Category.objects.all()
        context["current_category"] = self.kwargs.get("category_slug")
        context["current_tag"] = self.kwargs.get("tag_slug")
        context["query"] = self.request.GET.get("q", "")
        return context


class PostDetailView(PublishedPostQuerysetMixin, SeoContextMixin, DetailView):
    template_name = "blog/detail.html"
    context_object_name = "post"
    slug_url_kwarg = "slug"

    def get_queryset(self):
        return self.get_base_queryset()
