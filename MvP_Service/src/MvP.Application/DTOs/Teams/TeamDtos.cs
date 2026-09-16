namespace MvP.Application.DTOs;
using MvP.Domain.Enums.Storage;
public record CreateTeamRequest(string Name);
public record CreateTeamResponse(Guid TeamId, string Name, string JoinCode);

public record JoinTeamRequest(string JoinCode);
public record JoinTeamResponse(Guid TeamId, string Name);
public record TeamListItemResponse(Guid TeamId, string Name, string JoinCode, Guid? OwnerId, DateTime CreatedAt);
public record TeamMemberResponse(Guid UserId, string Role, DateTime JoinedAt);
public record TeamMembersResponse(Guid TeamId, string Name, Guid? OwnerId, IReadOnlyCollection<TeamMemberResponse> Members);
public record UploadTeamFileResponse(
    Guid FileId,
    Guid TeamId,
    Guid OwnerUserId,
    Guid UploadedByUserId,
    string S3Bucket,
    string S3Key,
    string FileName,
    string ContentType,
    long SizeBytes,
    StoredFileStatus Status,
    DateTime CreatedAt);
