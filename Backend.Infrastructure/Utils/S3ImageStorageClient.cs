using Amazon.S3;
using Amazon.S3.Model;
using Backend.Core.Utils;

namespace Backend.Infrastructure.Utils;

// TODO: Supply the configured image bucket name when constructing/registering this service.
public sealed class S3ImageStorageClient : IImageStorageClient
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3ImageStorageClient(IAmazonS3 s3Client, string bucketName)
    {
        ArgumentNullException.ThrowIfNull(s3Client);
        ArgumentException.ThrowIfNullOrWhiteSpace(bucketName, nameof(bucketName));

        _s3Client = s3Client;
        _bucketName = bucketName;
    }

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

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = imageStream,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }
}
