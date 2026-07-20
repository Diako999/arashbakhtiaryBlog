using SixLabors.ImageSharp;

namespace ArashBlog.Api.Common;

// Mirrors apps/core/validators.py's validate_image_file — checks what the
// file actually *is* (via ImageSharp decoding it), not what the client's
// filename or Content-Type header claims. Returns the detected format name
// on success so the caller can pick a safe extension itself rather than
// trusting the client-supplied filename for that too.
public static class ImageValidator
{
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedFormats = new(StringComparer.OrdinalIgnoreCase) { "jpeg", "png", "webp" };

    public static async Task<(string? Error, string? Format)> ValidateAsync(Stream stream, long length)
    {
        if (length > MaxSizeBytes)
        {
            return ($"File too large. Max size is {MaxSizeBytes / (1024 * 1024)} MB.", null);
        }

        try
        {
            stream.Position = 0;
            var format = await Image.DetectFormatAsync(stream);
            if (format is null || !AllowedFormats.Contains(format.Name))
            {
                return ("Unsupported image type.", null);
            }

            // Fully decode, not just sniff the header — catches truncated
            // or otherwise corrupt files, the same integrity check Pillow's
            // img.verify() performs in the Django source this mirrors.
            stream.Position = 0;
            using var image = await Image.LoadAsync(stream);

            return (null, format.Name.ToLowerInvariant());
        }
        catch
        {
            return ("Unsupported or corrupt image file.", null);
        }
        finally
        {
            stream.Position = 0;
        }
    }
}
