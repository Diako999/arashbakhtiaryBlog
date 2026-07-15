from django.urls import path

from . import views

app_name = "offerings"

urlpatterns = [
    path("", views.OfferingListView.as_view(), name="list"),
    path("<slug:slug>/", views.OfferingDetailView.as_view(), name="detail"),
]
