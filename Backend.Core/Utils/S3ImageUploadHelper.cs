using Amazon.S3;
using Amazon.S3.Model;

namespace Backend.Core.Utils;

public static class S3ImageUploadHelper
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
        IAmazonS3 s3Client,
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(s3Client);
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

        const string bucketName = "YOUR_S3_BUCKET_NAME";
        // TODO: Replace the bucket name above with your image bucket configuration value.

        if (string.Equals(bucketName, "YOUR_S3_BUCKET_NAME", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Configure the S3 image bucket name before using this helper.");
        }

        var safeFileName = Path.GetFileNameWithoutExtension(fileName);
        var normalizedFileName = string.Join(
            "-",
            safeFileName
                .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizedFileName))
        {
            normalizedFileName = "image";
        }

        var objectKey = $"images/{normalizedFileName}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            InputStream = imageStream,
            ContentType = expectedContentType,
            AutoCloseStream = false
        };

        await s3Client.PutObjectAsync(request, cancellationToken);

        return objectKey;
    }
}
