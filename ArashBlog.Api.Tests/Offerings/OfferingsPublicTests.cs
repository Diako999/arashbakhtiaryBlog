using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using ArashBlog.Api.Features.Offerings;
using Xunit;

namespace ArashBlog.Api.Tests.Offerings;

public class OfferingsPublicTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private async Task<HttpClient> AdminClientAsync(string username) =>
        await AuthTestHelper.CreateVerifiedClientAsync(factory, username);

    [Fact]
    public async Task List_404s_while_the_section_is_hidden()
    {
        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "offerings", false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/offerings");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_and_detail_work_once_visible_and_enrollment_round_trips()
    {
        var admin = await AdminClientAsync("offering-admin-1");
        var createResponse = await admin.PostAsJsonAsync("/api/dashboard/offerings", new UpsertOfferingRequest(
            "", null, null, 49.99m, "Published", "دوره برنامه‌نویسی", "کۆرسی بەرنامەسازی", "خلاصه", "کورتە",
            "<p>x</p>", "<p>x</p>", "", "", "", "",
            [new DashboardSessionDto(null, DateTimeOffset.UtcNow.AddDays(7), null, "آنلاین", 20)]));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardOfferingDetailDto>();

        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "offerings", true);
        var client = factory.CreateClient();

        var listResponse = await client.GetFromJsonAsync<List<OfferingSummaryDto>>("/api/offerings?lang=fa");
        Assert.Contains(listResponse!, o => o.Slug == created!.Slug);

        var detail = await client.GetFromJsonAsync<OfferingDetailDto>($"/api/offerings/{created!.Slug}?lang=fa");
        Assert.Single(detail!.Sessions);

        var enrollResponse = await client.PostAsJsonAsync($"/api/offerings/{created.Slug}/enroll",
            new CreateEnrollmentRequest(detail.Sessions[0].Id, "Learner", "learner@example.com", "", ""));
        Assert.Equal(HttpStatusCode.Created, enrollResponse.StatusCode);

        var dashboardDetail = await admin.GetFromJsonAsync<DashboardOfferingDetailDto>($"/api/dashboard/offerings/{created.Id}");
        Assert.Single(dashboardDetail!.Enrollments);
        Assert.Equal("Learner", dashboardDetail.Enrollments[0].Name);
    }

    [Fact]
    public async Task Enroll_rejects_a_session_that_does_not_belong_to_the_offering()
    {
        var admin = await AdminClientAsync("offering-admin-2");

        var offeringAResponse = await admin.PostAsJsonAsync("/api/dashboard/offerings", new UpsertOfferingRequest(
            "", null, null, null, "Published", "دوره الف", "کۆرسی ئەلف", "", "", "<p>x</p>", "<p>x</p>", "", "", "", "", []));
        var offeringA = await offeringAResponse.Content.ReadFromJsonAsync<DashboardOfferingDetailDto>();

        // A session under a completely different offering.
        var offeringBResponse = await admin.PostAsJsonAsync("/api/dashboard/offerings", new UpsertOfferingRequest(
            "", null, null, null, "Published", "دوره ب", "کۆرسی ب", "", "", "<p>x</p>", "<p>x</p>", "", "", "", "",
            [new DashboardSessionDto(null, DateTimeOffset.UtcNow.AddDays(1), null, "", null)]));
        var offeringB = await offeringBResponse.Content.ReadFromJsonAsync<DashboardOfferingDetailDto>();

        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "offerings", true);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/offerings/{offeringA!.Slug}/enroll",
            new CreateEnrollmentRequest(offeringB!.Sessions[0].Id, "X", "x@example.com", "", ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
