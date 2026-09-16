using MvP.Application.Interfaces.Storage;
using MvP.Domain.Enums.Storage;

namespace MvP.Application.Services.Storage;

public sealed class FileMetadataProcessor : IFileMetadataProcessor
{
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IObjectStorageService _objectStorageService;

    public FileMetadataProcessor(
        IStoredFileRepository storedFileRepository,
        IObjectStorageService objectStorageService)
    {
        _storedFileRepository = storedFileRepository;
        _objectStorageService = objectStorageService;
    }

    public async Task ProcessAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _storedFileRepository.GetByIdAsync(fileId, cancellationToken)
            ?? throw new InvalidOperationException("Stored file was not found.");

        if (file.Status == StoredFileStatus.Completed)
        {
            return;
        }

        file.Status = StoredFileStatus.Processing;
        file.FailureReason = null;
        file.UpdatedAt = DateTime.UtcNow;
        await _storedFileRepository.SaveChangesAsync(cancellationToken);

        var metadata = await _objectStorageService.GetMetadataAsync(file.S3Key, cancellationToken);

        file.ContentType = string.IsNullOrWhiteSpace(metadata.ContentType)
            ? file.ContentType
            : metadata.ContentType;
        file.SizeBytes = metadata.SizeBytes;
        file.ObjectETag = metadata.ETag;
        file.Status = StoredFileStatus.Completed;
        file.ProcessedAt = DateTime.UtcNow;
        file.UpdatedAt = file.ProcessedAt.Value;

        await _storedFileRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid fileId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var file = await _storedFileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file is null)
        {
            return;
        }

        file.Status = StoredFileStatus.Failed;
        file.FailureReason = reason.Length <= 1000 ? reason : reason[..1000];
        file.UpdatedAt = DateTime.UtcNow;
        await _storedFileRepository.SaveChangesAsync(cancellationToken);
    }
}
