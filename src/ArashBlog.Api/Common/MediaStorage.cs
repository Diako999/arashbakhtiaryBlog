namespace ArashBlog.Api.Common;

// Lives at ContentRoot/media — a sibling of, not inside, wwwroot. wwwroot
// is emptied and rebuilt by every `npm run build` (vite.config.ts's
// emptyOutDir), which would silently delete every uploaded file if media
// lived there instead. Mirrors Django's MEDIA_ROOT being separate from
// STATIC_ROOT for the same reason.
public static class MediaStorage
{
    public static async Task<string> SaveAsync(IWebHostEnvironment env, Stream stream, string extension, string category)
    {
        var now = DateTime.UtcNow;
        var relativeDir = $"{category}/{now:yyyy}/{now:MM}";
        var absoluteDir = Path.Combine(env.ContentRootPath, "media", category, now.ToString("yyyy"), now.ToString("MM"));
        Directory.CreateDirectory(absoluteDir);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteDir, fileName);

        stream.Position = 0;
        await using var fileStream = new FileStream(absolutePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        return $"/media/{relativeDir}/{fileName}";
    }
}
