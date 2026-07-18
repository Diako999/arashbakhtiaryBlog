from PIL import Image, UnidentifiedImageError

from django.conf import settings
from django.core.exceptions import ValidationError
from django.utils.translation import gettext_lazy as _

# Pillow's own format name for each content type we accept — checks what the
# file actually *is*, not what the client's Content-Type header claims it is.
IMAGE_FORMAT_BY_CONTENT_TYPE = {
    "image/jpeg": "JPEG",
    "image/png": "PNG",
    "image/webp": "WEBP",
}

# Magic-byte signatures for each document content type we accept.
DOCUMENT_SIGNATURES_BY_CONTENT_TYPE = {
    "application/pdf": (b"%PDF-",),
}


def _check_size(value):
    max_bytes = settings.MAX_UPLOAD_SIZE_MB * 1024 * 1024
    if value.size > max_bytes:
        raise ValidationError(
            _("File too large. Max size is %(max)s MB.") % {"max": settings.MAX_UPLOAD_SIZE_MB}
        )


def validate_image_file(value):
    _check_size(value)
    value.seek(0)
    try:
        img = Image.open(value)
        detected_format = img.format
        img.verify()
    except (UnidentifiedImageError, OSError, ValueError):
        raise ValidationError(_("Unsupported or corrupt image file."))
    finally:
        value.seek(0)

    allowed_formats = {
        IMAGE_FORMAT_BY_CONTENT_TYPE.get(ct) for ct in settings.ALLOWED_UPLOAD_IMAGE_TYPES
    }
    if detected_format not in allowed_formats:
        raise ValidationError(_("Unsupported image type."))


def validate_document_file(value):
    _check_size(value)
    value.seek(0)
    header = value.read(8)
    value.seek(0)

    signatures = [
        sig
        for content_type in settings.ALLOWED_UPLOAD_DOCUMENT_TYPES
        for sig in DOCUMENT_SIGNATURES_BY_CONTENT_TYPE.get(content_type, ())
    ]
    if not signatures or not any(header.startswith(sig) for sig in signatures):
        raise ValidationError(_("Unsupported file type."))
