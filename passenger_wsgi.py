"""
Entry point for IranServer's shared hosting (cPanel "Setup Python App",
backed by Phusion Passenger). Passenger imports this module directly and
looks for a module-level ``application`` callable — it does not run
`gunicorn`/`config.wsgi` itself, so this file bridges the two.

cPanel's Setup Python App creates and activates its own virtualenv (and
adds this file's directory to sys.path) before importing this module, so
no manual sys.path/virtualenv activation is done here.
"""
import os

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "config.settings.prod")

from config.wsgi import application  # noqa: E402,F401
