namespace Backend.Core.Utils;

public interface IImageStorageClient
{
    Task UploadAsync(
        Stream imageStream,
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default);
}
