from modeltranslation.translator import TranslationOptions, register

from .models import LeadMagnet


@register(LeadMagnet)
class LeadMagnetTranslationOptions(TranslationOptions):
    fields = ("title", "description", "meta_title", "meta_description")
