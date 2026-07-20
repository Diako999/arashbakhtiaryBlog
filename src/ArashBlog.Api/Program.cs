using System.Threading.RateLimiting;
using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 10;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// API, not a server-rendered app — a redirect-to-login-page response makes
// no sense here, so unauthenticated/unauthorized hits get a bare status
// code instead and the SPA decides what to render.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// Rate limits mirror the Django project's django-ratelimit decorators:
// 5/min on login + OTP endpoints, 5/min on comment submission.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    static RateLimitPartition<string> FixedWindow(HttpContext httpContext) =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 5,
                QueueLimit = 0,
            });

    options.AddPolicy("auth", FixedWindow);
    options.AddPolicy("comments", FixedWindow);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await DbSeeder.SeedAsync(app.Services);
}

// Unconditional (dev, test, and prod alike) — these NavItem rows are real
// application configuration the phased-rollout mechanism depends on, not
// demo content. Prod is expected to have already run migrations as a
// separate deploy step before the app starts, same assumption the Django
// project's own navigation-seed migration made.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await NavItemSeeder.EnsureAsync(db);
    await FlatPageSeeder.EnsureAsync(db);
}

// Split from the block above on purpose: the test host runs under a
// "Testing" environment (see ArashBlog.Api.Tests/TestWebApplicationFactory)
// so it gets neither the dev-only seed (which calls Database.MigrateAsync,
// meaningless against the in-memory test provider) nor a redirect that
// would break TestServer's in-process HTTP client.
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Skipped under "Testing": TestServer's in-process client has no real
// per-request IP, so every request in the whole test run collapses onto
// one RemoteIpAddress-keyed partition and trips the limiter almost
// immediately, failing unrelated tests for reasons that have nothing to
// do with what they're checking. Rate-limit behavior itself is verified
// manually against the real running app (see the M1 plan).
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Single-deployable SPA fallback: anything under /api that didn't match a
// controller is a real 404, anything else falls back to the built React
// app's index.html so client-side routing (React Router) can take over.
app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    var indexPath = Path.Combine(app.Environment.WebRootPath, "index.html");
    if (!File.Exists(indexPath))
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(
            "ArashBlog.Api is running. Run `npm run dev` in src/arashblog-web for the frontend in " +
            "development, or `npm run build` there to populate wwwroot for a single-deployable run.");
        return;
    }

    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(indexPath);
});

app.Run();

// Exposed so ArashBlog.Api.Tests can boot the app via WebApplicationFactory<Program>.
public partial class Program;
