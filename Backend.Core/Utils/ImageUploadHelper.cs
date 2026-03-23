using System.Globalization;
using System.Text;

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
        string objectKeyPrefix = "images",
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

        if (string.IsNullOrWhiteSpace(objectKeyPrefix))
        {
            throw new ArgumentException("An object key prefix is required for image uploads.", nameof(objectKeyPrefix));
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

        var normalizedPrefix = objectKeyPrefix.Trim('/');
        var sanitizedFileName = SanitizeFileName(fileName);
        var lowerExtension = extension.ToLowerInvariant();
        var objectKey = $"{normalizedPrefix}/{sanitizedFileName}-{Guid.NewGuid():N}{lowerExtension}";

        await storageClient.UploadAsync(imageStream, objectKey, expectedContentType, cancellationToken);

        return objectKey;
    }

    private static string SanitizeFileName(string fileName)
    {
        var rawFileName = Path.GetFileNameWithoutExtension(fileName);
        var sanitizedBuilder = new StringBuilder(rawFileName.Length);
        var previousCharacterWasDash = false;

        foreach (var character in rawFileName)
        {
            if (char.IsLetterOrDigit(character))
            {
                sanitizedBuilder.Append(char.ToLower(character, CultureInfo.InvariantCulture));
                previousCharacterWasDash = false;
                continue;
            }

            if (!previousCharacterWasDash && sanitizedBuilder.Length > 0)
            {
                sanitizedBuilder.Append('-');
                previousCharacterWasDash = true;
            }
        }

        while (sanitizedBuilder.Length > 0 && sanitizedBuilder[^1] == '-')
        {
            sanitizedBuilder.Length--;
        }

        return sanitizedBuilder.Length == 0 ? "image" : sanitizedBuilder.ToString();
    }
}
