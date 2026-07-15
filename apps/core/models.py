from django.db import models


class TimeStampedModel(models.Model):
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    class Meta:
        abstract = True


class SeoModelMixin(models.Model):
    meta_title = models.CharField(max_length=70, blank=True)
    meta_description = models.CharField(max_length=160, blank=True)

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
        obj, _ = cls.objects.get_or_create(pk=1)
        return obj


class ThemeConfig(SingletonModel):
    colors = models.JSONField(
        default=dict,
        blank=True,
        help_text="Brand color tokens, e.g. {'brand': '#0f9d8e', 'accent': '#f0b429'}",
    )
    default_mode = models.CharField(
        max_length=5,
        choices=[("light", "Light"), ("dark", "Dark")],
        default="dark",
    )

    def __str__(self):
        return "Theme configuration"


class SiteSetting(SingletonModel):
    site_name = models.CharField(max_length=120, default="Prod Blog")
    logo = models.ImageField(upload_to="site/", blank=True, null=True)
    contact_email = models.EmailField(blank=True)
    contact_phone = models.CharField(max_length=40, blank=True)
    social_links = models.JSONField(
        default=dict,
        blank=True,
        help_text="e.g. {'instagram': 'https://...', 'telegram': 'https://...'}",
    )
    meta_description = models.CharField(max_length=300, blank=True)

    def __str__(self):
        return "Site settings"
