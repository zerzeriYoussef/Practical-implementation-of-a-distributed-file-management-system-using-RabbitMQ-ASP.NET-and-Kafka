using MvP.Application.DTOs;
using MvP.Application.Interfaces;
using MvP.Application.Interfaces.Storage;
using MvP.Domain.Entities.Storage;
using MvP.Domain.Enums.Teams;

namespace MvP.Application.Services.Storage;

public sealed class StorageService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IStorageKeyBuilder _storageKeyBuilder;
    private readonly IObjectStorageService _objectStorageService;
    private readonly ICurrentUser _currentUser;
    private readonly IFileProcessingPublisher _fileProcessingPublisher;

    public StorageService(
        ITeamRepository teamRepository,
        IStoredFileRepository storedFileRepository,
        IStorageKeyBuilder storageKeyBuilder,
        IObjectStorageService objectStorageService,
        ICurrentUser currentUser,
        IFileProcessingPublisher fileProcessingPublisher)
    {
        _teamRepository = teamRepository;
        _storedFileRepository = storedFileRepository;
        _storageKeyBuilder = storageKeyBuilder;
        _objectStorageService = objectStorageService;
        _currentUser = currentUser;
        _fileProcessingPublisher = fileProcessingPublisher;
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

        var team = await AuthorizeTeamAccessAsync(teamId, TeamMemberPermission.Upload);
        var fileId = Guid.NewGuid();
        var s3Key = _storageKeyBuilder.BuildTeamFileKey(teamId, team.OwnerId!.Value, fileId, fileName);
        var now = DateTime.UtcNow;
        var storedFile = new StoredFile
        {
            Id = fileId,
            TeamId = teamId,
            OwnerUserId = team.OwnerId.Value,
            UploadedByUserId = _currentUser.UserId,
            S3Bucket = _objectStorageService.BucketName,
            S3Key = s3Key,
            FileName = GetSafeFileName(fileName),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            SizeBytes = sizeBytes,
            Status = MvP.Domain.Enums.Storage.StoredFileStatus.Queued,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _objectStorageService.UploadAsync(s3Key, content, storedFile.ContentType, cancellationToken);
        await _storedFileRepository.AddAsync(storedFile, cancellationToken);
        await _fileProcessingPublisher.PublishAsync(
            new MvP.Application.Messaging.FileProcessingMessage(storedFile.Id, storedFile.TeamId, storedFile.S3Key),
            cancellationToken);

        return new UploadTeamFileResponse(
            storedFile.Id, storedFile.TeamId, storedFile.OwnerUserId, storedFile.UploadedByUserId,
            storedFile.S3Bucket, storedFile.S3Key, storedFile.FileName, storedFile.ContentType,
            storedFile.SizeBytes, storedFile.Status, storedFile.CreatedAt);
    }

    public async Task<IReadOnlyCollection<StoredFileResponse>> GetTeamFilesAsync(
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        await AuthorizeTeamAccessAsync(teamId, TeamMemberPermission.View);
        var files = await _storedFileRepository.GetByTeamIdAsync(teamId, cancellationToken);
        return files.Select(ToResponse).ToList();
    }

    public async Task<StoredFileResponse> GetTeamFileAsync(
        Guid teamId,
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        await AuthorizeTeamAccessAsync(teamId, TeamMemberPermission.View);
        var file = await GetTeamFileOrThrowAsync(teamId, fileId, cancellationToken);
        return ToResponse(file);
    }

    public async Task<FileDownloadResponse> GetTeamFileDownloadAsync(
        Guid teamId,
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        await AuthorizeTeamAccessAsync(teamId, TeamMemberPermission.Download);
        var file = await GetTeamFileOrThrowAsync(teamId, fileId, cancellationToken);
        var download = await _objectStorageService.GenerateDownloadUrlAsync(
            file.S3Key,
            file.FileName,
            cancellationToken);

        return new FileDownloadResponse(download.Url, download.ExpiresAt);
    }

    public async Task<StoredFileResponse> RenameTeamFileAsync(
        Guid teamId,
        Guid fileId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        await AuthorizeTeamAccessAsync(teamId, TeamMemberPermission.Edit);
        var file = await GetTeamFileOrThrowAsync(teamId, fileId, cancellationToken);
        file.FileName = GetSafeFileName(fileName);
        file.UpdatedAt = DateTime.UtcNow;
        await _storedFileRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(file);
    }

    private async Task<MvP.Domain.Entities.Teams.Team> AuthorizeTeamAccessAsync(
        Guid teamId,
        TeamMemberPermission permission)
    {
        var team = await _teamRepository.GetByIdAsync(teamId)
            ?? throw new InvalidOperationException("Team not found.");

        if (!team.OwnerId.HasValue)
        {
            throw new InvalidOperationException("Team owner is missing.");
        }

        if (team.OwnerId.Value == _currentUser.UserId)
        {
            return team;
        }

        var membership = await _teamRepository.GetMembershipAsync(teamId, _currentUser.UserId);
        if (membership == null || !membership.Permissions.HasFlag(permission))
        {
            throw new UnauthorizedAccessException($"You do not have {permission} permission for this team.");
        }

        return team;
    }

    private async Task<StoredFile> GetTeamFileOrThrowAsync(
        Guid teamId,
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var file = await _storedFileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.TeamId != teamId)
        {
            throw new InvalidOperationException("File not found.");
        }

        return file;
    }

    private static StoredFileResponse ToResponse(StoredFile file) => new(
        file.Id, file.TeamId, file.UploadedByUserId, file.FileName, file.ContentType,
        file.SizeBytes, file.Status, file.FailureReason, file.CreatedAt, file.UpdatedAt);

    private static string GetSafeFileName(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        if (safeFileName.Length > 255)
        {
            throw new ArgumentException("File name cannot exceed 255 characters.", nameof(fileName));
        }   

        return safeFileName;
    }
}
