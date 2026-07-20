namespace ArashBlog.Api.Domain;

public enum ThemeMode
{
    Light,
    Dark,
}

// Singleton row (Id is always 1) — see SiteSetting for the same pattern.
public class ThemeConfig
{
    public int Id { get; set; }
    public string BrandColor { get; set; } = "#0f9d8e";
    public string AccentColor { get; set; } = "#f0b429";
    public ThemeMode DefaultMode { get; set; } = ThemeMode.Dark;
}
