from django.contrib import admin
from modeltranslation.admin import TranslationAdmin

from .models import Author


@admin.register(Author)
class AuthorAdmin(TranslationAdmin):
    list_display = ("user", "website")
