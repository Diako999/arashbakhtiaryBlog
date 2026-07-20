using System.Net;
using System.Net.Http.Json;
using ArashBlog.Api.Features.Dashboard;
using Xunit;

namespace ArashBlog.Api.Tests.Dashboard;

public class UploadsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    // A real 2x2 PNG (generated via Pillow) — a hand-crafted minimal 1x1
    // PNG tried first here threw ImageSharp's InvalidImageContentException
    // despite having a valid signature; not every byte sequence that
    // *starts* like a PNG decodes as one, which is exactly the distinction
    // ImageValidator's full-decode step (not just header sniffing) exists
    // to catch.
    private static readonly byte[] ValidPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFklEQVR4nGP8z8DAwMDAxMDAwMDAAAANHQEDasKb6QAAAABJRU5ErkJggg==");

    private static readonly byte[] ValidPdf = "%PDF-1.4\n%dummy content for signature check only"u8.ToArray();

    [Fact]
    public async Task Image_upload_is_denied_without_2fa_verification()
    {
        var client = factory.CreateClient();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(ValidPng), "file", "cover.png");

        var response = await client.PostAsync("/api/dashboard/uploads/image", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Valid_png_is_accepted_and_returns_a_media_url()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "upload-admin-1");
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(ValidPng), "file", "cover.png");

        var response = await client.PostAsync("/api/dashboard/uploads/image", content);
        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("/media/images/", body!.Url);
        Assert.EndsWith(".png", body.Url);
    }

    [Fact]
    public async Task A_text_file_disguised_as_png_is_rejected()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "upload-admin-2");
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent("just plain text, not an image"u8.ToArray()), "file", "cover.png");

        var response = await client.PostAsync("/api/dashboard/uploads/image", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Oversized_image_is_rejected()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "upload-admin-3");
        var oversized = new byte[6 * 1024 * 1024];
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(oversized), "file", "huge.png");

        var response = await client.PostAsync("/api/dashboard/uploads/image", content);

        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.RequestEntityTooLarge,
            $"Expected 400 or 413, got {response.StatusCode}");
    }

    [Fact]
    public async Task Valid_pdf_is_accepted()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "upload-admin-4");
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(ValidPdf), "file", "guide.pdf");

        var response = await client.PostAsync("/api/dashboard/uploads/document", content);
        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("/media/documents/", body!.Url);
        Assert.EndsWith(".pdf", body.Url);
    }

    [Fact]
    public async Task A_file_without_the_pdf_signature_is_rejected_even_with_a_pdf_extension()
    {
        var client = await AuthTestHelper.CreateVerifiedClientAsync(factory, "upload-admin-5");
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent("not really a pdf"u8.ToArray()), "file", "fake.pdf");

        var response = await client.PostAsync("/api/dashboard/uploads/document", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
