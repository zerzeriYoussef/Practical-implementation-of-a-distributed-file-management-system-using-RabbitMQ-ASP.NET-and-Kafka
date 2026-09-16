using Amazon;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MvP.Application.Interfaces.Storage;
using MvP.Application.Services.Storage;
using MvP.Infrastructure.Messaging;
using MvP.Infrastructure.Persistence;
using MvP.Infrastructure.Storage;
using MvP.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IStoredFileRepository, StoredFileRepository>();
builder.Services.Configure<AwsS3Options>(builder.Configuration.GetSection(AwsS3Options.SectionName));
builder.Services.AddSingleton<IAmazonS3>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AwsS3Options>>().Value;
    return new AmazonS3Client(RegionEndpoint.GetBySystemName(options.Region));
});
builder.Services.AddScoped<IObjectStorageService, S3ObjectStorageService>();
builder.Services.AddScoped<IFileMetadataProcessor, FileMetadataProcessor>();

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddHostedService<FileProcessingWorker>();

await builder.Build().RunAsync();
