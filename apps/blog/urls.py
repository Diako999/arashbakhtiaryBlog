from django.urls import path, register_converter

from apps.core.converters import UnicodeSlugConverter

from . import views

register_converter(UnicodeSlugConverter, "unicode_slug")

app_name = "blog"

urlpatterns = [
    path("", views.PostListView.as_view(), name="list"),
    path("category/<slug:category_slug>/", views.PostListView.as_view(), name="list_by_category"),
    path("tag/<unicode_slug:tag_slug>/", views.PostListView.as_view(), name="list_by_tag"),
    path("<slug:slug>/", views.PostDetailView.as_view(), name="detail"),
]
