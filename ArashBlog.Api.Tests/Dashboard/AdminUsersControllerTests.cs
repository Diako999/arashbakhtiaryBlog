using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using Xunit;

namespace ArashBlog.Api.Tests.Dashboard;

public class AdminUsersControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Create_adds_a_new_admin_who_can_then_log_in()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "admin-owner-1");

        var createResponse = await client.PostAsJsonAsync(
            "/api/dashboard/admins", new CreateAdminRequest("second-admin-1", "Sup3r-Secret!"));
        var created = await createResponse.Content.ReadFromJsonAsync<AdminUserDto>();

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal("second-admin-1", created!.UserName);
        Assert.False(created.TwoFactorEnabled);

        var newAdminClient = factory.CreateClient();
        var loginResponse = await newAdminClient.PostAsJsonAsync(
            "/api/auth/login", new Features.Auth.LoginRequest("second-admin-1", "Sup3r-Secret!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Create_rejects_a_password_that_fails_the_policy()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "admin-owner-2");

        var response = await client.PostAsJsonAsync("/api/dashboard/admins", new CreateAdminRequest("second-admin-2", "short"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_includes_every_admin()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "admin-owner-3");
        await client.PostAsJsonAsync("/api/dashboard/admins", new CreateAdminRequest("second-admin-3", "Sup3r-Secret!"));

        var admins = await client.GetFromJsonAsync<List<AdminUserDto>>("/api/dashboard/admins");

        Assert.Contains(admins!, a => a.UserName == "admin-owner-3");
        Assert.Contains(admins!, a => a.UserName == "second-admin-3");
    }

    [Fact]
    public async Task Delete_rejects_deleting_yourself()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "admin-owner-4");
        await client.PostAsJsonAsync("/api/dashboard/admins", new CreateAdminRequest("second-admin-4", "Sup3r-Secret!"));
        var admins = await client.GetFromJsonAsync<List<AdminUserDto>>("/api/dashboard/admins");
        var self = admins!.Single(a => a.UserName == "admin-owner-4");

        var response = await client.DeleteAsync($"/api/dashboard/admins/{self.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_another_admin()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "admin-owner-5");
        var createResponse = await client.PostAsJsonAsync("/api/dashboard/admins", new CreateAdminRequest("second-admin-5", "Sup3r-Secret!"));
        var created = await createResponse.Content.ReadFromJsonAsync<AdminUserDto>();

        var deleteResponse = await client.DeleteAsync($"/api/dashboard/admins/{created!.Id}");
        var admins = await client.GetFromJsonAsync<List<AdminUserDto>>("/api/dashboard/admins");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.DoesNotContain(admins!, a => a.UserName == "second-admin-5");
    }

    [Fact]
    public async Task List_is_denied_without_2fa_verification()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/admins");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
