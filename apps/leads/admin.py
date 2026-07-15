from django.contrib import admin
from modeltranslation.admin import TranslationAdmin

from .models import LeadMagnet, Submission


@admin.register(LeadMagnet)
class LeadMagnetAdmin(TranslationAdmin):
    list_display = ("title", "status", "created_at")
    list_filter = ("status",)


@admin.register(Submission)
class SubmissionAdmin(admin.ModelAdmin):
    list_display = ("name", "email", "lead_magnet", "is_contacted", "created_at")
    list_filter = ("is_contacted", "lead_magnet")
