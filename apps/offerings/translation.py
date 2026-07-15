from modeltranslation.translator import TranslationOptions, register

from .models import Offering


@register(Offering)
class OfferingTranslationOptions(TranslationOptions):
    fields = ("title", "summary", "body", "meta_title", "meta_description")
