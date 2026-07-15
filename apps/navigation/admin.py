from django.contrib import admin
from modeltranslation.admin import TranslationAdmin

from .models import NavItem


@admin.register(NavItem)
class NavItemAdmin(TranslationAdmin):
    list_display = ("title", "url_name", "is_visible", "order", "parent")
    list_editable = ("is_visible", "order")
    list_filter = ("is_visible",)
