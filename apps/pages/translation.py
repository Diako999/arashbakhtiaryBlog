from modeltranslation.translator import TranslationOptions, register

from .models import FlatPage


@register(FlatPage)
class FlatPageTranslationOptions(TranslationOptions):
    fields = ("title", "body", "meta_title", "meta_description")
