from django import forms
from tinymce.widgets import TinyMCE

from apps.blog.models import Category, Post
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
