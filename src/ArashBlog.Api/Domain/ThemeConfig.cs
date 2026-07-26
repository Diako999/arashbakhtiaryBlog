namespace ArashBlog.Api.Domain;

public enum ThemeMode
{
    Light,
    Dark,
}

public enum ThemeFont
{
    Vazirmatn,
    Sahel,
    Samim,
}

public enum ThemeCardStyle
{
    Rounded,
    Sharp,
    Soft,
}

public enum ThemeHeaderFooterStyle
{
    Glass,
    Solid,
}

// Singleton row (Id is always 1) — see SiteSetting for the same pattern.
public class ThemeConfig
{
    public int Id { get; set; }
    public string BrandColor { get; set; } = "#E5484D";
    public string AccentColor { get; set; } = "#8B9098";
    public ThemeMode DefaultMode { get; set; } = ThemeMode.Dark;
    public ThemeFont FontChoice { get; set; } = ThemeFont.Vazirmatn;
    public ThemeCardStyle CardStyle { get; set; } = ThemeCardStyle.Rounded;
    public ThemeHeaderFooterStyle HeaderFooterStyle { get; set; } = ThemeHeaderFooterStyle.Glass;
}
