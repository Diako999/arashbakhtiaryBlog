from django.contrib.sitemaps import Sitemap

from apps.navigation.mixins import section_is_visible

from .models import Offering


class OfferingSitemap(Sitemap):
    changefreq = "weekly"
    priority = 0.6

    def items(self):
        # Don't advertise offering URLs to search engines while the whole
        # section is still hidden — same gate the public views enforce.
        if not section_is_visible("offerings:list"):
            return Offering.objects.none()
        return Offering.objects.filter(status=Offering.STATUS_PUBLISHED)

    def lastmod(self, obj):
        return obj.updated_at
