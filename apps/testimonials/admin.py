from django.contrib import admin
from modeltranslation.admin import TranslationAdmin

from .models import Testimonial


@admin.register(Testimonial)
class TestimonialAdmin(TranslationAdmin):
    list_display = ("author_name", "offering", "is_approved", "order")
    list_editable = ("order",)
    list_filter = ("is_approved",)
