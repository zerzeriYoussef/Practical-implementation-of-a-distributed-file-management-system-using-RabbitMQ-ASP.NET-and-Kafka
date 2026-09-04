using MvP.Application.Interfaces.Storage;

namespace MvP.Application.Services.Storage;

public sealed class S3StorageKeyBuilder : IStorageKeyBuilder
{
    public string BuildTeamFileKey(Guid teamId, Guid ownerId, Guid fileId, string fileName)
    {
        if (teamId == Guid.Empty)
        {
            throw new ArgumentException("Team id is required.", nameof(teamId));
        }

        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner id is required.", nameof(ownerId));
        }

        if (fileId == Guid.Empty)
        {
            throw new ArgumentException("File id is required.", nameof(fileId));
        }

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        return $"teams/{teamId:D}/owners/{ownerId:D}/files/{fileId:D}/{safeFileName}";
    }
}
