using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using MvP.Application.Interfaces.Storage;

namespace MvP.Infrastructure.Storage;

public sealed class S3ObjectStorageService : IObjectStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly AwsS3Options _options;

    public S3ObjectStorageService(IAmazonS3 s3Client, IOptions<AwsS3Options> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public string BucketName => _options.BucketName;

    public async Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("AWS S3 bucket name is not configured.");
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("S3 object key is required.", nameof(key));
        }

        if (content.CanSeek && content.Length == 0)
        {
            throw new ArgumentException("File content is empty.", nameof(content));
        }

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<ObjectStorageMetadata> GetMetadataAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.GetObjectMetadataAsync(
            _options.BucketName,
            key,
            cancellationToken);

        return new ObjectStorageMetadata(response.Headers.ContentType, response.ContentLength, response.ETag);
    }

    public Task<PresignedDownloadUrl> GenerateDownloadUrlAsync(
        string key,
        string downloadFileName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("AWS S3 bucket name is not configured.");
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("S3 object key is required.", nameof(key));
        }

        var safeFileName = Path.GetFileName(downloadFileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("Download file name is required.", nameof(downloadFileName));
        }

        var expirationMinutes = _options.PresignedUrlExpirationMinutes;
        if (expirationMinutes <= 0)
        {
            throw new InvalidOperationException("AWS S3 pre-signed URL expiration must be greater than zero.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = expiresAt,
            ResponseHeaderOverrides = new ResponseHeaderOverrides
            {
                ContentDisposition = $"attachment; filename=\"{safeFileName.Replace("\"", string.Empty)}\""
            }
        };

        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(new PresignedDownloadUrl(url, expiresAt));
    }
}
