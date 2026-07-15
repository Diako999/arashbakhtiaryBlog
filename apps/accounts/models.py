from django.conf import settings
from django.db import models

from apps.core.models import TimeStampedModel


class Author(TimeStampedModel):
    user = models.OneToOneField(
        settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name="author_profile"
    )
    bio = models.TextField(blank=True)
    avatar = models.ImageField(upload_to="authors/", blank=True, null=True)
    website = models.URLField(blank=True)

    def __str__(self):
        return self.user.get_full_name() or self.user.username
