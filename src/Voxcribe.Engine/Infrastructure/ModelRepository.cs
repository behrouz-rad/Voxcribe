// © 2026 Behrouz Rad. All rights reserved.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voxcribe.Engine.Configuration;
using Voxcribe.Engine.Contracts;
using Voxcribe.Engine.Domain;

namespace Voxcribe.Engine.Infrastructure;

/// <summary>
/// Manages Whisper model storage, downloads, and metadata.
/// </summary>
public sealed class ModelRepository : IModelRepository
{
    private readonly EngineConfiguration _config;
    private readonly ILogger<ModelRepository> _logger;
    private readonly HttpClient _httpClient;

    private static readonly IReadOnlyDictionary<ModelSize, ModelMetadata> Metadata =
        new Dictionary<ModelSize, ModelMetadata>
        {
            [ModelSize.Tiny] = new("Tiny", "Fastest, lowest accuracy", 77_000_000L, "ggml-tiny.bin"),
            [ModelSize.Base] = new("Base", "Fast, good for English", 148_000_000L, "ggml-base.bin"),
            [ModelSize.Small] = new("Small", "Balanced speed & accuracy", 488_000_000L, "ggml-small.bin"),
            [ModelSize.Medium] = new("Medium", "High accuracy", 1_500_000_000L, "ggml-medium.bin"),
            [ModelSize.LargeV3] = new("Large V3", "Best accuracy, slowest", 3_100_000_000L, "ggml-large-v3.bin")
        };

    public ModelRepository(
        IOptions<EngineConfiguration> config,
        ILogger<ModelRepository> logger,
        HttpClient? httpClient = null)
    {
        _config = config.Value;
        _logger = logger;
        _httpClient = httpClient ?? new HttpClient
        {
            Timeout = _config.DownloadTimeout
        };

        _config.EnsureDirectoriesExist();
    }

    public Task<IReadOnlyList<ModelDescriptor>> GetAllModelsAsync(CancellationToken cancellationToken = default)
    {
        var descriptors = Metadata.Keys
            .Select(CreateDescriptor)
            .ToList();

        return Task.FromResult<IReadOnlyList<ModelDescriptor>>(descriptors);
    }

    public Task<ModelDescriptor> GetModelAsync(ModelSize size, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateDescriptor(size));
    }

    public Task<string> GetModelFilePathAsync(ModelSize size, CancellationToken cancellationToken = default)
    {
        var filePath = GetLocalPath(size);

        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            throw new InvalidOperationException(
                $"Model {size} is not available locally. Download it first using {nameof(AcquireModelAsync)}.");
        }

        return Task.FromResult(filePath);
    }

    public async Task AcquireModelAsync(
        ModelSize size,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var metadata = Metadata[size];
        var localPath = GetLocalPath(size);
        var downloadUrl = GetDownloadUrl(metadata.FileName);

        _logger.LogInformation("Starting download of {ModelSize} from {Url}", size, downloadUrl);

        try
        {
            using var response = await _httpClient.GetAsync(
                downloadUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var buffer = new byte[_config.FileBufferSize];
            long totalBytesRead = 0;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(
                localPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                _config.FileBufferSize,
                useAsync: true);

            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;

                if (totalBytes > 0)
                {
                    progress?.Report((double)totalBytesRead / totalBytes);
                }
            }

            _logger.LogInformation("Successfully downloaded {ModelSize} to {Path}", size, localPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download {ModelSize}", size);

            // Clean up partial download
            if (File.Exists(localPath))
            {
                try { File.Delete(localPath); }
                catch { /* Ignore cleanup errors */ }
            }

            throw;
        }
    }

    public Task RemoveModelAsync(ModelSize size, CancellationToken cancellationToken = default)
    {
        var filePath = GetLocalPath(size);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Deleted model {ModelSize} from {Path}", size, filePath);
        }

        return Task.CompletedTask;
    }

    private ModelDescriptor CreateDescriptor(ModelSize size)
    {
        var metadata = Metadata[size];
        var localPath = GetLocalPath(size);
        var isAvailable = File.Exists(localPath) && new FileInfo(localPath).Length > 0;

        return new ModelDescriptor
        {
            Size = size,
            Name = metadata.DisplayName,
            Description = metadata.Description,
            SizeInBytes = metadata.EstimatedBytes,
            IsAvailableLocally = isAvailable,
            LocalFilePath = isAvailable ? localPath : null
        };
    }

    private string GetLocalPath(ModelSize size) =>
        Path.Combine(_config.ModelsDirectory, Metadata[size].FileName);

    private static string GetDownloadUrl(string fileName) =>
        $"https://huggingface.co/ggerganov/whisper.cpp/resolve/main/{fileName}";

    private sealed record ModelMetadata(
        string DisplayName,
        string Description,
        long EstimatedBytes,
        string FileName);
}
