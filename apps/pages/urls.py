from django.urls import path

from . import views

app_name = "pages"

urlpatterns = [
    path("about/", views.FlatPageDetailView.as_view(), {"slug": "about"}, name="about"),
    path("contact/", views.FlatPageDetailView.as_view(), {"slug": "contact"}, name="contact"),
]
