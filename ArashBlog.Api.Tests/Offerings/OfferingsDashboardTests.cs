using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using Xunit;

namespace ArashBlog.Api.Tests.Offerings;

public class OfferingsDashboardTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task List_is_denied_without_2fa_verification()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/offerings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_can_add_edit_and_remove_sessions_independently()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "offering-admin-3");

        var createResponse = await client.PostAsJsonAsync("/api/dashboard/offerings", new UpsertOfferingRequest(
            "", null, null, "Draft", "دوره", "کۆرس", "", "", "<p>x</p>", "<p>x</p>", "", "", "", "",
            [
                new DashboardSessionDto(null, DateTimeOffset.UtcNow.AddDays(1), null, "جلسه یک", 10),
                new DashboardSessionDto(null, DateTimeOffset.UtcNow.AddDays(2), null, "جلسه دو", 10),
            ]));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardOfferingDetailDto>();
        Assert.Equal(2, created!.Sessions.Count);

        // Keep the first session but edit its location, drop the second, add a third.
        var keep = created.Sessions[0];
        var updateResponse = await client.PutAsJsonAsync($"/api/dashboard/offerings/{created.Id}", new UpsertOfferingRequest(
            created.Slug, null, null, "Published", "دوره", "کۆرس", "", "", "<p>x</p>", "<p>x</p>", "", "", "", "",
            [
                keep with { Location = "جلسه یک (اصلاح‌شده)" },
                new DashboardSessionDto(null, DateTimeOffset.UtcNow.AddDays(3), null, "جلسه سه", 5),
            ]));
        var updated = await updateResponse.Content.ReadFromJsonAsync<DashboardOfferingDetailDto>();

        Assert.Equal(2, updated!.Sessions.Count);
        Assert.Contains(updated.Sessions, s => s.Location == "جلسه یک (اصلاح‌شده)");
        Assert.Contains(updated.Sessions, s => s.Location == "جلسه سه");
        Assert.DoesNotContain(updated.Sessions, s => s.Location == "جلسه دو");
    }

    [Fact]
    public async Task Delete_removes_the_offering()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "offering-admin-4");
        var createResponse = await client.PostAsJsonAsync("/api/dashboard/offerings", new UpsertOfferingRequest(
            "", null, null, "Draft", "حذف‌شدنی", "سڕاوە", "", "", "<p>x</p>", "<p>x</p>", "", "", "", "", []));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardOfferingDetailDto>();

        var deleteResponse = await client.DeleteAsync($"/api/dashboard/offerings/{created!.Id}");
        var getResponse = await client.GetAsync($"/api/dashboard/offerings/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
