using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using ArashBlog.Api.Features.Testimonials;
using Xunit;

namespace ArashBlog.Api.Tests.Testimonials;

public class TestimonialsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task List_404s_while_hidden_and_excludes_unapproved_once_visible()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "testimonial-admin-1");
        var createResponse = await admin.PostAsJsonAsync("/api/dashboard/testimonials", new UpsertTestimonialRequest(
            "Reader One", "دانشجو", "قوتابی", "عالی بود", "زۆر باش بوو", null, "", null));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardTestimonialDto>();

        var client = factory.CreateClient();
        var hiddenResponse = await client.GetAsync("/api/testimonials");
        Assert.Equal(HttpStatusCode.NotFound, hiddenResponse.StatusCode);

        await SectionVisibilityTestHelper.SetVisibleAsync(factory, "testimonials", true);

        var beforeApproval = await client.GetFromJsonAsync<List<TestimonialDto>>("/api/testimonials?lang=fa");
        Assert.DoesNotContain(beforeApproval!, t => t.AuthorName == "Reader One");

        await admin.PostAsync($"/api/dashboard/testimonials/{created!.Id}/toggle", null);

        var afterApproval = await client.GetFromJsonAsync<List<TestimonialDto>>("/api/testimonials?lang=fa");
        Assert.Contains(afterApproval!, t => t.AuthorName == "Reader One" && t.AuthorRole == "دانشجو");
    }

    [Fact]
    public async Task Move_up_swaps_order_with_the_previous_entry()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "testimonial-admin-2");
        var firstResponse = await admin.PostAsJsonAsync("/api/dashboard/testimonials", new UpsertTestimonialRequest(
            "First", "", "", "q1", "q1", null, "", null));
        var secondResponse = await admin.PostAsJsonAsync("/api/dashboard/testimonials", new UpsertTestimonialRequest(
            "Second", "", "", "q2", "q2", null, "", null));
        var first = await firstResponse.Content.ReadFromJsonAsync<DashboardTestimonialDto>();
        var second = await secondResponse.Content.ReadFromJsonAsync<DashboardTestimonialDto>();

        // Both tie on Order=0, so the list falls back to -CreatedAt: the
        // just-created "Second" sorts before "First" until an explicit move.
        var beforeList = await admin.GetFromJsonAsync<List<DashboardTestimonialDto>>("/api/dashboard/testimonials");
        var firstIndexBefore = beforeList!.FindIndex(t => t.Id == first!.Id);
        Assert.True(firstIndexBefore > 0);

        await admin.PostAsync($"/api/dashboard/testimonials/{first!.Id}/move/up", null);

        var afterList = await admin.GetFromJsonAsync<List<DashboardTestimonialDto>>("/api/dashboard/testimonials");
        var firstIndexAfter = afterList!.FindIndex(t => t.Id == first.Id);

        Assert.True(firstIndexAfter < firstIndexBefore);
    }

    [Fact]
    public async Task Delete_removes_the_testimonial()
    {
        var admin = await AuthTestHelper.CreateVerifiedClientAsync(factory, "testimonial-admin-3");
        var createResponse = await admin.PostAsJsonAsync("/api/dashboard/testimonials", new UpsertTestimonialRequest(
            "Deletable", "", "", "q", "q", null, "", null));
        var created = await createResponse.Content.ReadFromJsonAsync<DashboardTestimonialDto>();

        var deleteResponse = await admin.DeleteAsync($"/api/dashboard/testimonials/{created!.Id}");
        var listResponse = await admin.GetFromJsonAsync<List<DashboardTestimonialDto>>("/api/dashboard/testimonials");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.DoesNotContain(listResponse!, t => t.Id == created.Id);
    }
}
