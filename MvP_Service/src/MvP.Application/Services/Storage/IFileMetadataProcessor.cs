namespace MvP.Application.Services.Storage;

public interface IFileMetadataProcessor
{
    Task ProcessAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task MarkFailedAsync(Guid fileId, string reason, CancellationToken cancellationToken = default);
}
