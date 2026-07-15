from django.contrib import admin
from modeltranslation.admin import TranslationAdmin

from .models import Enrollment, Offering, Session


class SessionInline(admin.TabularInline):
    model = Session
    extra = 0


class EnrollmentInline(admin.TabularInline):
    model = Enrollment
    extra = 0


@admin.register(Offering)
class OfferingAdmin(TranslationAdmin):
    list_display = ("title", "status", "price", "created_at")
    list_filter = ("status",)
    inlines = [SessionInline, EnrollmentInline]


@admin.register(Session)
class SessionAdmin(admin.ModelAdmin):
    list_display = ("offering", "starts_at", "location", "capacity")


@admin.register(Enrollment)
class EnrollmentAdmin(admin.ModelAdmin):
    list_display = ("name", "offering", "session", "created_at")
