using Amazon.S3;
using Amazon.S3.Model;
using Backend.Core.Utils;

namespace Backend.Infrastructure.Utils;

// TODO: Supply the configured image bucket name when constructing/registering this service.
public sealed class S3ImageStorageClient(IAmazonS3 s3Client, string bucketName) : IImageStorageClient
{
    public async Task UploadAsync(
        Stream imageStream,
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);

        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new ArgumentException("An S3 object key is required for image uploads.", nameof(objectKey));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("A content type is required for image uploads.", nameof(contentType));
        }

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new InvalidOperationException("Configure the S3 image bucket name before using this helper.");
        }

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            InputStream = imageStream,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await s3Client.PutObjectAsync(request, cancellationToken);
    }
}
