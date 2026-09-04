namespace MvP.Application.Interfaces.Storage;

public interface IObjectStorageService
{
    string BucketName { get; }

    Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);
}
