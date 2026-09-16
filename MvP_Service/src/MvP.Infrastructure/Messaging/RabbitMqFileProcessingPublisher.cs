using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MvP.Application.Interfaces.Storage;
using MvP.Application.Messaging;
using RabbitMQ.Client;

namespace MvP.Infrastructure.Messaging;

public sealed class RabbitMqFileProcessingPublisher : IFileProcessingPublisher, IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;

    public RabbitMqFileProcessingPublisher(IOptions<RabbitMqOptions> options)
    {
        var value = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = value.HostName,
            Port = value.Port,
            UserName = value.UserName,
            Password = value.Password
        };
    }

    public async Task PublishAsync(FileProcessingMessage message, CancellationToken cancellationToken = default)
    {
        _connection ??= await _factory.CreateConnectionAsync(cancellationToken);

        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        await using var channel = await _connection.CreateChannelAsync(channelOptions);
        await RabbitMqTopology.DeclareAsync(channel, cancellationToken);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString()
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: RabbitMqTopology.Exchange,
            routingKey: RabbitMqTopology.RoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
