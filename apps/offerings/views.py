from django.contrib import messages
from django.shortcuts import redirect
from django.utils.decorators import method_decorator
from django.utils.translation import gettext as _
from django.views.generic import DetailView, ListView
from django_ratelimit.decorators import ratelimit

from apps.core.mixins import SeoContextMixin
from apps.navigation.mixins import SectionVisibleRequiredMixin

from .forms import EnrollmentForm
from .models import Offering


class OfferingListView(SectionVisibleRequiredMixin, SeoContextMixin, ListView):
    visibility_url_name = "offerings:list"
    template_name = "offerings/list.html"
    context_object_name = "offerings"

    def get_queryset(self):
        return Offering.objects.filter(status=Offering.STATUS_PUBLISHED)


class OfferingDetailView(SectionVisibleRequiredMixin, SeoContextMixin, DetailView):
    visibility_url_name = "offerings:list"
    template_name = "offerings/detail.html"
    context_object_name = "offering"
    slug_url_kwarg = "slug"

    def get_queryset(self):
        return Offering.objects.filter(status=Offering.STATUS_PUBLISHED)

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context.setdefault("form", EnrollmentForm(offering=self.object))
        return context

    @method_decorator(ratelimit(key="ip", rate="5/m", block=True))
    def post(self, request, *args, **kwargs):
        self.object = self.get_object()
        form = EnrollmentForm(request.POST, offering=self.object)
        if form.is_valid():
            enrollment = form.save(commit=False)
            enrollment.offering = self.object
            enrollment.save()
            messages.success(request, _("Thanks — we received your enrollment."))
            return redirect(self.object.get_absolute_url())
        context = self.get_context_data(form=form)
        return self.render_to_response(context)
