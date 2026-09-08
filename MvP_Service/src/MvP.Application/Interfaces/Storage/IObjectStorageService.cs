namespace MvP.Application.Interfaces.Storage;

public interface IObjectStorageService
{
    string BucketName { get; }

    Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<PresignedDownloadUrl> GenerateDownloadUrlAsync(
        string key,
        string downloadFileName,
        CancellationToken cancellationToken = default);
}

public sealed record PresignedDownloadUrl(string Url, DateTime ExpiresAt);
