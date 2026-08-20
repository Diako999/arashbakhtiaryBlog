namespace ArashBlog.Api.Domain;

// Singleton row (Id is always 1) — see SiteSetting for the same pattern.
// Empty Fa/Ckb string fields mean "not customized yet" and the public API
// falls back to SiteSetting.SiteName/MetaDescription (same convention the
// frontend already used before this existed), so a fresh install with an
// unfilled-in landing page still renders sensible copy instead of blanks.
public class LandingPageSettings
{
    public int Id { get; set; }

    public string HeroBadgeFa { get; set; } = "";
    public string HeroBadgeCkb { get; set; } = "";
    public string HeroSubtitleFa { get; set; } = "";
    public string HeroSubtitleCkb { get; set; } = "";
    public string HeroDescriptionFa { get; set; } = "";
    public string HeroDescriptionCkb { get; set; } = "";

    public string AboutRoleFa { get; set; } = "";
    public string AboutRoleCkb { get; set; } = "";
    public string AboutBioFa { get; set; } = "";
    public string AboutBioCkb { get; set; } = "";
    public string? AboutPhotoUrl { get; set; }
    public string AboutGithubUrl { get; set; } = "";
    public string AboutYoutubeUrl { get; set; } = "";
}
