from django.db import models
from django.urls import reverse
from django.utils.text import slugify
from django.utils.translation import gettext_lazy as _

from apps.core.models import SeoModelMixin, TimeStampedModel
from apps.core.validators import validate_image_file


class Offering(SeoModelMixin, TimeStampedModel):
    STATUS_DRAFT = "draft"
    STATUS_PUBLISHED = "published"
    STATUS_CHOICES = [
        (STATUS_DRAFT, _("Draft")),
        (STATUS_PUBLISHED, _("Published")),
    ]

    title = models.CharField(_("Title"), max_length=200)
    slug = models.SlugField(_("Slug"), max_length=220, unique=True, blank=True)
    summary = models.CharField(_("Summary"), max_length=300, blank=True)
    body = models.TextField(_("Body"), blank=True)
    cover_image = models.ImageField(
        _("Cover image"),
        upload_to="offerings/%Y/%m/",
        blank=True,
        null=True,
        validators=[validate_image_file],
    )
    price = models.DecimalField(
        _("Price"), max_digits=10, decimal_places=2, blank=True, null=True
    )
    status = models.CharField(
        _("Status"), max_length=10, choices=STATUS_CHOICES, default=STATUS_DRAFT
    )

    class Meta:
        verbose_name = _("offering")
        verbose_name_plural = _("offerings")
        ordering = ["-created_at"]

    def __str__(self):
        return self.title

    def save(self, *args, **kwargs):
        if not self.slug:
            base = slugify(self.title)[:200] or "offering"
            slug = base
            counter = 1
            while Offering.objects.filter(slug=slug).exclude(pk=self.pk).exists():
                counter += 1
                slug = f"{base}-{counter}"
            self.slug = slug
        super().save(*args, **kwargs)

    def get_absolute_url(self):
        return reverse("offerings:detail", kwargs={"slug": self.slug})

    @property
    def is_published(self):
        return self.status == self.STATUS_PUBLISHED


class Session(TimeStampedModel):
    offering = models.ForeignKey(
        Offering, on_delete=models.CASCADE, related_name="sessions", verbose_name=_("Offering")
    )
    starts_at = models.DateTimeField(_("Starts at"))
    ends_at = models.DateTimeField(_("Ends at"), blank=True, null=True)
    location = models.CharField(
        _("Location"), max_length=200, blank=True, help_text=_("Online link or venue address")
    )
    capacity = models.PositiveIntegerField(_("Capacity"), blank=True, null=True)

    class Meta:
        verbose_name = _("session")
        verbose_name_plural = _("sessions")
        ordering = ["starts_at"]

    def __str__(self):
        return f"{self.offering} — {self.starts_at:%Y-%m-%d}"


class Enrollment(TimeStampedModel):
    offering = models.ForeignKey(
        Offering, on_delete=models.CASCADE, related_name="enrollments", verbose_name=_("Offering")
    )
    session = models.ForeignKey(
        Session,
        on_delete=models.SET_NULL,
        blank=True,
        null=True,
        related_name="enrollments",
        verbose_name=_("Session"),
    )
    name = models.CharField(_("Name"), max_length=120)
    email = models.EmailField(_("Email"))
    phone = models.CharField(_("Phone"), max_length=40, blank=True)
    notes = models.TextField(_("Notes"), blank=True)

    class Meta:
        verbose_name = _("enrollment")
        verbose_name_plural = _("enrollments")
        ordering = ["-created_at"]

    def __str__(self):
        return f"{self.name} — {self.offering}"
