using System.Text.Json;
using Microsoft.Extensions.Options;
using MvP.Application.Messaging;
using MvP.Application.Services.Storage;
using MvP.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MvP.Worker;

public sealed class FileProcessingWorker : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FileProcessingWorker> _logger;

    public FileProcessingWorker(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<FileProcessingWorker> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            ConsumerDispatchConcurrency = _options.WorkerConcurrency
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync();

        await RabbitMqTopology.DeclareAsync(channel, stoppingToken);
        await channel.BasicQosAsync(0, _options.PrefetchCount, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, delivery) =>
        {
            FileProcessingMessage? message = null;

            try
            {
                message = JsonSerializer.Deserialize<FileProcessingMessage>(delivery.Body.Span)
                    ?? throw new InvalidOperationException("The file-processing message is invalid.");

                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IFileMetadataProcessor>();

                await processor.ProcessAsync(message.FileId, stoppingToken);

                // The job is removed only after S3 metadata and database status are saved.
                await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken);
                _logger.LogInformation("File {FileId} processed.", message.FileId);
            }
            catch (InvalidOperationException exception)
            {
                if (message is not null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IFileMetadataProcessor>();
                    await processor.MarkFailedAsync(message.FileId, exception.Message, stoppingToken);
                }

                // A permanent failure is routed to storage.file-processing.failed.
                await channel.BasicNackAsync(delivery.DeliveryTag, false, false, stoppingToken);
                _logger.LogWarning(exception, "File-processing message was dead-lettered.");
            }
            catch (Exception exception)
            {
                // Transient failures remain available for another consumer attempt.
                await channel.BasicNackAsync(delivery.DeliveryTag, false, true, stoppingToken);
                _logger.LogError(exception, "File-processing message was requeued.");
            }
        };

        await channel.BasicConsumeAsync(
            RabbitMqTopology.ProcessingQueue,
            autoAck: false,
            consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
