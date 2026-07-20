namespace ArashBlog.Api.Common;

// Mirrors apps/core/validators.py's validate_document_file — a magic-byte
// signature check (the actual file content), not the client's Content-Type
// header or filename extension. Only PDF is accepted, matching
// ALLOWED_UPLOAD_DOCUMENT_TYPES in the Django source.
public static class DocumentValidator
{
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

    public static async Task<string?> ValidateAsync(Stream stream, long length)
    {
        if (length > MaxSizeBytes)
        {
            return $"File too large. Max size is {MaxSizeBytes / (1024 * 1024)} MB.";
        }

        stream.Position = 0;
        var header = new byte[PdfSignature.Length];
        var read = await stream.ReadAsync(header);
        stream.Position = 0;

        if (read < PdfSignature.Length || !header.AsSpan().SequenceEqual(PdfSignature))
        {
            return "Unsupported file type.";
        }

        return null;
    }
}
