from django.db.models import Prefetch

from .models import NavItem


def nav_items(request):
    visible_children = NavItem.objects.filter(is_visible=True).order_by("order")
    items = (
        NavItem.objects.filter(is_visible=True, parent__isnull=True)
        .order_by("order")
        .prefetch_related(Prefetch("children", queryset=visible_children))
    )
    return {"nav_items": items}
