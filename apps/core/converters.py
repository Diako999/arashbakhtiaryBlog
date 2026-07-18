class UnicodeSlugConverter:
    """Matches slugs produced by slugify(..., allow_unicode=True).

    Django's built-in `slug` converter is ASCII-only ([-a-zA-Z0-9_]+), but
    django-taggit slugifies tag names with allow_unicode=True, so a Persian
    or Kurdish tag name keeps its own characters as the slug. Needed
    wherever a tag_slug is used in a URL pattern.
    """

    regex = r"[-\w]+"

    def to_python(self, value):
        return value

    def to_url(self, value):
        return value
