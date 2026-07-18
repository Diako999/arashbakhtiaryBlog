from django.contrib import messages
from django.db.models import F
from django.utils.decorators import method_decorator
from django.utils.translation import get_language, gettext as _
from django.views.generic import DetailView, ListView
from django_ratelimit.decorators import ratelimit

from apps.core.mixins import SeoContextMixin

from .forms import CommentForm
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
            # Stored, GIN-indexed vector (populated on save) — one per
            # language, since title/excerpt/body are separate fa/ckb columns.
            qs = qs.filter(**{f"search_vector_{get_language()}": query})
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

    def get(self, request, *args, **kwargs):
        response = super().get(request, *args, **kwargs)
        Post.objects.filter(pk=self.object.pk).update(view_count=F("view_count") + 1)
        return response

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["comments"] = self.object.comments.filter(is_approved=True).order_by(
            "created_at"
        )
        context.setdefault("comment_form", CommentForm())
        return context

    @method_decorator(ratelimit(key="ip", rate="5/m", block=True))
    def post(self, request, *args, **kwargs):
        self.object = self.get_object()
        form = CommentForm(request.POST)
        if form.is_valid():
            comment = form.save(commit=False)
            comment.post = self.object
            comment.save()
            messages.success(
                request, _("Thanks — your comment will appear once it's approved.")
            )
            return self.render_to_response(
                self.get_context_data(object=self.object, comment_form=CommentForm())
            )
        context = self.get_context_data(object=self.object, comment_form=form)
        return self.render_to_response(context)
