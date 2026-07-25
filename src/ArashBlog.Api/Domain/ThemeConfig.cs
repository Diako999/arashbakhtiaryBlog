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
    public string BrandColor { get; set; } = "#E5484D";
    public string AccentColor { get; set; } = "#8B9098";
    public ThemeMode DefaultMode { get; set; } = ThemeMode.Dark;
}
