from django.contrib import admin
from modeltranslation.admin import TranslationAdmin

from .models import Category, Comment, Post


@admin.register(Category)
class CategoryAdmin(TranslationAdmin):
    list_display = ("name", "slug")
    prepopulated_fields = {"slug": ("name",)}


@admin.register(Post)
class PostAdmin(TranslationAdmin):
    list_display = ("title", "author", "category", "status", "published_at")
    list_filter = ("status", "category")
    search_fields = ("title", "excerpt", "body")


@admin.register(Comment)
class CommentAdmin(admin.ModelAdmin):
    list_display = ("post", "name", "is_approved", "created_at")
    list_filter = ("is_approved",)
