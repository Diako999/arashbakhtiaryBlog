from django.urls import reverse
from django.utils.translation import gettext_lazy as _

# Maps every dashboard url_name to the nav item it should highlight as active.
_ACTIVE_MAP = {
    "overview": "overview",
    "analytics": "analytics",
    "content": "content",
    "post_create": "content",
    "post_edit": "content",
    "post_delete": "content",
    "category_create": "content",
    "category_edit": "content",
    "offerings": "offerings",
    "offering_create": "offerings",
    "offering_edit": "offerings",
    "offering_delete": "offerings",
    "testimonials": "testimonials",
    "testimonial_create": "testimonials",
    "testimonial_edit": "testimonials",
    "testimonial_delete": "testimonials",
    "comments": "comments",
    "comment_delete": "comments",
    "pages": "pages",
    "leads": "leads",
    "leadmagnet_create": "leads",
    "leadmagnet_edit": "leads",
    "leadmagnet_delete": "leads",
    "leads_inbox": "leads",
    "settings": "settings",
}


def dashboard_nav(request):
    if not request.resolver_match or request.resolver_match.namespace != "dashboard":
        return {}

    groups = [
        {
            "label": _("General"),
            "items": [
                {"id": "overview", "icon": "◇", "label": _("Overview"), "url": reverse("dashboard:overview")},
                {"id": "analytics", "icon": "📊", "label": _("Analytics"), "url": reverse("dashboard:analytics")},
            ],
        },
        {
            "label": _("Content"),
            "items": [
                {"id": "content", "icon": "▤", "label": _("Posts & categories"), "url": reverse("dashboard:content")},
                {"id": "offerings", "icon": "◈", "label": _("Offerings & sessions"), "url": reverse("dashboard:offerings")},
                {"id": "testimonials", "icon": "❝", "label": _("Testimonials"), "url": reverse("dashboard:testimonials")},
                {"id": "comments", "icon": "💬", "label": _("Comments"), "url": reverse("dashboard:comments")},
                {"id": "pages", "icon": "▥", "label": _("Site pages"), "url": reverse("dashboard:pages")},
            ],
        },
        {
            "label": _("Audience"),
            "items": [
                {"id": "leads", "icon": "✉", "label": _("Submissions inbox"), "url": reverse("dashboard:leads_inbox")},
            ],
        },
        {
            "label": _("Settings"),
            "items": [
                {"id": "settings", "icon": "⚙", "label": _("Settings"), "url": reverse("dashboard:settings")},
            ],
        },
    ]

    return {
        "dash_nav_groups": groups,
        "dash_active": _ACTIVE_MAP.get(request.resolver_match.url_name, ""),
    }
