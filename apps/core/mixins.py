class SeoContextMixin:
    """Adds seo_title/seo_description/seo_image to template context.

    Falls back to self.object's meta_title/meta_description/cover_image (if
    the view has a detail object with those fields) then to
    seo_title/seo_description attributes set on the view class.
    """

    seo_title = None
    seo_description = None

    def get_seo_title(self):
        obj = getattr(self, "object", None)
        if obj is not None and getattr(obj, "meta_title", ""):
            return obj.meta_title
        return self.seo_title

    def get_seo_description(self):
        obj = getattr(self, "object", None)
        if obj is not None and getattr(obj, "meta_description", ""):
            return obj.meta_description
        return self.seo_description

    def get_seo_image(self):
        obj = getattr(self, "object", None)
        image = getattr(obj, "cover_image", None) or getattr(obj, "photo", None)
        return image if image else None

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["seo_title"] = self.get_seo_title()
        context["seo_description"] = self.get_seo_description()
        context["seo_image"] = self.get_seo_image()
        return context
