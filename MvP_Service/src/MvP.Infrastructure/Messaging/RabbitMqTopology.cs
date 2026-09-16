using RabbitMQ.Client;

namespace MvP.Infrastructure.Messaging;

public static class RabbitMqTopology
{
    public const string Exchange = "storage.events";
    public const string ProcessingQueue = "storage.file-processing";
    public const string DeadLetterExchange = "storage.dlx";
    public const string DeadLetterQueue = "storage.file-processing.failed";
    public const string RoutingKey = "storage.file.queued";

    public static async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, true, false, null, false, false, cancellationToken);
        await channel.ExchangeDeclareAsync(DeadLetterExchange, ExchangeType.Direct, true, false, null, false, false, cancellationToken);

        await channel.QueueDeclareAsync(
            ProcessingQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = DeadLetterExchange
            },
            passive: false,
            noWait: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(DeadLetterQueue, true, false, false, null, false, false, cancellationToken);
        await channel.QueueBindAsync(ProcessingQueue, Exchange, RoutingKey, null, false, cancellationToken);
        await channel.QueueBindAsync(DeadLetterQueue, DeadLetterExchange, ProcessingQueue, null, false, cancellationToken);
    }
}
