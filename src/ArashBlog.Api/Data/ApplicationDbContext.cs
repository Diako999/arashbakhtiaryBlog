using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<NavItem> NavItems => Set<NavItem>();
    public DbSet<Offering> Offerings => Set<Offering>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<LeadMagnet> LeadMagnets => Set<LeadMagnet>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<FlatPage> FlatPages => Set<FlatPage>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<ThemeConfig> ThemeConfigs => Set<ThemeConfig>();
    public DbSet<LandingSection> LandingSections => Set<LandingSection>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
            e.Property(c => c.NameFa).HasMaxLength(80);
            e.Property(c => c.NameCkb).HasMaxLength(80);
            e.Property(c => c.Slug).HasMaxLength(90);
        });

        builder.Entity<Tag>(e =>
        {
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.Name).HasMaxLength(80);
            e.Property(t => t.Slug).HasMaxLength(90);
        });

        builder.Entity<Post>(e =>
        {
            e.HasIndex(p => p.Slug).IsUnique();
            e.Property(p => p.TitleFa).HasMaxLength(200);
            e.Property(p => p.TitleCkb).HasMaxLength(200);
            e.Property(p => p.Slug).HasMaxLength(220);
            e.Property(p => p.ExcerptFa).HasMaxLength(300);
            e.Property(p => p.ExcerptCkb).HasMaxLength(300);
            e.Property(p => p.MetaTitleFa).HasMaxLength(70);
            e.Property(p => p.MetaTitleCkb).HasMaxLength(70);
            e.Property(p => p.MetaDescriptionFa).HasMaxLength(160);
            e.Property(p => p.MetaDescriptionCkb).HasMaxLength(160);
            e.Property(p => p.BgColor).HasMaxLength(7);
            e.Property(p => p.TextColor).HasMaxLength(7);
            e.Property(p => p.AccentColor).HasMaxLength(7);
            e.HasOne(p => p.Author).WithMany().HasForeignKey(p => p.AuthorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Category).WithMany(c => c.Posts).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Comment>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(80);
            e.Property(c => c.Email).HasMaxLength(256);
            e.HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<NavItem>(e =>
        {
            e.HasIndex(n => n.Key).IsUnique();
            e.Property(n => n.TitleFa).HasMaxLength(60);
            e.Property(n => n.TitleCkb).HasMaxLength(60);
            e.Property(n => n.Key).HasMaxLength(50);
            e.Property(n => n.Path).HasMaxLength(200);
        });

        builder.Entity<Offering>(e =>
        {
            e.HasIndex(o => o.Slug).IsUnique();
            e.Property(o => o.TitleFa).HasMaxLength(200);
            e.Property(o => o.TitleCkb).HasMaxLength(200);
            e.Property(o => o.Slug).HasMaxLength(220);
            e.Property(o => o.SummaryFa).HasMaxLength(300);
            e.Property(o => o.SummaryCkb).HasMaxLength(300);
            e.Property(o => o.Price).HasPrecision(10, 2);
            e.Property(o => o.MetaTitleFa).HasMaxLength(70);
            e.Property(o => o.MetaTitleCkb).HasMaxLength(70);
            e.Property(o => o.MetaDescriptionFa).HasMaxLength(160);
            e.Property(o => o.MetaDescriptionCkb).HasMaxLength(160);
        });

        builder.Entity<Session>(e =>
        {
            e.Property(s => s.Location).HasMaxLength(200);
            e.HasOne(s => s.Offering).WithMany(o => o.Sessions).HasForeignKey(s => s.OfferingId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Enrollment>(e =>
        {
            e.Property(en => en.Name).HasMaxLength(120);
            e.Property(en => en.Email).HasMaxLength(256);
            e.Property(en => en.Phone).HasMaxLength(40);
            e.HasOne(en => en.Offering).WithMany(o => o.Enrollments).HasForeignKey(en => en.OfferingId).OnDelete(DeleteBehavior.Cascade);
            // ClientSetNull, not SetNull: SQL Server refuses a DB-level ON
            // DELETE SET NULL here because Offering already cascades to
            // Enrollment directly AND indirectly via Session, and two
            // cascade paths to the same table isn't allowed. EF still
            // nulls the FK for tracked entities in the same context;
            // deleting a Session out from under enrollments loaded in a
            // different context is not a scenario this app has.
            e.HasOne(en => en.Session).WithMany(s => s.Enrollments).HasForeignKey(en => en.SessionId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        builder.Entity<Testimonial>(e =>
        {
            e.Property(t => t.AuthorName).HasMaxLength(120);
            e.Property(t => t.AuthorRoleFa).HasMaxLength(150);
            e.Property(t => t.AuthorRoleCkb).HasMaxLength(150);
            e.HasOne(t => t.Offering).WithMany().HasForeignKey(t => t.OfferingId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<LeadMagnet>(e =>
        {
            e.HasIndex(l => l.Slug).IsUnique();
            e.Property(l => l.TitleFa).HasMaxLength(200);
            e.Property(l => l.TitleCkb).HasMaxLength(200);
            e.Property(l => l.Slug).HasMaxLength(220);
            e.Property(l => l.MetaTitleFa).HasMaxLength(70);
            e.Property(l => l.MetaTitleCkb).HasMaxLength(70);
            e.Property(l => l.MetaDescriptionFa).HasMaxLength(160);
            e.Property(l => l.MetaDescriptionCkb).HasMaxLength(160);
        });

        builder.Entity<Submission>(e =>
        {
            e.Property(s => s.Name).HasMaxLength(120);
            e.Property(s => s.Email).HasMaxLength(256);
            e.HasOne(s => s.LeadMagnet).WithMany(l => l.Submissions).HasForeignKey(s => s.LeadMagnetId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FlatPage>(e =>
        {
            e.HasIndex(p => p.Slug).IsUnique();
            e.Property(p => p.Slug).HasMaxLength(100);
            e.Property(p => p.TitleFa).HasMaxLength(200);
            e.Property(p => p.TitleCkb).HasMaxLength(200);
            e.Property(p => p.MetaTitleFa).HasMaxLength(70);
            e.Property(p => p.MetaTitleCkb).HasMaxLength(70);
            e.Property(p => p.MetaDescriptionFa).HasMaxLength(160);
            e.Property(p => p.MetaDescriptionCkb).HasMaxLength(160);
        });

        builder.Entity<SiteSetting>(e =>
        {
            e.Property(s => s.SiteName).HasMaxLength(120);
            e.Property(s => s.ContactEmail).HasMaxLength(256);
            e.Property(s => s.ContactPhone).HasMaxLength(40);
            e.Property(s => s.MetaDescription).HasMaxLength(300);
        });

        builder.Entity<ThemeConfig>(e =>
        {
            e.Property(t => t.BrandColor).HasMaxLength(7);
            e.Property(t => t.AccentColor).HasMaxLength(7);
        });

        builder.Entity<LandingSection>(e =>
        {
            e.HasIndex(s => s.Type).IsUnique();
            e.Property(s => s.HeadingFa).HasMaxLength(200);
            e.Property(s => s.HeadingCkb).HasMaxLength(200);
            e.Property(s => s.SubheadingFa).HasMaxLength(300);
            e.Property(s => s.SubheadingCkb).HasMaxLength(300);
            e.Property(s => s.PrimaryCtaTextFa).HasMaxLength(60);
            e.Property(s => s.PrimaryCtaTextCkb).HasMaxLength(60);
            e.Property(s => s.SecondaryCtaTextFa).HasMaxLength(60);
            e.Property(s => s.SecondaryCtaTextCkb).HasMaxLength(60);
        });
    }
}
