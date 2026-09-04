using MvP.Application.Interfaces.Storage;
using MvP.Domain.Entities.Storage;

namespace MvP.Infrastructure.Persistence;

public sealed class StoredFileRepository : IStoredFileRepository
{
    private readonly AppDbContext _db;

    public StoredFileRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(StoredFile file, CancellationToken cancellationToken = default)
    {
        await _db.StoredFiles.AddAsync(file, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
