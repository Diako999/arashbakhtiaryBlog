from modeltranslation.translator import TranslationOptions, register

from .models import Testimonial


@register(Testimonial)
class TestimonialTranslationOptions(TranslationOptions):
    fields = ("author_role", "quote")
