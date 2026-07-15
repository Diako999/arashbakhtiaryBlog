from django.contrib import admin
from modeltranslation.admin import TranslationAdmin

from .models import FlatPage


@admin.register(FlatPage)
class FlatPageAdmin(TranslationAdmin):
    list_display = ("title", "slug")
