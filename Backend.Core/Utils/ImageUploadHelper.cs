namespace Backend.Core.Utils;

public static class ImageUploadHelper
{
    private static readonly Dictionary<string, string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp"
    };

    public static async Task<string> UploadImageAsync(
        IImageStorageClient storageClient,
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(storageClient);
        ArgumentNullException.ThrowIfNull(imageStream);

        if (!imageStream.CanRead)
        {
            throw new ArgumentException("The provided image stream must be readable.", nameof(imageStream));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("A file name is required for image uploads.", nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("A content type is required for image uploads.", nameof(contentType));
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageContentTypes.TryGetValue(extension, out var expectedContentType))
        {
            throw new ArgumentException("Only .jpg, .jpeg, .png, .gif, and .webp images are supported.", nameof(fileName));
        }

        if (!string.Equals(expectedContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The provided content type does not match the image file extension.", nameof(contentType));
        }

        var objectKey = $"images/{SanitizeFileName(fileName)}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        await storageClient.UploadAsync(imageStream, objectKey, expectedContentType, cancellationToken);

        return objectKey;
    }

    private static string SanitizeFileName(string fileName)
    {
        var rawFileName = Path.GetFileNameWithoutExtension(fileName);
        var normalizedCharacters = rawFileName
            .Select(character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-')
            .ToArray();

        var sanitizedFileName = string.Join(
            "-",
            new string(normalizedCharacters)
                .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return string.IsNullOrWhiteSpace(sanitizedFileName) ? "image" : sanitizedFileName;
    }
}
