using MvP.Application.Interfaces.Storage;
using MvP.Domain.Entities.Storage;
using Microsoft.EntityFrameworkCore;

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

    public Task<StoredFile?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return _db.StoredFiles
            .FirstOrDefaultAsync(file => file.Id == fileId && file.DeletedAt == null, cancellationToken);
    }

    public async Task<IReadOnlyCollection<StoredFile>> GetByTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        return await _db.StoredFiles
            .AsNoTracking()
            .Where(file => file.TeamId == teamId && file.DeletedAt == null)
            .OrderByDescending(file => file.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
