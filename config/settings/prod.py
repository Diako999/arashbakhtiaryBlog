from .base import *  # noqa: F401,F403
from .base import env

DEBUG = False

ALLOWED_HOSTS = env.list("ALLOWED_HOSTS")

# Defaults to https://<host> for each ALLOWED_HOSTS entry (this proxy always
# terminates TLS — see SECURE_PROXY_SSL_HEADER below) so a first deploy isn't
# immediately hit with "CSRF verification failed" on every POST. Override via
# env if the public origin(s) differ from ALLOWED_HOSTS (e.g. a CDN domain).
CSRF_TRUSTED_ORIGINS = env.list(
    "CSRF_TRUSTED_ORIGINS",
    default=[f"https://{host}" for host in ALLOWED_HOSTS],
)

# --- Security stack (Production_Blog_Tech_Stack.md → Security stack) ---
SECURE_SSL_REDIRECT = True
SESSION_COOKIE_SECURE = True
CSRF_COOKIE_SECURE = True
SECURE_HSTS_SECONDS = 31536000
SECURE_HSTS_INCLUDE_SUBDOMAINS = True
SECURE_HSTS_PRELOAD = True
SECURE_CONTENT_TYPE_NOSNIFF = True
SESSION_COOKIE_HTTPONLY = True
CSRF_COOKIE_HTTPONLY = True
X_FRAME_OPTIONS = "DENY"

# Host's reverse proxy terminates TLS and forwards this header
SECURE_PROXY_SSL_HEADER = ("HTTP_X_FORWARDED_PROTO", "https")

EMAIL_BACKEND = "django.core.mail.backends.smtp.EmailBackend"
EMAIL_HOST = env("EMAIL_HOST", default="")
EMAIL_PORT = env.int("EMAIL_PORT", default=587)
EMAIL_HOST_USER = env("EMAIL_HOST_USER", default="")
EMAIL_HOST_PASSWORD = env("EMAIL_HOST_PASSWORD", default="")
EMAIL_USE_TLS = True

# Email ADMINS (set via the ADMINS env var, "Name:email,Name:email") on every
# unhandled server error, on top of the console logging from base.py.
LOGGING["handlers"]["mail_admins"] = {
    "level": "ERROR",
    "class": "django.utils.log.AdminEmailHandler",
    "filters": ["require_debug_false"],
}
LOGGING["filters"] = {
    "require_debug_false": {"()": "django.utils.log.RequireDebugFalse"},
}
LOGGING["loggers"]["django.request"] = {
    "handlers": ["console", "mail_admins"],
    "level": "ERROR",
    "propagate": False,
}

# Optional: set SENTRY_DSN to wire up real error tracking/alerting. A no-op
# (not a hard dependency) until a DSN is actually configured.
SENTRY_DSN = env("SENTRY_DSN", default="")
if SENTRY_DSN:
    import sentry_sdk
    from sentry_sdk.integrations.django import DjangoIntegration

    sentry_sdk.init(
        dsn=SENTRY_DSN,
        integrations=[DjangoIntegration()],
        send_default_pii=False,
    )

# Without this, django-ratelimit (and any other cache use) falls back to the
# default per-process LocMemCache — correct for a single Gunicorn worker, but
# each additional worker would keep its own independent counter, silently
# turning "10/m" into "10/m × worker count" instead of a real shared limit.
# Set REDIS_URL once the deployment moves beyond a single worker.
REDIS_URL = env("REDIS_URL", default="")
if REDIS_URL:
    CACHES = {
        "default": {
            "BACKEND": "django.core.cache.backends.redis.RedisCache",
            "LOCATION": REDIS_URL,
        }
    }
