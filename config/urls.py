from django.conf import settings
from django.conf.urls.i18n import i18n_patterns
from django.conf.urls.static import static
from django.contrib import admin
from django.contrib.sitemaps.views import sitemap
from django.urls import include, path
from django.views.generic import TemplateView

from apps.blog.sitemaps import PostSitemap
from apps.offerings.sitemaps import OfferingSitemap

sitemaps = {
    "posts": PostSitemap,
    "offerings": OfferingSitemap,
}

# Public-facing site is language-prefixed (/fa/..., /ckb/...) so content and
# UI strings switch together. Dashboard/admin stay unprefixed — the language
# switcher sets a cookie that LocaleMiddleware honors everywhere regardless.
urlpatterns = i18n_patterns(
    path("", include("apps.core.urls")),
    path("blog/", include("apps.blog.urls")),
    path("courses/", include("apps.offerings.urls")),
    path("testimonials/", include("apps.testimonials.urls")),
    path("free-resource/", include("apps.leads.urls")),
    path("", include("apps.pages.urls")),
    prefix_default_language=True,
)

urlpatterns += [
    path("i18n/", include("django.conf.urls.i18n")),
    path("dashboard/", include("apps.dashboard.urls")),
    path("admin/", admin.site.urls),
    path("sitemap.xml", sitemap, {"sitemaps": sitemaps}, name="django.contrib.sitemaps.views.sitemap"),
    path(
        "robots.txt",
        TemplateView.as_view(template_name="robots.txt", content_type="text/plain"),
        name="robots",
    ),
]

if settings.DEBUG:
    urlpatterns += static(settings.MEDIA_URL, document_root=settings.MEDIA_ROOT)
