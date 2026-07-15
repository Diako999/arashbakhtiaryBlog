from django.http import Http404

from .models import NavItem


def section_is_visible(url_name):
    return NavItem.objects.filter(url_name=url_name, is_visible=True).exists()


class SectionVisibleRequiredMixin:
    """404s the view unless its NavItem (matched by url_name) is is_visible=True.

    This is the view-level half of the phased-rollout mechanism: hiding the
    nav link isn't enough, since someone could still guess the URL. Both
    checks read the same NavItem row, so there's a single flag to toggle
    rather than two that can drift out of sync.
    """

    visibility_url_name = None

    def dispatch(self, request, *args, **kwargs):
        if not section_is_visible(self.visibility_url_name):
            raise Http404()
        return super().dispatch(request, *args, **kwargs)
