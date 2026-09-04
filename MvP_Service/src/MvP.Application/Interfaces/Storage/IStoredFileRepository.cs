using MvP.Domain.Entities.Storage;

namespace MvP.Application.Interfaces.Storage;

public interface IStoredFileRepository
{
    Task AddAsync(StoredFile file, CancellationToken cancellationToken = default);
}
