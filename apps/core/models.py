from django.db import models
from django.utils.translation import gettext_lazy as _

from apps.core.validators import validate_image_file


class TimeStampedModel(models.Model):
    created_at = models.DateTimeField(_("Created at"), auto_now_add=True)
    updated_at = models.DateTimeField(_("Updated at"), auto_now=True)

    class Meta:
        abstract = True


class SeoModelMixin(models.Model):
    meta_title = models.CharField(_("Meta title"), max_length=70, blank=True)
    meta_description = models.CharField(_("Meta description"), max_length=160, blank=True)

    class Meta:
        abstract = True


class SingletonModel(models.Model):
    """Base for config models that only ever have one row (pk=1)."""

    class Meta:
        abstract = True

    def save(self, *args, **kwargs):
        self.pk = 1
        super().save(*args, **kwargs)

    def delete(self, *args, **kwargs):
        pass

    @classmethod
    def load(cls):
        obj, _created = cls.objects.get_or_create(pk=1)
        return obj


class ThemeConfig(SingletonModel):
    colors = models.JSONField(
        _("Colors"),
        default=dict,
        blank=True,
        help_text=_("Brand color tokens, e.g. {'brand': '#0f9d8e', 'accent': '#f0b429'}"),
    )
    default_mode = models.CharField(
        _("Default mode"),
        max_length=5,
        choices=[("light", _("Light")), ("dark", _("Dark"))],
        default="dark",
    )

    class Meta:
        verbose_name = _("theme configuration")
        verbose_name_plural = _("theme configuration")

    def __str__(self):
        return str(_("Theme configuration"))


class SiteSetting(SingletonModel):
    site_name = models.CharField(_("Site name"), max_length=120, default="وبلاگ")
    logo = models.ImageField(
        _("Logo"), upload_to="site/", blank=True, null=True, validators=[validate_image_file]
    )
    contact_email = models.EmailField(_("Contact email"), blank=True)
    contact_phone = models.CharField(_("Contact phone"), max_length=40, blank=True)
    social_links = models.JSONField(
        _("Social links"),
        default=dict,
        blank=True,
        help_text=_("e.g. {'instagram': 'https://...', 'telegram': 'https://...'}"),
    )
    meta_description = models.CharField(_("Meta description"), max_length=300, blank=True)

    class Meta:
        verbose_name = _("site settings")
        verbose_name_plural = _("site settings")

    def __str__(self):
        return str(_("Site settings"))
