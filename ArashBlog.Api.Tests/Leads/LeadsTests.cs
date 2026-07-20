using System.Linq;
using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using ArashBlog.Api.Features.Leads;
using Xunit;

namespace ArashBlog.Api.Tests.Leads;

public class LeadsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task List_404s_while_hidden()
    {
        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "leads", false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/leads");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Submit_creates_a_submission_visible_in_the_dashboard_inbox()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "lead-admin-1");
        var createResponse = await admin.PostAsJsonAsync("/api/dashboard/leads", new UpsertLeadMagnetRequest(
            "", null, "https://example.com/guide.pdf", "Published",
            "راهنمای رایگان", "ڕێنمایی بەخۆڕایی", "توضیح", "وردەکاری", "", "", "", ""));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardLeadMagnetDetailDto>();

        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "leads", true);
        var client = factory.CreateClient();

        var detail = await client.GetFromJsonAsync<LeadMagnetDetailDto>($"/api/leads/{created!.Slug}?lang=fa");
        Assert.Equal("https://example.com/guide.pdf", detail!.FileUrl);

        var submitResponse = await client.PostAsJsonAsync($"/api/leads/{created.Slug}/submit",
            new CreateSubmissionRequest("Jane", "jane@example.com"));
        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);

        var submissions = await admin.GetFromJsonAsync<List<DashboardSubmissionDto>>("/api/dashboard/leads/submissions");
        Assert.Contains(submissions!, s => s.Email == "jane@example.com" && !s.IsContacted);
    }

    [Fact]
    public async Task Toggle_contacted_flips_the_flag()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "lead-admin-2");
        var createResponse = await admin.PostAsJsonAsync("/api/dashboard/leads", new UpsertLeadMagnetRequest(
            "", null, "https://example.com/file.pdf", "Published", "منبع", "سەرچاوە", "", "", "", "", "", ""));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardLeadMagnetDetailDto>();

        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "leads", true);
        var client = factory.CreateClient();
        await client.PostAsJsonAsync($"/api/leads/{created!.Slug}/submit", new CreateSubmissionRequest("Sam", "sam@example.com"));

        var submissions = await admin.GetFromJsonAsync<List<DashboardSubmissionDto>>("/api/dashboard/leads/submissions");
        var submission = submissions!.Single(s => s.Email == "sam@example.com");

        var toggleResponse = await admin.PostAsync($"/api/dashboard/leads/submissions/{submission.Id}/toggle", null);
        Assert.Equal(HttpStatusCode.NoContent, toggleResponse.StatusCode);

        var afterToggle = await admin.GetFromJsonAsync<List<DashboardSubmissionDto>>("/api/dashboard/leads/submissions");
        Assert.True(afterToggle!.Single(s => s.Id == submission.Id).IsContacted);
    }

    [Fact]
    public async Task Csv_export_returns_a_csv_body()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "lead-admin-3");

        var response = await admin.GetAsync("/api/dashboard/leads/submissions/export");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("Name,Email,Lead magnet,Contacted,Submitted at", body);
    }
}
