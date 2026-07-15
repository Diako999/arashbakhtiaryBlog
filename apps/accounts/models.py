from django.conf import settings
from django.db import models
from django.utils.translation import gettext_lazy as _

from apps.core.models import TimeStampedModel


class Author(TimeStampedModel):
    user = models.OneToOneField(
        settings.AUTH_USER_MODEL,
        on_delete=models.CASCADE,
        related_name="author_profile",
        verbose_name=_("User"),
    )
    bio = models.TextField(_("Bio"), blank=True)
    avatar = models.ImageField(_("Avatar"), upload_to="authors/", blank=True, null=True)
    website = models.URLField(_("Website"), blank=True)

    class Meta:
        verbose_name = _("author")
        verbose_name_plural = _("authors")

    def __str__(self):
        return self.user.get_full_name() or self.user.username
