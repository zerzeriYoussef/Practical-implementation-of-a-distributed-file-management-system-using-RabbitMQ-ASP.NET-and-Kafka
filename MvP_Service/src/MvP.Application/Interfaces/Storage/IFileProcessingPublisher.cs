using MvP.Application.Messaging;

namespace MvP.Application.Interfaces.Storage;

public interface IFileProcessingPublisher
{
    Task PublishAsync(FileProcessingMessage message, CancellationToken cancellationToken = default);
}
