from django.contrib import messages
from django.contrib.auth.decorators import login_required
from django.contrib.auth.mixins import LoginRequiredMixin
from django.contrib.auth.views import LoginView, LogoutView
from django.shortcuts import get_object_or_404, redirect
from django.urls import reverse_lazy
from django.utils.translation import gettext as _
from django.views.decorators.http import require_POST
from django.views.generic import CreateView, DeleteView, ListView, TemplateView, UpdateView

from apps.blog.models import Category, Post
from apps.navigation.models import NavItem

from .forms import CategoryForm, PostForm

DASHBOARD_LOGIN_URL = "dashboard:login"

# Sections a human can toggle from the Pages screen. Blog isn't listed here —
# it's the one section that's always live, not part of the phased rollout.
# The display label always comes from the NavItem's own (translated) title,
# not from this list — this is just a security whitelist of which url_names
# are toggleable from this screen.
TOGGLEABLE_SECTION_URL_NAMES = ["offerings:list", "testimonials:list", "leads:list"]


class DashboardLoginView(LoginView):
    template_name = "dashboard/login.html"
    redirect_authenticated_user = True

    def get_success_url(self):
        return reverse_lazy("dashboard:overview")


class DashboardLogoutView(LogoutView):
    next_page = "home"


class OverviewView(LoginRequiredMixin, TemplateView):
    login_url = DASHBOARD_LOGIN_URL
    template_name = "dashboard/overview.html"

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["draft_count"] = Post.objects.filter(status=Post.STATUS_DRAFT).count()
        context["published_count"] = Post.objects.filter(status=Post.STATUS_PUBLISHED).count()
        context["recent_posts"] = Post.objects.order_by("-created_at")[:5]
        return context


class PostDashboardListView(LoginRequiredMixin, ListView):
    login_url = DASHBOARD_LOGIN_URL
    template_name = "dashboard/content_list.html"
    context_object_name = "posts"
    paginate_by = 20

    def get_queryset(self):
        return Post.objects.select_related("category", "author").order_by("-created_at")

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["categories"] = Category.objects.all()
        return context


class PostCreateView(LoginRequiredMixin, CreateView):
    login_url = DASHBOARD_LOGIN_URL
    model = Post
    form_class = PostForm
    template_name = "dashboard/post_form.html"
    success_url = reverse_lazy("dashboard:content")

    def form_valid(self, form):
        form.instance.author = self.request.user
        messages.success(self.request, _("Post saved."))
        return super().form_valid(form)


class PostUpdateView(LoginRequiredMixin, UpdateView):
    login_url = DASHBOARD_LOGIN_URL
    model = Post
    form_class = PostForm
    template_name = "dashboard/post_form.html"
    success_url = reverse_lazy("dashboard:content")

    def form_valid(self, form):
        messages.success(self.request, _("Post updated."))
        return super().form_valid(form)


class PostDeleteView(LoginRequiredMixin, DeleteView):
    login_url = DASHBOARD_LOGIN_URL
    model = Post
    template_name = "dashboard/post_confirm_delete.html"
    success_url = reverse_lazy("dashboard:content")


class CategoryCreateView(LoginRequiredMixin, CreateView):
    login_url = DASHBOARD_LOGIN_URL
    model = Category
    form_class = CategoryForm
    template_name = "dashboard/category_form.html"
    success_url = reverse_lazy("dashboard:content")


class CategoryUpdateView(LoginRequiredMixin, UpdateView):
    login_url = DASHBOARD_LOGIN_URL
    model = Category
    form_class = CategoryForm
    template_name = "dashboard/category_form.html"
    success_url = reverse_lazy("dashboard:content")


class PagesVisibilityView(LoginRequiredMixin, TemplateView):
    login_url = DASHBOARD_LOGIN_URL
    template_name = "dashboard/pages.html"

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["sections"] = NavItem.objects.filter(
            url_name__in=TOGGLEABLE_SECTION_URL_NAMES
        ).order_by("order")
        return context


@login_required(login_url=DASHBOARD_LOGIN_URL)
@require_POST
def toggle_section_visibility(request, pk):
    item = get_object_or_404(NavItem, pk=pk, url_name__in=TOGGLEABLE_SECTION_URL_NAMES)
    item.is_visible = not item.is_visible
    item.save(update_fields=["is_visible"])
    state = _("Published") if item.is_visible else _("Hidden")
    messages.success(request, _("%(title)s is now %(state)s.") % {"title": item.title, "state": state})
    return redirect("dashboard:pages")
