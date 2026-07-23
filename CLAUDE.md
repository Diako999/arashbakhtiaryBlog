# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A bilingual (Persian `fa` default + Kurdish Sorani `ckb`), fully RTL Django blog/course-marketing site for an Iran-based audience, with a custom login+2FA-gated Dashboard (not the Django admin) for content editors. No JS framework/build step — server-rendered templates + Tailwind CSS, no npm/node_modules anywhere.

## Commands

```bash
source .venv/bin/activate                      # Python 3.12 venv

python manage.py runserver                     # dev server (DJANGO_SETTINGS_MODULE defaults to config.settings.dev)
python manage.py check                          # should say "no issues"
python manage.py test                           # full suite
python manage.py test apps.dashboard             # single app
python manage.py test apps.dashboard.tests.TwoFactorGateTests.test_valid_totp_token_grants_access  # single test

python manage.py makemigrations
python manage.py migrate
python manage.py makemigrations --check          # detect drift, e.g. in CI

# i18n — required for ANY new user-facing string, same work session (see below)
python manage.py makemessages -l fa -l ckb --no-obsolete
# hand-fill locale/{fa,ckb}/LC_MESSAGES/django.po, then:
python manage.py compilemessages

# Tailwind (standalone CLI, no Node/npm — binary is gitignored)
bin/fetch-tailwind.sh                            # re-download bin/tailwindcss on a new machine
./bin/tailwindcss -i tailwind/input.css -o static/css/output.css --minify

python manage.py collectstatic                  # writes to staticfiles/ (whitenoise serves it)
```

Settings module is selected via `DJANGO_SETTINGS_MODULE`: `config.settings.dev` (manage.py's default) or `config.settings.prod`. Both import everything from `config.settings.base`. Local secrets/DB URL live in `.env` (gitignored; see `.env.example`).

Database is MySQL/MariaDB via PyMySQL (pure-Python, no compiler needed for the target host) — `config/__init__.py` installs the `pymysql.install_as_MySQLdb()` shim before anything touches the DB layer, which is why it lives in `__init__.py` rather than in settings. Local dev DB is a real MariaDB instance (not SQLite) — `DATABASE_URL=mysql://...` in `.env`.

## Architecture

**Apps** (`apps/*`, each `AppConfig.name = "apps.<app>"` with explicit `label`):
- `core` — cross-cutting: `SiteSetting`/`ThemeConfig` singletons, `SeoContextMixin`, upload validators (`validators.py`, real file-content sniffing via Pillow/magic bytes, not trusting Content-Type), HTML sanitizer (`sanitizers.py`, nh3 — must be kept in sync with whatever tags `TINYMCE_DEFAULT_CONFIG`'s toolbar exposes since server-side sanitization is the real security boundary), `UnicodeSlugConverter` (django-taggit tag slugs keep non-ASCII fa/ckb characters, so the stock `slug` URL converter can't match them).
- `navigation` — `NavItem` model + `section_is_visible()` / `SectionVisibleRequiredMixin` (`apps/navigation/mixins.py`): the **phased-rollout mechanism**. A single `NavItem.is_visible` flag is read both by the nav context processor (hides the link) and by the mixin at the view layer (404s the URL) — deliberately one flag instead of the two the original spec called for, so nav-hidden and URL-blocked can't drift out of sync. Toggled live from the Dashboard's Pages screen, no restart needed.
- `blog` — Category/Post/Comment, django-taggit tags, plain `icontains` search (see MySQL migration note below) — the one section that's always live, never gated by the phased-rollout mechanism.
- `offerings` / `testimonials` / `leads` / `pages` — gated sections (Offering+Session+Enrollment; Testimonial; LeadMagnet+Submission; flat pages) using `SectionVisibleRequiredMixin`. `pages` app has no dedicated dashboard CRUD — deliberately left to the stock Django admin fallback.
- `accounts` — `Author` profile, auto-created via a `post_save` signal on `auth.User`.
- `dashboard` — the custom editor UI (Overview/Content/Pages/Offerings/Leads/Testimonials/Settings). `apps/dashboard/otp_views.py` implements TOTP 2FA (`django-otp`, QR enrollment, no external service) plus static recovery codes; `OTPRequiredMixin`/`dashboard_login_required` gate every dashboard view (stronger than plain `LoginRequiredMixin`). `TOGGLEABLE_SECTION_URL_NAMES` in `views.py` is the security whitelist of which `url_name`s the Pages screen may flip.

**URL structure** (`config/urls.py`): public site is wrapped in `i18n_patterns(..., prefix_default_language=True)` → every public URL is `/fa/...` or `/ckb/...`. `/dashboard/`, `settings.ADMIN_URL` (moved off `/admin/`, must be a unique value in prod `.env`), `/sitemap.xml`, and `/i18n/` (language-switcher endpoint) stay unprefixed; language there is cookie-driven via the same switcher, honored everywhere by `LocaleMiddleware`. `sitemap.xml` cross-checks `NavItem.is_visible` for every hidden-until-published section so it never leaks a URL before the admin actually publishes it.

**i18n is not optional/best-effort** — the site must never render English chrome text:
- Every model field needs an explicit `verbose_name=gettext_lazy("...")`; Django otherwise derives an always-English label from the Python attribute name regardless of active locale.
- `{% trans %}` tags alone do nothing without compiled catalogs — run `makemessages`, hand-translate both `locale/{fa,ckb}/LC_MESSAGES/django.po`, then `compilemessages`, in the same session a new user-facing string is added (not deferred).
- `modeltranslation` is installed and must load before `django.contrib.admin` in `INSTALLED_APPS` (load-order requirement); default language `fa`, fallback `ckb`.
- TinyMCE and django-taggit's own bundled fields localize themselves automatically.

**Security stack**: dashboard behind login+2FA (not just login); admin moved off `/admin/`; `django-ratelimit`'s `RatelimitMiddleware` wired in (without it, `RATELIMIT_VIEW` is silently never consulted) covering every public form (comments, enrollment, lead-magnet gate, contact); upload validators check actual file bytes; nh3 sanitizes rich text server-side; `prod.py` adds the full HSTS/secure-cookie/proxy-SSL stack plus optional Sentry (`SENTRY_DSN`) and Redis-backed cache (`REDIS_URL` — needed once deployment goes beyond a single worker, otherwise ratelimit counters are per-process and silently multiply by worker count).

**MySQL migration note**: this project was originally built against PostgreSQL and migrated to MySQL/MariaDB for hosting reasons. Full-text search was downgraded from Postgres `SearchVectorField`/GIN indexes to plain `Q(...icontains=...)` in `apps/blog/views.py` — a deliberate scope decision, not a placeholder; don't assume ranked/stemmed search exists. `charset: utf8mb4` is set explicitly on `DATABASES["default"]["OPTIONS"]` (MySQL's bare `utf8` alias is 3-byte and can silently corrupt Persian/Kurdish content).

**Deploy target**: IranServer shared Python hosting via cPanel/Passenger (`passenger_wsgi.py` at repo root), not a systemd-managed Gunicorn process — no SSH/Git on that host, deploys go through DirectAdmin's Setup Python App.
