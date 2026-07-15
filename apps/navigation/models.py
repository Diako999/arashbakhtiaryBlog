from django.db import models
from django.urls import NoReverseMatch, reverse


class NavItem(models.Model):
    title = models.CharField(max_length=60)
    url_name = models.CharField(
        max_length=100,
        help_text="Named URL to resolve, e.g. 'blog:list'",
    )
    is_visible = models.BooleanField(
        default=False,
        help_text="The phased-rollout switch. Blog starts True, everything else False.",
    )
    order = models.PositiveIntegerField(default=0)
    parent = models.ForeignKey(
        "self",
        null=True,
        blank=True,
        related_name="children",
        on_delete=models.CASCADE,
    )

    class Meta:
        ordering = ["order"]

    def __str__(self):
        return self.title

    def get_url(self):
        try:
            return reverse(self.url_name)
        except NoReverseMatch:
            return "#"
