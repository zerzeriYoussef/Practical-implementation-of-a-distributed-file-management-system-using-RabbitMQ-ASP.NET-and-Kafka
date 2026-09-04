namespace MvP.Application.Interfaces.Storage;

public interface IStorageKeyBuilder
{
    string BuildTeamFileKey(Guid teamId, Guid ownerId, Guid fileId, string fileName);
}
