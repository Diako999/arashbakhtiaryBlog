from django.contrib import messages
from django.core.mail import EmailMessage
from django.shortcuts import get_object_or_404
from django.utils.decorators import method_decorator
from django.utils.translation import gettext as _
from django.views.generic import DetailView
from django_ratelimit.decorators import ratelimit

from apps.core.mixins import SeoContextMixin
from apps.core.models import SiteSetting
from apps.navigation.mixins import SectionVisibleRequiredMixin

from .forms import ContactForm
from .models import FlatPage


class FlatPageDetailView(SectionVisibleRequiredMixin, SeoContextMixin, DetailView):
    # The whole pages app is one section — gated by a single NavItem, same
    # as offerings/testimonials/leads (see progress.md).
    visibility_url_name = "pages:about"
    template_name = "pages/detail.html"
    context_object_name = "page"

    def get_object(self, queryset=None):
        return get_object_or_404(FlatPage, slug=self.kwargs["slug"])

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        if self.kwargs["slug"] == "contact":
            context.setdefault("contact_form", ContactForm())
        return context

    @method_decorator(ratelimit(key="ip", rate="5/m", block=True))
    def post(self, request, *args, **kwargs):
        self.object = self.get_object()
        if self.kwargs["slug"] != "contact":
            return self.get(request, *args, **kwargs)

        form = ContactForm(request.POST)
        if form.is_valid():
            site_settings = SiteSetting.load()
            if site_settings.contact_email:
                email = EmailMessage(
                    subject=f"[{site_settings.site_name}] {form.cleaned_data['name']}",
                    body=form.cleaned_data["message"],
                    to=[site_settings.contact_email],
                    reply_to=[form.cleaned_data["email"]],
                )
                email.send(fail_silently=True)
            messages.success(request, _("Thanks — your message has been sent."))
            return self.render_to_response(
                self.get_context_data(object=self.object, contact_form=ContactForm())
            )
        context = self.get_context_data(object=self.object, contact_form=form)
        return self.render_to_response(context)
