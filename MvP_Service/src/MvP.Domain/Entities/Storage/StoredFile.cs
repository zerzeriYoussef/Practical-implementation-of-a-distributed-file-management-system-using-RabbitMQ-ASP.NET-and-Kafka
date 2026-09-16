using MvP.Domain.Entities.Teams;
using MvP.Domain.Enums.Storage;

namespace MvP.Domain.Entities.Storage;

public class StoredFile : BaseEntity
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public Guid OwnerUserId { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string S3Bucket { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public StoredFileStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public string? ObjectETag { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
