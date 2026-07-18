import base64
from functools import wraps
from io import BytesIO

import qrcode
from django import forms
from django.contrib import messages
from django.contrib.auth.decorators import login_required
from django.contrib.auth.mixins import LoginRequiredMixin
from django.contrib.auth.views import redirect_to_login
from django.shortcuts import redirect
from django.utils.translation import gettext as _
from django.views.generic import TemplateView
from django_otp import login as otp_login
from django_otp.plugins.otp_totp.models import TOTPDevice

DASHBOARD_LOGIN_URL = "dashboard:login"


def _confirmed_device(user):
    return TOTPDevice.objects.filter(user=user, confirmed=True).first()


class TOTPTokenForm(forms.Form):
    token = forms.CharField(label=_("Authenticator code"), max_length=6)


class OTPRequiredMixin:
    """Gates a dashboard view behind login *and* a verified TOTP device.

    This is the 2FA half of "Custom Dashboard gated behind login + 2FA"
    from the tech stack doc. Plain LoginRequiredMixin only checks
    authentication; this also redirects to enroll/verify a TOTP device
    before letting the request through to the view.
    """

    login_url = DASHBOARD_LOGIN_URL

    def dispatch(self, request, *args, **kwargs):
        if not request.user.is_authenticated:
            return redirect_to_login(request.get_full_path(), self.login_url)
        if not request.user.is_verified():
            if _confirmed_device(request.user):
                return redirect("dashboard:otp_verify")
            return redirect("dashboard:otp_setup")
        return super().dispatch(request, *args, **kwargs)


def dashboard_login_required(view_func):
    """Function-view equivalent of OTPRequiredMixin."""

    @wraps(view_func)
    @login_required(login_url=DASHBOARD_LOGIN_URL)
    def wrapped(request, *args, **kwargs):
        if not request.user.is_verified():
            if _confirmed_device(request.user):
                return redirect("dashboard:otp_verify")
            return redirect("dashboard:otp_setup")
        return view_func(request, *args, **kwargs)

    return wrapped


class OTPSetupView(LoginRequiredMixin, TemplateView):
    """Enroll a new TOTP device — shown once, the first time a user reaches
    the dashboard with no confirmed device yet."""

    login_url = DASHBOARD_LOGIN_URL
    template_name = "dashboard/otp_setup.html"

    def dispatch(self, request, *args, **kwargs):
        if request.user.is_authenticated and _confirmed_device(request.user):
            return redirect("dashboard:otp_verify")
        return super().dispatch(request, *args, **kwargs)

    def get_device(self):
        device, _created = TOTPDevice.objects.get_or_create(
            user=self.request.user, confirmed=False, defaults={"name": "default"}
        )
        return device

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        device = self.get_device()
        qr_img = qrcode.make(device.config_url)
        buf = BytesIO()
        qr_img.save(buf, format="PNG")
        context["qr_data_uri"] = "data:image/png;base64," + base64.b64encode(
            buf.getvalue()
        ).decode()
        context["secret_key"] = base64.b32encode(device.bin_key).decode()
        context.setdefault("form", TOTPTokenForm())
        return context

    def post(self, request, *args, **kwargs):
        form = TOTPTokenForm(request.POST)
        device = self.get_device()
        if form.is_valid() and device.verify_token(form.cleaned_data["token"]):
            device.confirmed = True
            device.save()
            otp_login(request, device)
            messages.success(request, _("Two-factor authentication enabled."))
            return redirect("dashboard:overview")
        messages.error(request, _("Invalid code. Please try again."))
        return self.render_to_response(self.get_context_data(form=form))


class OTPVerifyView(LoginRequiredMixin, TemplateView):
    """Prompt for a TOTP code — shown every new session once a device is
    already confirmed."""

    login_url = DASHBOARD_LOGIN_URL
    template_name = "dashboard/otp_verify.html"

    def dispatch(self, request, *args, **kwargs):
        if request.user.is_authenticated and not _confirmed_device(request.user):
            return redirect("dashboard:otp_setup")
        return super().dispatch(request, *args, **kwargs)

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context.setdefault("form", TOTPTokenForm())
        return context

    def post(self, request, *args, **kwargs):
        form = TOTPTokenForm(request.POST)
        device = _confirmed_device(request.user)
        if form.is_valid() and device and device.verify_token(form.cleaned_data["token"]):
            otp_login(request, device)
            return redirect("dashboard:overview")
        messages.error(request, _("Invalid code. Please try again."))
        return self.render_to_response(self.get_context_data(form=form))
