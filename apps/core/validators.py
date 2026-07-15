from django.conf import settings
from django.core.exceptions import ValidationError
from django.utils.translation import gettext_lazy as _


def _check_size(value):
    max_bytes = settings.MAX_UPLOAD_SIZE_MB * 1024 * 1024
    if value.size > max_bytes:
        raise ValidationError(
            _("File too large. Max size is %(max)s MB.") % {"max": settings.MAX_UPLOAD_SIZE_MB}
        )


def validate_image_file(value):
    _check_size(value)
    content_type = getattr(getattr(value, "file", None), "content_type", None)
    if content_type and content_type not in settings.ALLOWED_UPLOAD_IMAGE_TYPES:
        raise ValidationError(_("Unsupported image type."))


def validate_document_file(value):
    _check_size(value)
    content_type = getattr(getattr(value, "file", None), "content_type", None)
    if content_type and content_type not in settings.ALLOWED_UPLOAD_DOCUMENT_TYPES:
        raise ValidationError(_("Unsupported file type."))
