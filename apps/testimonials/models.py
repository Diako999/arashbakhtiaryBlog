from django.db import models
from django.utils.translation import gettext_lazy as _

from apps.core.models import TimeStampedModel
from apps.core.validators import validate_image_file
from apps.offerings.models import Offering


class Testimonial(TimeStampedModel):
    author_name = models.CharField(_("Author name"), max_length=120)
    author_role = models.CharField(_("Author role"), max_length=150, blank=True)
    quote = models.TextField(_("Quote"))
    photo = models.ImageField(
        _("Photo"),
        upload_to="testimonials/%Y/%m/",
        blank=True,
        null=True,
        validators=[validate_image_file],
    )
    video_url = models.URLField(_("Video URL"), blank=True)
    offering = models.ForeignKey(
        Offering,
        on_delete=models.SET_NULL,
        blank=True,
        null=True,
        related_name="testimonials",
        verbose_name=_("Offering"),
    )
    is_approved = models.BooleanField(_("Approved"), default=False)
    order = models.PositiveIntegerField(_("Order"), default=0)

    class Meta:
        verbose_name = _("testimonial")
        verbose_name_plural = _("testimonials")
        ordering = ["order", "-created_at"]

    def __str__(self):
        return self.author_name
