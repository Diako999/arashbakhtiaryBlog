using ArashBlog.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace ArashBlog.Api.Features.Dashboard;

public record UploadResponse(string Url);

// Mirrors apps/core/validators.py — real content-sniffing (ImageValidator/
// DocumentValidator), not trusting the client's Content-Type header or
// filename. Every cover-image/photo/logo/lead-magnet-file URL field
// elsewhere in the app can be filled by pasting a URL directly OR by
// uploading through here and using the URL this returns — the storage
// format never changed, this just gives admins a real upload path instead
// of "host it elsewhere yourself."
[ApiController]
[Route("api/dashboard/uploads")]
[RequireVerifiedTwoFactor]
public class UploadsController(IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("image")]
    [RequestSizeLimit(ImageValidator.MaxSizeBytes)]
    public async Task<ActionResult<UploadResponse>> UploadImage(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "file_required" });
        }

        using var stream = file.OpenReadStream();
        var (error, format) = await ImageValidator.ValidateAsync(stream, file.Length);
        if (error is not null)
        {
            return BadRequest(new { error });
        }

        var extension = format switch { "jpeg" => ".jpg", "png" => ".png", "webp" => ".webp", _ => "" };
        var url = await MediaStorage.SaveAsync(env, stream, extension, "images");
        return Ok(new UploadResponse(url));
    }

    [HttpPost("document")]
    [RequestSizeLimit(DocumentValidator.MaxSizeBytes)]
    public async Task<ActionResult<UploadResponse>> UploadDocument(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "file_required" });
        }

        using var stream = file.OpenReadStream();
        var error = await DocumentValidator.ValidateAsync(stream, file.Length);
        if (error is not null)
        {
            return BadRequest(new { error });
        }

        var url = await MediaStorage.SaveAsync(env, stream, ".pdf", "documents");
        return Ok(new UploadResponse(url));
    }
}
