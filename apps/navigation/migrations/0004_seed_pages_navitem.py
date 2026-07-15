from django.db import migrations


def seed_pages_navitem(apps, schema_editor):
    NavItem = apps.get_model("navigation", "NavItem")
    NavItem.objects.create(
        title="درباره ما",
        title_fa="درباره ما",
        title_ckb="دەربارەمان",
        url_name="pages:about",
        is_visible=False,
        order=50,
    )


def unseed_pages_navitem(apps, schema_editor):
    NavItem = apps.get_model("navigation", "NavItem")
    NavItem.objects.filter(url_name="pages:about").delete()


class Migration(migrations.Migration):

    dependencies = [
        ("navigation", "0003_alter_navitem_options_alter_navitem_is_visible_and_more"),
    ]

    operations = [
        migrations.RunPython(seed_pages_navitem, unseed_pages_navitem),
    ]
