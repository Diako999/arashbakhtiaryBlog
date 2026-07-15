from .models import SiteSetting, ThemeConfig


def theme(request):
    config = ThemeConfig.load()
    return {
        "theme_colors": config.colors,
        "theme_default_mode": config.default_mode,
    }


def site_settings(request):
    return {"site_settings": SiteSetting.load()}
