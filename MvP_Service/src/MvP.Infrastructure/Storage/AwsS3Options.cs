namespace MvP.Infrastructure.Storage;

public sealed class AwsS3Options
{
    public const string SectionName = "AwsS3";

    public string BucketName { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
}
