using MvP.Application.DTOs;
using MvP.Application.Interfaces;
using MvP.Application.Interfaces.Storage;
using MvP.Domain.Entities.Storage;
using MvP.Domain.Enums.Teams;

namespace MvP.Application.Services.Storage;

public sealed class FileUploadService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IStorageKeyBuilder _storageKeyBuilder;
    private readonly IObjectStorageService _objectStorageService;
    private readonly ICurrentUser _currentUser;

    public FileUploadService(
        ITeamRepository teamRepository,
        IStoredFileRepository storedFileRepository,
        IStorageKeyBuilder storageKeyBuilder,
        IObjectStorageService objectStorageService,
        ICurrentUser currentUser)
    {
        _teamRepository = teamRepository;
        _storedFileRepository = storedFileRepository;
        _storageKeyBuilder = storageKeyBuilder;
        _objectStorageService = objectStorageService;
        _currentUser = currentUser;
    }

    public async Task<UploadTeamFileResponse> UploadTeamFileAsync(
        Guid teamId,
        string fileName,
        string contentType,
        long sizeBytes,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        if (sizeBytes <= 0)
        {
            throw new ArgumentException("File is empty.", nameof(sizeBytes));
        }

        var userId = _currentUser.UserId;
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            throw new InvalidOperationException("Team not found.");
        }

        if (!team.OwnerId.HasValue)
        {
            throw new InvalidOperationException("Team owner is missing.");
        }

        if (team.OwnerId.Value != userId)
        {
            var membership = await _teamRepository.GetMembershipAsync(teamId, userId);
            if (membership == null || !membership.Permissions.HasFlag(TeamMemberPermission.Upload))
            {
                throw new UnauthorizedAccessException("You do not have upload permission for this team.");
            }
        }

        var fileId = Guid.NewGuid();
        var s3Key = _storageKeyBuilder.BuildTeamFileKey(teamId, team.OwnerId.Value, fileId, fileName);
        var storedFile = new StoredFile
        {
            Id = fileId,
            TeamId = teamId,
            OwnerUserId = team.OwnerId.Value,
            UploadedByUserId = userId,
            S3Bucket = _objectStorageService.BucketName,
            S3Key = s3Key,
            FileName = Path.GetFileName(fileName),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            SizeBytes = sizeBytes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _objectStorageService.UploadAsync(s3Key, content, storedFile.ContentType, cancellationToken);
        await _storedFileRepository.AddAsync(storedFile, cancellationToken);

        return new UploadTeamFileResponse(
            storedFile.Id,
            storedFile.TeamId,
            storedFile.OwnerUserId,
            storedFile.UploadedByUserId,
            storedFile.S3Bucket,
            storedFile.S3Key,
            storedFile.FileName,
            storedFile.ContentType,
            storedFile.SizeBytes,
            storedFile.CreatedAt);
    }
}
