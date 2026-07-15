from django.db import migrations


def seed_navitems(apps, schema_editor):
    NavItem = apps.get_model("navigation", "NavItem")
    entries = [
        # (title_fa, title_ckb, url_name, is_visible, order)
        ("وبلاگ", "بلۆگ", "blog:list", True, 10),
        ("دوره‌ها و کارگاه‌ها", "کۆرس و کارگە کان", "offerings:list", False, 20),
        ("نظرات مشتریان", "بۆچوونەکان", "testimonials:list", False, 30),
        ("منابع رایگان", "سەرچاوە بەخۆڕایی", "leads:list", False, 40),
    ]
    for title_fa, title_ckb, url_name, is_visible, order in entries:
        NavItem.objects.create(
            title=title_fa,
            title_fa=title_fa,
            title_ckb=title_ckb,
            url_name=url_name,
            is_visible=is_visible,
            order=order,
        )


def unseed_navitems(apps, schema_editor):
    NavItem = apps.get_model("navigation", "NavItem")
    NavItem.objects.filter(
        url_name__in=["blog:list", "offerings:list", "testimonials:list", "leads:list"]
    ).delete()


class Migration(migrations.Migration):

    dependencies = [
        ("navigation", "0001_initial"),
    ]

    operations = [
        migrations.RunPython(seed_navitems, unseed_navitems),
    ]
