from django.conf import settings
from django.contrib.postgres.indexes import GinIndex
from django.contrib.postgres.search import SearchVector, SearchVectorField
from django.db import models
from django.urls import reverse
from django.utils.text import slugify
from django.utils.translation import gettext_lazy as _
from taggit.managers import TaggableManager

from apps.core.models import SeoModelMixin, TimeStampedModel
from apps.core.sanitizers import sanitize_html
from apps.core.validators import validate_image_file


class Category(TimeStampedModel):
    name = models.CharField(_("Name"), max_length=80)
    slug = models.SlugField(_("Slug"), max_length=90, unique=True)

    class Meta:
        verbose_name = _("category")
        verbose_name_plural = _("categories")
        ordering = ["name"]

    def __str__(self):
        return self.name

    def get_absolute_url(self):
        return reverse("blog:list_by_category", kwargs={"category_slug": self.slug})


class Post(SeoModelMixin, TimeStampedModel):
    STATUS_DRAFT = "draft"
    STATUS_PUBLISHED = "published"
    STATUS_CHOICES = [
        (STATUS_DRAFT, _("Draft")),
        (STATUS_PUBLISHED, _("Published")),
    ]

    title = models.CharField(_("Title"), max_length=200)
    slug = models.SlugField(_("Slug"), max_length=220, unique=True, blank=True)
    author = models.ForeignKey(
        settings.AUTH_USER_MODEL,
        on_delete=models.PROTECT,
        related_name="posts",
        verbose_name=_("Author"),
    )
    category = models.ForeignKey(
        Category,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name="posts",
        verbose_name=_("Category"),
    )
    tags = TaggableManager(verbose_name=_("Tags"), blank=True)
    excerpt = models.CharField(_("Excerpt"), max_length=300, blank=True)
    body = models.TextField(_("Body"))
    cover_image = models.ImageField(
        _("Cover image"),
        upload_to="blog/%Y/%m/",
        blank=True,
        null=True,
        validators=[validate_image_file],
    )
    status = models.CharField(
        _("Status"), max_length=10, choices=STATUS_CHOICES, default=STATUS_DRAFT
    )
    published_at = models.DateTimeField(_("Published at"), null=True, blank=True)
    view_count = models.PositiveIntegerField(_("Views"), default=0, editable=False)

    # Stored (not recomputed per request) full-text search vectors, one per
    # language since modeltranslation keeps title/excerpt/body as separate
    # physical *_fa / *_ckb columns — a single vector can't cover both.
    search_vector_fa = SearchVectorField(null=True, blank=True, editable=False)
    search_vector_ckb = SearchVectorField(null=True, blank=True, editable=False)

    class Meta:
        verbose_name = _("post")
        verbose_name_plural = _("posts")
        ordering = ["-published_at", "-created_at"]
        indexes = [
            GinIndex(fields=["search_vector_fa"], name="blog_post_search_fa_gin"),
            GinIndex(fields=["search_vector_ckb"], name="blog_post_search_ckb_gin"),
        ]

    def __str__(self):
        return self.title

    def save(self, *args, **kwargs):
        if not self.slug:
            base = slugify(self.title)[:200] or "post"
            slug = base
            counter = 1
            while Post.objects.filter(slug=slug).exclude(pk=self.pk).exists():
                counter += 1
                slug = f"{base}-{counter}"
            self.slug = slug
        # Explicit per-language fields, not the modeltranslation `self.body`
        # proxy — that only resolves to whichever language is currently
        # active, silently leaving the other language's HTML unsanitized.
        self.body_fa = sanitize_html(self.body_fa)
        self.body_ckb = sanitize_html(self.body_ckb)
        super().save(*args, **kwargs)
        Post.objects.filter(pk=self.pk).update(
            search_vector_fa=SearchVector("title_fa", "excerpt_fa", "body_fa"),
            search_vector_ckb=SearchVector("title_ckb", "excerpt_ckb", "body_ckb"),
        )

    def get_absolute_url(self):
        return reverse("blog:detail", kwargs={"slug": self.slug})

    @property
    def is_published(self):
        return self.status == self.STATUS_PUBLISHED


class Comment(TimeStampedModel):
    post = models.ForeignKey(
        Post, on_delete=models.CASCADE, related_name="comments", verbose_name=_("Post")
    )
    name = models.CharField(_("Name"), max_length=80)
    email = models.EmailField(_("Email"))
    body = models.TextField(_("Body"))
    is_approved = models.BooleanField(_("Approved"), default=False)

    class Meta:
        verbose_name = _("comment")
        verbose_name_plural = _("comments")
        ordering = ["created_at"]

    def __str__(self):
        return f"Comment by {self.name} on {self.post}"
