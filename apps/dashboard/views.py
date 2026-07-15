import csv

from django.contrib import messages
from django.contrib.auth.decorators import login_required
from django.contrib.auth.mixins import LoginRequiredMixin
from django.contrib.auth.views import LoginView, LogoutView
from django.http import HttpResponse
from django.shortcuts import get_object_or_404, redirect
from django.urls import reverse_lazy
from django.utils.translation import gettext as _
from django.views.decorators.http import require_POST
from django.views.generic import CreateView, DeleteView, ListView, TemplateView, UpdateView

from apps.blog.models import Category, Post
from apps.leads.models import LeadMagnet, Submission
from apps.navigation.models import NavItem
from apps.offerings.models import Offering
from apps.testimonials.models import Testimonial

from .forms import (
    CategoryForm,
    LeadMagnetForm,
    OfferingForm,
    PostForm,
    SessionFormSet,
    TestimonialForm,
)

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


# --- Offerings ---------------------------------------------------------


class OfferingDashboardListView(LoginRequiredMixin, ListView):
    login_url = DASHBOARD_LOGIN_URL
    template_name = "dashboard/offering_list.html"
    context_object_name = "offerings"

    def get_queryset(self):
        return Offering.objects.order_by("-created_at")


class OfferingCreateView(LoginRequiredMixin, CreateView):
    login_url = DASHBOARD_LOGIN_URL
    model = Offering
    form_class = OfferingForm
    template_name = "dashboard/offering_form.html"
    success_url = reverse_lazy("dashboard:offerings")

    def form_valid(self, form):
        messages.success(self.request, _("Offering saved."))
        return super().form_valid(form)


class OfferingUpdateView(LoginRequiredMixin, UpdateView):
    login_url = DASHBOARD_LOGIN_URL
    model = Offering
    form_class = OfferingForm
    template_name = "dashboard/offering_form.html"
    success_url = reverse_lazy("dashboard:offerings")

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        if self.request.method == "POST":
            context.setdefault("session_formset", SessionFormSet(self.request.POST, instance=self.object))
        else:
            context.setdefault("session_formset", SessionFormSet(instance=self.object))
        context["enrollments"] = self.object.enrollments.select_related("session").order_by("-created_at")
        return context

    def form_valid(self, form):
        context = self.get_context_data(form=form)
        session_formset = context["session_formset"]
        if session_formset.is_valid():
            response = super().form_valid(form)
            session_formset.instance = self.object
            session_formset.save()
            messages.success(self.request, _("Offering updated."))
            return response
        return self.render_to_response(self.get_context_data(form=form))


class OfferingDeleteView(LoginRequiredMixin, DeleteView):
    login_url = DASHBOARD_LOGIN_URL
    model = Offering
    template_name = "dashboard/offering_confirm_delete.html"
    success_url = reverse_lazy("dashboard:offerings")


# --- Leads --------------------------------------------------------------


class LeadMagnetDashboardListView(LoginRequiredMixin, ListView):
    login_url = DASHBOARD_LOGIN_URL
    template_name = "dashboard/leadmagnet_list.html"
    context_object_name = "lead_magnets"

    def get_queryset(self):
        return LeadMagnet.objects.order_by("-created_at")


class LeadMagnetCreateView(LoginRequiredMixin, CreateView):
    login_url = DASHBOARD_LOGIN_URL
    model = LeadMagnet
    form_class = LeadMagnetForm
    template_name = "dashboard/leadmagnet_form.html"
    success_url = reverse_lazy("dashboard:leads")

    def form_valid(self, form):
        messages.success(self.request, _("Lead magnet saved."))
        return super().form_valid(form)


class LeadMagnetUpdateView(LoginRequiredMixin, UpdateView):
    login_url = DASHBOARD_LOGIN_URL
    model = LeadMagnet
    form_class = LeadMagnetForm
    template_name = "dashboard/leadmagnet_form.html"
    success_url = reverse_lazy("dashboard:leads")

    def form_valid(self, form):
        messages.success(self.request, _("Lead magnet updated."))
        return super().form_valid(form)


