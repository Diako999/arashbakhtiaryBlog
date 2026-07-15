from django import forms
from django.utils.translation import gettext_lazy as _
from tinymce.widgets import TinyMCE

from apps.blog.models import Category, Post
from apps.core.models import SiteSetting, ThemeConfig
from apps.leads.models import LeadMagnet
from apps.offerings.models import Offering, Session
from apps.testimonials.models import Testimonial


class PostForm(forms.ModelForm):
    class Meta:
        model = Post
        fields = [
            "slug",
            "category",
            "tags",
            "cover_image",
            "status",
            "published_at",
            "title_fa",
            "title_ckb",
            "excerpt_fa",
            "excerpt_ckb",
            "body_fa",
            "body_ckb",
            "meta_title_fa",
            "meta_title_ckb",
            "meta_description_fa",
            "meta_description_ckb",
        ]
        widgets = {
            "body_fa": TinyMCE(),
            "body_ckb": TinyMCE(),
            "published_at": forms.DateTimeInput(attrs={"type": "datetime-local"}),
        }


class CategoryForm(forms.ModelForm):
    class Meta:
        model = Category
        fields = ["slug", "name_fa", "name_ckb"]


class OfferingForm(forms.ModelForm):
    class Meta:
        model = Offering
        fields = [
            "slug",
            "cover_image",
            "price",
            "status",
            "title_fa",
            "title_ckb",
            "summary_fa",
            "summary_ckb",
            "body_fa",
            "body_ckb",
            "meta_title_fa",
            "meta_title_ckb",
            "meta_description_fa",
            "meta_description_ckb",
        ]
        widgets = {
            "body_fa": TinyMCE(),
            "body_ckb": TinyMCE(),
        }


SessionFormSet = forms.inlineformset_factory(
    Offering,
    Session,
    fields=["starts_at", "ends_at", "location", "capacity"],
    widgets={
        "starts_at": forms.DateTimeInput(attrs={"type": "datetime-local"}),
        "ends_at": forms.DateTimeInput(attrs={"type": "datetime-local"}),
    },
    extra=1,
    can_delete=True,
)


class LeadMagnetForm(forms.ModelForm):
    class Meta:
        model = LeadMagnet
        fields = [
            "slug",
            "cover_image",
            "file",
            "status",
            "title_fa",
            "title_ckb",
            "description_fa",
            "description_ckb",
            "meta_title_fa",
            "meta_title_ckb",
            "meta_description_fa",
            "meta_description_ckb",
        ]


class TestimonialForm(forms.ModelForm):
    class Meta:
        model = Testimonial
        fields = [
            "author_name",
            "photo",
            "video_url",
            "offering",
            "author_role_fa",
            "author_role_ckb",
            "quote_fa",
            "quote_ckb",
        ]


SOCIAL_LINK_FIELDS = [
    ("instagram", "instagram_url", _("Instagram")),
    ("telegram", "telegram_url", _("Telegram")),
    ("twitter", "twitter_url", _("Twitter / X")),
    ("linkedin", "linkedin_url", _("LinkedIn")),
    ("whatsapp", "whatsapp_url", _("WhatsApp")),
]


class SiteSettingForm(forms.ModelForm):
    instagram_url = forms.URLField(label=_("Instagram"), required=False)
    telegram_url = forms.URLField(label=_("Telegram"), required=False)
    twitter_url = forms.URLField(label=_("Twitter / X"), required=False)
    linkedin_url = forms.URLField(label=_("LinkedIn"), required=False)
    whatsapp_url = forms.URLField(label=_("WhatsApp"), required=False)

    class Meta:
        model = SiteSetting
        fields = ["site_name", "logo", "contact_email", "contact_phone", "meta_description"]

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        links = self.instance.social_links or {}
        for key, field_name, _label in SOCIAL_LINK_FIELDS:
            self.fields[field_name].initial = links.get(key, "")

    def save(self, commit=True):
        instance = super().save(commit=False)
        links = {}
        for key, field_name, _label in SOCIAL_LINK_FIELDS:
            value = self.cleaned_data.get(field_name)
            if value:
                links[key] = value
        instance.social_links = links
        if commit:
            instance.save()
        return instance


class ThemeConfigForm(forms.ModelForm):
    brand_color = forms.CharField(
        label=_("Brand color"), widget=forms.TextInput(attrs={"type": "color"})
    )
    accent_color = forms.CharField(
        label=_("Accent color"), widget=forms.TextInput(attrs={"type": "color"})
    )

    class Meta:
        model = ThemeConfig
        fields = ["default_mode"]

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        colors = self.instance.colors or {}
        self.fields["brand_color"].initial = colors.get("brand", "#0f9d8e")
        self.fields["accent_color"].initial = colors.get("accent", "#f0b429")

    def save(self, commit=True):
        instance = super().save(commit=False)
        instance.colors = {
            "brand": self.cleaned_data["brand_color"],
            "accent": self.cleaned_data["accent_color"],
        }
        if commit:
            instance.save()
        return instance
