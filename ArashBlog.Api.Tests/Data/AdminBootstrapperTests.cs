using ArashBlog.Api.Data;
using ArashBlog.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArashBlog.Api.Tests.Data;

// Exercises AdminBootstrapper directly against a minimal DI container
// (InMemory EF + Identity) rather than the full WebApplicationFactory
// pipeline, since the only thing under test is "does the right thing
// happen given zero/one users and present/absent config" — no HTTP
// involved.
public class AdminBootstrapperTests
{
    private static ServiceProvider BuildServices()
    {
        // The database name must be generated ONCE per test and captured by
        // the lambda below, not evaluated inside it — AddDbContext invokes
        // the configure callback fresh on every DbContext construction, so
        // `Guid.NewGuid()` inside the lambda silently hands each scope its
        // own isolated in-memory store despite them all sharing this same
        // ServiceProvider. Confirmed by a throwaway probe before this fix.
        var databaseName = Guid.NewGuid().ToString();
        var root = new InMemoryDatabaseRoot();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName, root));
        services.AddLogging();
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfig(string? username, string? password)
    {
        var dict = new Dictionary<string, string?>();
        if (username is not null) dict["InitialAdmin:Username"] = username;
        if (password is not null) dict["InitialAdmin:Password"] = password;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public async Task CreatesAdmin_WhenNoUsersExistAndConfigIsSet()
    {
        var services = BuildServices();
        var config = BuildConfig("admin", "Sup3r-Secret!");

        await AdminBootstrapper.EnsureAsync(services, config);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByNameAsync("admin");
        Assert.NotNull(admin);
        Assert.True(await userManager.CheckPasswordAsync(admin!, "Sup3r-Secret!"));
    }

    [Fact]
    public async Task DoesNothing_WhenAUserAlreadyExists()
    {
        var services = BuildServices();
        var config = BuildConfig("admin", "Sup3r-Secret!");

        using (var scope = services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var existing = new ApplicationUser { UserName = "existing", Email = "existing@example.com" };
            var result = await userManager.CreateAsync(existing, "Sup3r-Secret!");
            Assert.True(result.Succeeded);
        }

        await AdminBootstrapper.EnsureAsync(services, config);

        using var verifyScope = services.CreateScope();
        var verifyManager = verifyScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Null(await verifyManager.FindByNameAsync("admin"));
    }

    [Fact]
    public async Task DoesNothing_WhenConfigIsMissing()
    {
        var services = BuildServices();
        var config = BuildConfig(null, null);

        await AdminBootstrapper.EnsureAsync(services, config);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Empty(userManager.Users);
    }
}
