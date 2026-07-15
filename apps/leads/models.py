from django.db import models
from django.urls import reverse
from django.utils.text import slugify
from django.utils.translation import gettext_lazy as _

from apps.core.models import SeoModelMixin, TimeStampedModel


def validate_upload_size(value):
    from django.conf import settings
    from django.core.exceptions import ValidationError

    max_bytes = settings.MAX_UPLOAD_SIZE_MB * 1024 * 1024
    if value.size > max_bytes:
        raise ValidationError(
            _("File too large. Max size is %(max)s MB.") % {"max": settings.MAX_UPLOAD_SIZE_MB}
        )


class LeadMagnet(SeoModelMixin, TimeStampedModel):
    STATUS_DRAFT = "draft"
    STATUS_PUBLISHED = "published"
    STATUS_CHOICES = [
        (STATUS_DRAFT, _("Draft")),
        (STATUS_PUBLISHED, _("Published")),
    ]

    title = models.CharField(_("Title"), max_length=200)
    slug = models.SlugField(_("Slug"), max_length=220, unique=True, blank=True)
    description = models.TextField(_("Description"), blank=True)
    cover_image = models.ImageField(
        _("Cover image"), upload_to="leads/%Y/%m/", blank=True, null=True
    )
    file = models.FileField(
        _("File"), upload_to="leads/files/%Y/%m/", validators=[validate_upload_size]
    )
    status = models.CharField(
        _("Status"), max_length=10, choices=STATUS_CHOICES, default=STATUS_DRAFT
    )

    class Meta:
        verbose_name = _("lead magnet")
        verbose_name_plural = _("lead magnets")
        ordering = ["-created_at"]

    def __str__(self):
        return self.title

    def save(self, *args, **kwargs):
        if not self.slug:
            base = slugify(self.title)[:200] or "resource"
            slug = base
            counter = 1
            while LeadMagnet.objects.filter(slug=slug).exclude(pk=self.pk).exists():
                counter += 1
                slug = f"{base}-{counter}"
            self.slug = slug
        super().save(*args, **kwargs)

    def get_absolute_url(self):
        return reverse("leads:detail", kwargs={"slug": self.slug})

    @property
    def is_published(self):
        return self.status == self.STATUS_PUBLISHED


class Submission(TimeStampedModel):
    lead_magnet = models.ForeignKey(
        LeadMagnet,
        on_delete=models.CASCADE,
        related_name="submissions",
        verbose_name=_("Lead magnet"),
    )
    name = models.CharField(_("Name"), max_length=120)
    email = models.EmailField(_("Email"))
    is_contacted = models.BooleanField(_("Contacted"), default=False)

    class Meta:
        verbose_name = _("submission")
        verbose_name_plural = _("submissions")
        ordering = ["-created_at"]

    def __str__(self):
        return f"{self.name} — {self.lead_magnet}"
