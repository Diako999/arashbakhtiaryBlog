from django.urls import path

from . import views

app_name = "dashboard"

urlpatterns = [
    path("login/", views.DashboardLoginView.as_view(), name="login"),
    path("logout/", views.DashboardLogoutView.as_view(), name="logout"),
    path("", views.OverviewView.as_view(), name="overview"),
    path("content/", views.PostDashboardListView.as_view(), name="content"),
    path("content/new/", views.PostCreateView.as_view(), name="post_create"),
    path("content/<int:pk>/edit/", views.PostUpdateView.as_view(), name="post_edit"),
    path("content/<int:pk>/delete/", views.PostDeleteView.as_view(), name="post_delete"),
    path("content/categories/new/", views.CategoryCreateView.as_view(), name="category_create"),
    path("content/categories/<int:pk>/edit/", views.CategoryUpdateView.as_view(), name="category_edit"),
    path("pages/", views.PagesVisibilityView.as_view(), name="pages"),
    path("pages/toggle/<int:pk>/", views.toggle_section_visibility, name="pages_toggle"),
]
