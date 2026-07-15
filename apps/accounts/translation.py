from modeltranslation.translator import TranslationOptions, register

from .models import Author


@register(Author)
class AuthorTranslationOptions(TranslationOptions):
    fields = ("bio",)
