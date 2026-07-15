from modeltranslation.translator import TranslationOptions, register

from .models import NavItem


@register(NavItem)
class NavItemTranslationOptions(TranslationOptions):
    fields = ("title",)
