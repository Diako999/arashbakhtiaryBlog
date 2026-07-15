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
- **Every model field needs an explicit `verbose_name=_("...")`**: Django
  auto-generates a field's form/admin label from its Python attribute name
  when no `verbose_name` is set (e.g. `cover_image` → "Cover image") — always
  in English, regardless of `LANGUAGES`/active locale, since it's derived
  from the field name, not looked up in any translation catalog. This showed
  up as English labels ("Slug", "Category", "Status"...) on the dashboard
  post form even after `{% trans %}` was used everywhere in templates. Fix:
  every field on every model needs `verbose_name=gettext_lazy("English source string")`,
  then that string goes through the normal makemessages/compilemessages
  cycle like any other. Done for `core`, `navigation`, `accounts`, `blog`
  models so far — **must do the same for offerings/testimonials/leads/pages
  models when building them**, or their dashboard forms will leak English.
  (TinyMCE and django-taggit's own bundled fields already localize
  themselves automatically — no action needed there.)
- **TinyMCE promotional banner**: the free/open-source build shows a "Get all
  features💝" upsell nag by default. Suppressed via
  `TINYMCE_DEFAULT_CONFIG = {"promotion": False, "branding": False, ...}` in
  `config/settings/base.py` — apply this to any other TinyMCE configs added
  later too.

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
3. [x] core + navigation apps: `NavItem` model, nav context processor,
   `ThemeConfig`/`SiteSetting` singletons, theme context processor,
   `base/layout.html` (responsive nav, theme toggle, language switcher, RTL).
4. [x] blog app (Phase 1 deliverable): Category/Post/Comment models
   (django-taggit for tags), Postgres full-text search, list/detail views,
   templates. Verified end to end in browser (Persian + Kurdish content).
5. [x] accounts app: Author profile (OneToOne on `auth.User`), auto-created
   via post_save signal.
6. [x] dashboard app: Overview, Content (post + category CRUD, TinyMCE),
   Pages visibility-switch screen. Login-gated (`dashboard:login`). Verified
   end to end in browser: login, create post, toggle a section
   Published/Hidden and watched the public nav update live with no restart.
7. [x] offerings/testimonials/leads apps: Offering/Session/Enrollment,
   Testimonial, LeadMagnet/Submission models (all with `verbose_name=_(...)`
   on every field). Public views 404 while hidden via
   `SectionVisibleRequiredMixin` (`apps/navigation/mixins.py`), verified: all
   three returned 404 while hidden, 200 immediately after toggling visible
   via the dashboard Pages screen, no restart needed. Dashboard screens:
   Offerings (CRUD + inline session formset + read-only enrollment list),
   Leads (CRUD + submissions inbox + CSV export + mark-contacted toggle),
   Testimonials (CRUD + approve/reject toggle + up/down reorder). Public
   forms (enrollment, lead-magnet gate) both verified working end to end via
   curl, including django-ratelimit decorators. All new UI strings
   translated (fa/ckb) and compiled.
8. [x] pages app (flat pages): `FlatPage` model, fixed routes `/about/` and
   `/contact/` (matches the doc's literal URL map exactly, rather than a
   generic `<slug>/` catch-all). The architecture doc's own app table lists
   `pages` as "Built, hidden" alongside offerings/testimonials/leads — so
   it's gated the same way, via one shared NavItem (`pages:about`) added in
   a new migration (`navigation/migrations/0004_seed_pages_navitem.py`;
   the original seed migration only covered the first three). No dedicated
   dashboard CRUD screen — deliberately left to the Django admin fallback,
   since "Pages" as a dashboard nav label is already taken by the
   visibility-switch screen and doc's own dashboard module table doesn't
   list flat-page content editing as a screen. Verified end to end: 404
   while hidden, 200 + correct content immediately after toggling visible,
   nav link appears live.
9. [x] SEO: sitemap.xml now includes posts, offerings, leads, and pages —
   **and, importantly, each hidden-until-published section's sitemap
   entries respect `NavItem.is_visible` too** (`section_is_visible()` from
   `apps/navigation/mixins.py`), so `/sitemap.xml` never leaks a hidden
   section's URLs to search engines before the admin actually publishes it.
   Verified: created a published Offering while its section was hidden —
   sitemap.xml stayed empty; toggling the section visible made the URL
   appear immediately. Added `<link rel="canonical">`, `og:url`, `og:image`
   (falls back to `SiteSetting.logo`), and `twitter:title`/`description` to
   `base/layout.html`. `SeoContextMixin` gained `get_seo_image()` (checks
   `cover_image` then `photo` on the view's object).
10. [ ] Security settings pass in `config/settings/prod.py` (mostly drafted
    already — SSL/HSTS/cookie flags in place; still need: admin URL renamed,
    django-otp 2FA wired to dashboard login, django-ratelimit is already
    applied to the enrollment and lead-gate POST views but double check
    blog comments if a submission view is ever added, upload validators —
    `apps/leads/models.py` has a size validator already, apply the same
    pattern to any other upload fields).
11. [x] i18n: fully wired and verified for everything built (blog, dashboard,
    offerings, testimonials, leads, pages) — nothing left to translate as of
    this writing. Keep following the "translate as you go" practice for
    anything still to come (SEO templates, security-related UI if any).
12. [ ] End-to-end verification against the "Definition of done" in the
    kickoff prompt, then a stop-and-review summary for the user.

## Local test login (dev DB only)

Superuser `admin` / password `test-only-not-for-prod` — created for manual
testing during this build. Not a credential for any real environment.

## Key architectural decisions already locked in

- `config/urls.py`: public site wrapped in `i18n_patterns(..., prefix_default_language=True)`
  → `/fa/...` and `/ckb/...`. `/dashboard/`, `/admin/`, `/sitemap.xml`,
  `/i18n/` (language switcher endpoint) stay unprefixed; language there is
  cookie/session-driven via the same switcher.
- `LOGIN_URL = "dashboard:login"`, `LOGIN_REDIRECT_URL = "dashboard:overview"`
  — wired and working.
- Phased-rollout visibility is a **single flag**, not the doc's literal
  "NavItem flag + separate app-level flag": `NavItem.is_visible` is checked
  both by the nav context processor (hides the link) and by
  `SectionVisibleRequiredMixin` at the view layer (404s the page). Two
  separate flags per section seemed likely to drift out of sync; reading the
  same NavItem row from both places gives the doc's required behavior
  (nav-hidden AND URL-blocked) from one source of truth.
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

Then continue at step 10 above (security settings) unless this file has
been updated since.
