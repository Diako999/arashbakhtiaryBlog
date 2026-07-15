from django.utils.decorators import method_decorator
from django.views.generic import DetailView, ListView
from django_ratelimit.decorators import ratelimit

from apps.core.mixins import SeoContextMixin
from apps.navigation.mixins import SectionVisibleRequiredMixin

from .forms import SubmissionForm
from .models import LeadMagnet


class LeadMagnetListView(SectionVisibleRequiredMixin, SeoContextMixin, ListView):
    visibility_url_name = "leads:list"
    template_name = "leads/list.html"
    context_object_name = "lead_magnets"

    def get_queryset(self):
        return LeadMagnet.objects.filter(status=LeadMagnet.STATUS_PUBLISHED)


class LeadMagnetDetailView(SectionVisibleRequiredMixin, SeoContextMixin, DetailView):
    visibility_url_name = "leads:list"
    template_name = "leads/detail.html"
    context_object_name = "lead_magnet"
    slug_url_kwarg = "slug"

    def get_queryset(self):
        return LeadMagnet.objects.filter(status=LeadMagnet.STATUS_PUBLISHED)

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context.setdefault("form", SubmissionForm())
        context.setdefault("submitted", False)
        return context

    @method_decorator(ratelimit(key="ip", rate="5/m", block=True))
    def post(self, request, *args, **kwargs):
        self.object = self.get_object()
        form = SubmissionForm(request.POST)
        if form.is_valid():
            submission = form.save(commit=False)
            submission.lead_magnet = self.object
            submission.save()
            context = self.get_context_data(form=SubmissionForm(), submitted=True)
            return self.render_to_response(context)
        context = self.get_context_data(form=form)
        return self.render_to_response(context)
