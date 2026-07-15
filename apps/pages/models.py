from django.db import models
from django.urls import NoReverseMatch, reverse
from django.utils.translation import gettext_lazy as _

from apps.core.models import SeoModelMixin, TimeStampedModel


class FlatPage(SeoModelMixin, TimeStampedModel):
    title = models.CharField(_("Title"), max_length=200)
    slug = models.SlugField(
        _("Slug"),
        max_length=100,
        unique=True,
        help_text=_("Must match a routed page name, e.g. 'about' or 'contact'."),
    )
    body = models.TextField(_("Body"), blank=True)

    class Meta:
        verbose_name = _("page")
        verbose_name_plural = _("pages")
        ordering = ["title"]

    def __str__(self):
        return self.title

    def get_absolute_url(self):
        try:
            return reverse(f"pages:{self.slug}")
        except NoReverseMatch:
            return "#"
