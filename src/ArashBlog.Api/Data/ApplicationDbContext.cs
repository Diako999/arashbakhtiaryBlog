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
            e.Property(p => p.MetaTitle).HasMaxLength(70);
            e.Property(p => p.MetaDescription).HasMaxLength(160);
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
    }
}
