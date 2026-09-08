namespace MvP.Application.DTOs;

public record StoredFileResponse(
    Guid FileId,
    Guid TeamId,
    Guid UploadedByUserId,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record FileDownloadResponse(string DownloadUrl, DateTime ExpiresAt);

public record RenameStoredFileRequest(string FileName);
