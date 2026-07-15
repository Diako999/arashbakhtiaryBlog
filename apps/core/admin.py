from django.contrib import admin

from .models import SiteSetting, ThemeConfig


@admin.register(ThemeConfig)
class ThemeConfigAdmin(admin.ModelAdmin):
    def has_add_permission(self, request):
        return not ThemeConfig.objects.exists()


@admin.register(SiteSetting)
class SiteSettingAdmin(admin.ModelAdmin):
    def has_add_permission(self, request):
        return not SiteSetting.objects.exists()
