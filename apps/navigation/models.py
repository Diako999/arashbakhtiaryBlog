from django.db import models
from django.urls import NoReverseMatch, reverse
from django.utils.translation import gettext_lazy as _


class NavItem(models.Model):
    title = models.CharField(_("Title"), max_length=60)
    url_name = models.CharField(
        _("URL name"),
        max_length=100,
        help_text=_("Named URL to resolve, e.g. 'blog:list'"),
    )
    is_visible = models.BooleanField(
        _("Visible"),
        default=False,
        help_text=_("The phased-rollout switch. Blog starts True, everything else False."),
    )
    order = models.PositiveIntegerField(_("Order"), default=0)
    parent = models.ForeignKey(
        "self",
        null=True,
        blank=True,
        related_name="children",
        on_delete=models.CASCADE,
        verbose_name=_("Parent"),
    )

    class Meta:
        verbose_name = _("navigation item")
        verbose_name_plural = _("navigation items")
        ordering = ["order"]

    def __str__(self):
        return self.title

    def get_url(self):
        try:
            return reverse(self.url_name)
        except NoReverseMatch:
            return "#"
