from django.contrib.sitemaps import Sitemap

from apps.navigation.mixins import section_is_visible

from .models import LeadMagnet


class LeadMagnetSitemap(Sitemap):
    changefreq = "monthly"
    priority = 0.4

    def items(self):
        if not section_is_visible("leads:list"):
            return LeadMagnet.objects.none()
        return LeadMagnet.objects.filter(status=LeadMagnet.STATUS_PUBLISHED)

    def lastmod(self, obj):
        return obj.updated_at
