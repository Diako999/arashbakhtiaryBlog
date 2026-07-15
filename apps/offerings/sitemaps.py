from django.contrib.sitemaps import Sitemap


class OfferingSitemap(Sitemap):
    changefreq = "weekly"
    priority = 0.6

    def items(self):
        return []
