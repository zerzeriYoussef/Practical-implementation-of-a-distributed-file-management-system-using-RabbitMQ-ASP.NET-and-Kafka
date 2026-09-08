using MvP.Domain.Entities.Storage;

namespace MvP.Application.Interfaces.Storage;

public interface IStoredFileRepository
{
    Task AddAsync(StoredFile file, CancellationToken cancellationToken = default);
    Task<StoredFile?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<StoredFile>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
