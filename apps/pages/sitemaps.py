from django.contrib.sitemaps import Sitemap

from apps.navigation.mixins import section_is_visible

from .models import FlatPage


class FlatPageSitemap(Sitemap):
    changefreq = "yearly"
    priority = 0.3

    def items(self):
        if not section_is_visible("pages:about"):
            return FlatPage.objects.none()
        return FlatPage.objects.all()

    def location(self, obj):
        return obj.get_absolute_url()

    def lastmod(self, obj):
        return obj.updated_at
