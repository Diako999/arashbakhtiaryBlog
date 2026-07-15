from django import forms
from tinymce.widgets import TinyMCE

from apps.blog.models import Category, Post


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
