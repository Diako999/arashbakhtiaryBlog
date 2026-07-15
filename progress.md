# Build Progress

Source of truth for scope/architecture: `Production_Blog_Architecture.html` and
`Production_Blog_Tech_Stack.md` (paths given at project kickoff, not stored in
this repo — re-read them from the original location if unsure about a rule).

Deviations from those two docs, agreed with the user during this build:
- **Bilingual i18n added**: Persian (`fa`, default) + Kurdish Sorani (`ckb`),
  both RTL, live at launch with a language switcher — not in the original
  docs. Dashboard UI is translated too, not just the public site.
- **Rich text editor**: docs offered `django-ckeditor` *or* `django-tinymce`.
  Went with **django-tinymce** — django-ckeditor bundles CKEditor 4, which the
  package itself flags as unsupported with unfixed security issues.
- **Fully responsive**: user confirmed this is a web app that must work well
  across mobile/tablet/desktop — every template is built mobile-first with
  Tailwind breakpoints (`sm:`/`md:`/`lg:`), not just desktop-checked.
- **Translate as you go, not as a final pass**: user caught that the site
  must never render English chrome text (it's fully Persian/Kurdish, no
  English at all) — `{% trans %}` tags alone aren't enough without compiled
  `.po`/`.mo` catalogs, or Django falls back to the English msgid. So: every
  time a template with new user-facing strings is added, run
  `python manage.py makemessages -l fa -l ckb`, fill in both `locale/*/LC_MESSAGES/django.po`
  by hand, then `python manage.py compilemessages`, in the same work session
  — not deferred to task 13 at the end. `gettext` (msgfmt/xgettext) had to be
  installed via apt for this (same sudo-needs-a-terminal issue as Postgres).
- **Environment**: dev machine only had Python 3.14 (Django 5.x doesn't
  officially support it yet) and no PostgreSQL/Tailwind CLI installed.
  Installed Python 3.12 (deadsnakes PPA) and PostgreSQL via apt — user ran
  these themselves since sudo needs an interactive password this session
  can't supply.

## Environment (local dev)

- Python 3.12 in `.venv/` (project venv — activate with `source .venv/bin/activate`)
- PostgreSQL role `prodblog` / db `prodblog_dev`, credentials in `.env` (gitignored)
- Tailwind standalone CLI at `bin/tailwindcss` (gitignored, large binary) —
  re-download any time with `bin/fetch-tailwind.sh`
- Git remote: `origin` → https://github.com/Diako999/arashbakhtiaryBlog.git
  (was empty when connected; nothing pushed yet as of this writing)

## Build order (from kickoff prompt) — status

1. [x] Scaffold project: `config/settings/{base,dev,prod}.py` (django-environ +
   `.env`), `apps/` package with all 9 apps (core, navigation, accounts, blog,
   offerings, testimonials, leads, pages, dashboard) as empty shells with
   correct `AppConfig.name = "apps.<app>"`. `manage.py check` passes.
2. [x] Tailwind standalone CLI wired: `bin/tailwindcss`, `tailwind.config.js`,
   `static/css/input.css` → `static/css/output.css` (v4, `@config` bridges the
   legacy JS config file the architecture doc calls for). Compiles clean.
   **Note:** `output.css` is a build artifact (gitignored) — regenerate it as
   part of the deploy step, same as `collectstatic`.
3. [ ] core + navigation apps: `NavItem` model, nav context processor,
   `ThemeConfig`/`SiteSetting` singletons, theme context processor,
   `base/layout.html`. **Not started yet.**
4. [ ] blog app (Phase 1 deliverable): Post/Category/Tag/Comment, list/detail
   views+templates, live in nav.
5. [ ] accounts app: Author profile (OneToOne on `auth.User`).
6. [ ] dashboard app: Overview/Content/Pages screens (login-gated, plain
   CBVs, not ModelAdmin).
7. [ ] offerings/testimonials/leads apps: models + hidden-by-default views
   (404/redirect enforced at view level) + their dashboard screens. Seed
   NavItem data (blog visible=True, rest False).
8. [ ] pages app (flat pages).
9. [ ] SEO: sitemap.xml (stub sitemaps already wired in `config/urls.py`,
   `items()` empty until blog/offerings models exist), meta fields, OG/Twitter
   tags, robots.txt.
10. [ ] Security settings pass in `config/settings/prod.py` (mostly drafted
    already — SSL/HSTS/cookie flags in place; still need: admin URL renamed,
    django-otp 2FA wired to dashboard login, django-ratelimit on public
    forms, upload validators).
11. [ ] i18n: Persian+Kurdish `.po`/`.mo` translations for actual UI strings
    (framework is wired — `LocaleMiddleware`, `LANGUAGES`, `modeltranslation`
    settings — but no templates/strings exist yet to translate).
12. [ ] End-to-end verification against the "Definition of done" in the
    kickoff prompt, then a stop-and-review summary for the user.

## Key architectural decisions already locked in

- `config/urls.py`: public site wrapped in `i18n_patterns(..., prefix_default_language=True)`
  → `/fa/...` and `/ckb/...`. `/dashboard/`, `/admin/`, `/sitemap.xml`,
  `/i18n/` (language switcher endpoint) stay unprefixed; language there is
  cookie/session-driven via the same switcher.
- `LOGIN_URL = "dashboard:login"`, `LOGIN_REDIRECT_URL = "dashboard:overview"`
  — these named URLs don't exist yet, will be added when the dashboard app
  is built (step 6 above).
- Every app under `apps/` uses `name = "apps.<app>"` with an explicit
  `label = "<app>"` in its `AppConfig`.
- `modeltranslation` is installed and listed first in `INSTALLED_APPS`
  (required load order), default language `fa`, fallback `ckb`.

## How to resume

```bash
cd "/run/media/diako/New Volume/projects/arashbakhtiraiBlog"
source .venv/bin/activate
python manage.py check      # should say "no issues"
```

Then continue at step 3 above (core + navigation apps) unless this file has
been updated since.
