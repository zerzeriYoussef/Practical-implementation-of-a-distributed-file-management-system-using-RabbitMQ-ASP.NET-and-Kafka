namespace MvP.Application.DTOs;
using MvP.Domain.Enums.Storage;

public record StoredFileResponse(
    Guid FileId,
    Guid TeamId,
    Guid UploadedByUserId,
    string FileName,
    string ContentType,
    long SizeBytes,
    StoredFileStatus Status,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record FileDownloadResponse(string DownloadUrl, DateTime ExpiresAt);

public record RenameStoredFileRequest(string FileName);
