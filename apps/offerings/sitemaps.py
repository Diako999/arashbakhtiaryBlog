from django.contrib.sitemaps import Sitemap

from .models import Offering


class OfferingSitemap(Sitemap):
    changefreq = "weekly"
    priority = 0.6

    def items(self):
        return Offering.objects.filter(status=Offering.STATUS_PUBLISHED)

    def lastmod(self, obj):
        return obj.updated_at
