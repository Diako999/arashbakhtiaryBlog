from django.conf import settings
from django.db import models
from django.urls import reverse
from django.utils.text import slugify
from taggit.managers import TaggableManager

from apps.core.models import SeoModelMixin, TimeStampedModel


class Category(TimeStampedModel):
    name = models.CharField(max_length=80)
    slug = models.SlugField(max_length=90, unique=True)

    class Meta:
        verbose_name_plural = "categories"
        ordering = ["name"]

    def __str__(self):
        return self.name

    def get_absolute_url(self):
        return reverse("blog:list_by_category", kwargs={"category_slug": self.slug})


class Post(SeoModelMixin, TimeStampedModel):
    STATUS_DRAFT = "draft"
    STATUS_PUBLISHED = "published"
    STATUS_CHOICES = [
        (STATUS_DRAFT, "Draft"),
        (STATUS_PUBLISHED, "Published"),
    ]

    title = models.CharField(max_length=200)
    slug = models.SlugField(max_length=220, unique=True, blank=True)
    author = models.ForeignKey(
        settings.AUTH_USER_MODEL, on_delete=models.PROTECT, related_name="posts"
    )
    category = models.ForeignKey(
        Category, on_delete=models.SET_NULL, null=True, blank=True, related_name="posts"
    )
    tags = TaggableManager(blank=True)
    excerpt = models.CharField(max_length=300, blank=True)
    body = models.TextField()
    cover_image = models.ImageField(upload_to="blog/%Y/%m/", blank=True, null=True)
    status = models.CharField(max_length=10, choices=STATUS_CHOICES, default=STATUS_DRAFT)
    published_at = models.DateTimeField(null=True, blank=True)

    class Meta:
        ordering = ["-published_at", "-created_at"]

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
        super().save(*args, **kwargs)

    def get_absolute_url(self):
        return reverse("blog:detail", kwargs={"slug": self.slug})

    @property
    def is_published(self):
        return self.status == self.STATUS_PUBLISHED


class Comment(TimeStampedModel):
    post = models.ForeignKey(Post, on_delete=models.CASCADE, related_name="comments")
    name = models.CharField(max_length=80)
    email = models.EmailField()
    body = models.TextField()
    is_approved = models.BooleanField(default=False)

    class Meta:
        ordering = ["created_at"]

    def __str__(self):
        return f"Comment by {self.name} on {self.post}"
