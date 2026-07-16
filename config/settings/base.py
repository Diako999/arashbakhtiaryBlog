"""
Shared settings for all environments. dev.py / prod.py import * from here
and override what differs.
"""
from pathlib import Path

import environ

BASE_DIR = Path(__file__).resolve().parent.parent.parent

env = environ.Env(
    DEBUG=(bool, False),
)
environ.Env.read_env(BASE_DIR / ".env")

SECRET_KEY = env("SECRET_KEY")

DEBUG = env("DEBUG")

ALLOWED_HOSTS = env.list("ALLOWED_HOSTS", default=[])

# Stock django.contrib.admin stays installed as a technical fallback (per
# the tech stack doc) but is moved off /admin/. Set a unique ADMIN_URL in
# production's .env — this default is only safe for local dev.
ADMIN_URL = env("ADMIN_URL", default="manage-portal/")


# Application definition

INSTALLED_APPS = [
    # modeltranslation must load before contrib.admin
    "modeltranslation",
    "django.contrib.admin",
    "django.contrib.auth",
    "django.contrib.contenttypes",
    "django.contrib.sessions",
    "django.contrib.messages",
    "django.contrib.staticfiles",
    "django.contrib.sitemaps",
    "django.contrib.humanize",
    # third-party
    "django_otp",
    "django_otp.plugins.otp_totp",
    "django_otp.plugins.otp_static",
    "taggit",
    "tinymce",
    "widget_tweaks",
    # local apps
    "apps.core",
    "apps.navigation",
    "apps.accounts",
    "apps.blog",
    "apps.offerings",
    "apps.testimonials",
    "apps.leads",
    "apps.pages",
    "apps.dashboard",
]

MIDDLEWARE = [
    "django.middleware.security.SecurityMiddleware",
    "whitenoise.middleware.WhiteNoiseMiddleware",
    "django.contrib.sessions.middleware.SessionMiddleware",
    "django.middleware.locale.LocaleMiddleware",
    "django.middleware.common.CommonMiddleware",
    "django.middleware.csrf.CsrfViewMiddleware",
    "django.contrib.auth.middleware.AuthenticationMiddleware",
    "django_otp.middleware.OTPMiddleware",
    "django.contrib.messages.middleware.MessageMiddleware",
    "django.middleware.clickjacking.XFrameOptionsMiddleware",
]

ROOT_URLCONF = "config.urls"

TEMPLATES = [
    {
        "BACKEND": "django.template.backends.django.DjangoTemplates",
        "DIRS": [BASE_DIR / "templates"],
        "APP_DIRS": True,
        "OPTIONS": {
            "context_processors": [
                "django.template.context_processors.debug",
                "django.template.context_processors.request",
                "django.contrib.auth.context_processors.auth",
                "django.contrib.messages.context_processors.messages",
                "django.template.context_processors.i18n",
                "apps.core.context_processors.theme",
                "apps.core.context_processors.site_settings",
                "apps.navigation.context_processors.nav_items",
                "apps.dashboard.context_processors.dashboard_nav",
            ],
        },
    },
]

WSGI_APPLICATION = "config.wsgi.application"


# Database
DATABASES = {
    "default": env.db("DATABASE_URL"),
}


# Password validation
AUTH_PASSWORD_VALIDATORS = [
    {"NAME": "django.contrib.auth.password_validation.UserAttributeSimilarityValidator"},
    {"NAME": "django.contrib.auth.password_validation.MinimumLengthValidator"},
    {"NAME": "django.contrib.auth.password_validation.CommonPasswordValidator"},
    {"NAME": "django.contrib.auth.password_validation.NumericPasswordValidator"},
]


# Internationalization — Persian (default) + Kurdish Sorani, both RTL
LANGUAGE_CODE = "fa"

LANGUAGES = [
    ("fa", "فارسی"),
    ("ckb", "کوردی"),
]

LOCALE_PATHS = [BASE_DIR / "locale"]

MODELTRANSLATION_DEFAULT_LANGUAGE = "fa"
MODELTRANSLATION_LANGUAGES = ("fa", "ckb")
MODELTRANSLATION_FALLBACK_LANGUAGES = {"default": ("fa", "ckb")}

TIME_ZONE = "Asia/Tehran"

USE_I18N = True

USE_TZ = True


# Static files
STATIC_URL = "static/"
STATICFILES_DIRS = [BASE_DIR / "static"]
STATIC_ROOT = BASE_DIR / "staticfiles"

STORAGES = {
    "default": {
        "BACKEND": "django.core.files.storage.FileSystemStorage",
    },
    "staticfiles": {
        "BACKEND": "whitenoise.storage.CompressedManifestStaticFilesStorage",
    },
}

# Media files (uploads)
MEDIA_URL = "media/"
MEDIA_ROOT = BASE_DIR / "media"

DEFAULT_AUTO_FIELD = "django.db.models.BigAutoField"

AUTHENTICATION_BACKENDS = [
    "django.contrib.auth.backends.ModelBackend",
]

LOGIN_URL = "dashboard:login"
LOGIN_REDIRECT_URL = "dashboard:overview"
LOGOUT_REDIRECT_URL = "home"

# django-tinymce — content is Persian/Kurdish (Sorani), both RTL
TINYMCE_DEFAULT_CONFIG = {
    "height": 400,
    "width": "100%",
    "directionality": "rtl",
    "promotion": False,  # hides the "Get all features" upsell banner
    "branding": False,
    "menubar": "file edit view insert format tools table",
    "plugins": "advlist autolink lists link image charmap preview anchor "
    "searchreplace visualblocks code fullscreen insertdatetime media "
    "table paste code help wordcount directionality",
    "toolbar": "undo redo | formatselect | bold italic backcolor | "
    "alignleft aligncenter alignright alignjustify | "
    "bullist numlist outdent indent | ltr rtl | removeformat | help",
}

# django-ratelimit
RATELIMIT_VIEW = "apps.core.views.ratelimited"

# File upload validation (size/MIME) — enforced in forms; caps set here for reference
MAX_UPLOAD_SIZE_MB = 5
ALLOWED_UPLOAD_IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"]
ALLOWED_UPLOAD_DOCUMENT_TYPES = ["application/pdf"]