class LeadMagnetDeleteView(LoginRequiredMixin, DeleteView):
    login_url = DASHBOARD_LOGIN_URL
    model = LeadMagnet
    template_name = "dashboard/leadmagnet_confirm_delete.html"
    success_url = reverse_lazy("dashboard:leads")


class SubmissionInboxView(LoginRequiredMixin, ListView):
    login_url = DASHBOARD_LOGIN_URL
    template_name = "dashboard/submission_list.html"
    context_object_name = "submissions"
    paginate_by = 30

    def get_queryset(self):
        return Submission.objects.select_related("lead_magnet").order_by("-created_at")


@login_required(login_url=DASHBOARD_LOGIN_URL)
@require_POST
def toggle_submission_contacted(request, pk):
    submission = get_object_or_404(Submission, pk=pk)
    submission.is_contacted = not submission.is_contacted
    submission.save(update_fields=["is_contacted"])
    return redirect("dashboard:leads_inbox")


@login_required(login_url=DASHBOARD_LOGIN_URL)
def export_submissions_csv(request):
    response = HttpResponse(content_type="text/csv")
    response["Content-Disposition"] = 'attachment; filename="submissions.csv"'
    writer = csv.writer(response)
    writer.writerow(["Name", "Email", "Lead magnet", "Contacted", "Submitted at"])
    for submission in Submission.objects.select_related("lead_magnet").order_by("-created_at"):
        writer.writerow(
            [
                submission.name,
                submission.email,
                submission.lead_magnet.title,
                submission.is_contacted,
                submission.created_at,
            ]
        )
    return response


# --- Testimonials ---------------------------------------------------------


class TestimonialDashboardListView(LoginRequiredMixin, ListView):
    login_url = DASHBOARD_LOGIN_URL
    template_name = "dashboard/testimonial_list.html"
    context_object_name = "testimonials"

    def get_queryset(self):
        return Testimonial.objects.order_by("order", "-created_at")


class TestimonialCreateView(LoginRequiredMixin, CreateView):
    login_url = DASHBOARD_LOGIN_URL
    model = Testimonial
    form_class = TestimonialForm
    template_name = "dashboard/testimonial_form.html"
    success_url = reverse_lazy("dashboard:testimonials")

    def form_valid(self, form):
        messages.success(self.request, _("Testimonial saved."))
        return super().form_valid(form)


class TestimonialUpdateView(LoginRequiredMixin, UpdateView):
    login_url = DASHBOARD_LOGIN_URL
    model = Testimonial
    form_class = TestimonialForm
    template_name = "dashboard/testimonial_form.html"
    success_url = reverse_lazy("dashboard:testimonials")

    def form_valid(self, form):
        messages.success(self.request, _("Testimonial updated."))
        return super().form_valid(form)


class TestimonialDeleteView(LoginRequiredMixin, DeleteView):
    login_url = DASHBOARD_LOGIN_URL
    model = Testimonial
    template_name = "dashboard/testimonial_confirm_delete.html"
    success_url = reverse_lazy("dashboard:testimonials")


@login_required(login_url=DASHBOARD_LOGIN_URL)
@require_POST
def toggle_testimonial_approved(request, pk):
    testimonial = get_object_or_404(Testimonial, pk=pk)
    testimonial.is_approved = not testimonial.is_approved
    testimonial.save(update_fields=["is_approved"])
    return redirect("dashboard:testimonials")


@login_required(login_url=DASHBOARD_LOGIN_URL)
@require_POST
def move_testimonial(request, pk, direction):
    ordered_ids = list(
        Testimonial.objects.order_by("order", "-created_at").values_list("pk", flat=True)
    )
    index = ordered_ids.index(int(pk))
    swap_index = index - 1 if direction == "up" else index + 1
    if 0 <= swap_index < len(ordered_ids):
        ordered_ids[index], ordered_ids[swap_index] = ordered_ids[swap_index], ordered_ids[index]
        # Renumber sequentially so `order` always reflects the real position,
        # even when everything started out tied at the default (0).
        for position, testimonial_id in enumerate(ordered_ids):
            Testimonial.objects.filter(pk=testimonial_id).update(order=position)
    return redirect("dashboard:testimonials")
