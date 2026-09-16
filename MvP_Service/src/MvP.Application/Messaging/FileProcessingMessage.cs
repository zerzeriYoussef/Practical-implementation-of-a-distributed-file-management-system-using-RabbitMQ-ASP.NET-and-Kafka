namespace MvP.Application.Messaging;

public sealed record FileProcessingMessage(Guid FileId, Guid TeamId, string S3Key);
