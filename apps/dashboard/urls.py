from django.urls import path

from . import otp_views, views

app_name = "dashboard"

urlpatterns = [
    path("login/", views.DashboardLoginView.as_view(), name="login"),
    path("logout/", views.DashboardLogoutView.as_view(), name="logout"),
    path("otp/setup/", otp_views.OTPSetupView.as_view(), name="otp_setup"),
    path("otp/verify/", otp_views.OTPVerifyView.as_view(), name="otp_verify"),
    path("", views.OverviewView.as_view(), name="overview"),
    path("content/", views.PostDashboardListView.as_view(), name="content"),
    path("content/new/", views.PostCreateView.as_view(), name="post_create"),
    path("content/<int:pk>/edit/", views.PostUpdateView.as_view(), name="post_edit"),
    path("content/<int:pk>/delete/", views.PostDeleteView.as_view(), name="post_delete"),
    path("content/categories/new/", views.CategoryCreateView.as_view(), name="category_create"),
    path("content/categories/<int:pk>/edit/", views.CategoryUpdateView.as_view(), name="category_edit"),
    path("pages/", views.PagesVisibilityView.as_view(), name="pages"),
    path("pages/toggle/<int:pk>/", views.toggle_section_visibility, name="pages_toggle"),
    # Offerings
    path("offerings/", views.OfferingDashboardListView.as_view(), name="offerings"),
    path("offerings/new/", views.OfferingCreateView.as_view(), name="offering_create"),
    path("offerings/<int:pk>/edit/", views.OfferingUpdateView.as_view(), name="offering_edit"),
    path("offerings/<int:pk>/delete/", views.OfferingDeleteView.as_view(), name="offering_delete"),
    # Leads
    path("leads/", views.LeadMagnetDashboardListView.as_view(), name="leads"),
    path("leads/new/", views.LeadMagnetCreateView.as_view(), name="leadmagnet_create"),
    path("leads/<int:pk>/edit/", views.LeadMagnetUpdateView.as_view(), name="leadmagnet_edit"),
    path("leads/<int:pk>/delete/", views.LeadMagnetDeleteView.as_view(), name="leadmagnet_delete"),
    path("leads/inbox/", views.SubmissionInboxView.as_view(), name="leads_inbox"),
    path("leads/inbox/<int:pk>/toggle/", views.toggle_submission_contacted, name="submission_toggle"),
    path("leads/inbox/export/", views.export_submissions_csv, name="submission_export"),
    # Testimonials
    path("testimonials/", views.TestimonialDashboardListView.as_view(), name="testimonials"),
    path("testimonials/new/", views.TestimonialCreateView.as_view(), name="testimonial_create"),
    path("testimonials/<int:pk>/edit/", views.TestimonialUpdateView.as_view(), name="testimonial_edit"),
    path(
        "testimonials/<int:pk>/delete/",
        views.TestimonialDeleteView.as_view(),
        name="testimonial_delete",
    ),
    path(
        "testimonials/<int:pk>/toggle/",
        views.toggle_testimonial_approved,
        name="testimonial_toggle",
    ),
    path(
        "testimonials/<int:pk>/move/<str:direction>/",
        views.move_testimonial,
        name="testimonial_move",
    ),
    # Settings
    path("settings/", views.SettingsView.as_view(), name="settings"),
]
