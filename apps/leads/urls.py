from django.urls import path

from . import views

app_name = "leads"

urlpatterns = [
    path("", views.LeadMagnetListView.as_view(), name="list"),
    path("<slug:slug>/", views.LeadMagnetDetailView.as_view(), name="detail"),
]
