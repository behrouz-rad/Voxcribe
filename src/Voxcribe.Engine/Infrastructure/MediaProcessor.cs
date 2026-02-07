// © 2026 Behrouz Rad. All rights reserved.

using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voxcribe.Engine.Configuration;
using Voxcribe.Engine.Contracts;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Voxcribe.Engine.Infrastructure;

/// <summary>
/// Processes media files using FFmpeg for audio extraction and conversion.
/// </summary>
public sealed class MediaProcessor : IMediaProcessor
{
    private readonly EngineConfiguration _config;
    private readonly ILogger<MediaProcessor> _logger;
    private bool _isInitialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public MediaProcessor(
        IOptions<EngineConfiguration> config,
        ILogger<MediaProcessor> logger)
    {
        _config = config.Value;
        _logger = logger;
        _config.EnsureDirectoriesExist();
    }

    public async Task InitializeAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
            {
                return;
            }

            FFmpeg.SetExecutablesPath(_config.FFmpegDirectory);

            var ffmpegExecutable = Path.Combine(
                _config.FFmpegDirectory,
                OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg");

            if (!File.Exists(ffmpegExecutable))
            {
                _logger.LogInformation("FFmpeg not found. Downloading to {Path}", _config.FFmpegDirectory);
                progress?.Report("Downloading FFmpeg...");
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, _config.FFmpegDirectory);
                _logger.LogInformation("FFmpeg download complete");
                progress?.Report("FFmpeg download complete");
            }

            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<string> ExtractAudioAsync(
        string sourceFilePath,
        int targetSampleRate = 16000,
        int targetChannels = 1,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Source media file not found", sourceFilePath);
        }

        await InitializeAsync(progress: null, cancellationToken);

        var outputPath = Path.Combine(
            _config.TemporaryFilesDirectory,
            $"{Guid.NewGuid():N}.wav");

        _logger.LogInformation(
            "Extracting audio from {Source} to {Target} ({SampleRate}Hz, {Channels}ch)",
            sourceFilePath, outputPath, targetSampleRate, targetChannels);

        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(sourceFilePath, cancellationToken);

            if (mediaInfo.AudioStreams.Any())
            {
                var conversion = FFmpeg.Conversions.New()
                    .AddStream(mediaInfo.AudioStreams)
                    .SetOutput(outputPath)
                    .AddParameter(string.Format(CultureInfo.InvariantCulture, "-ar {0}", targetSampleRate))
                    .AddParameter(string.Format(CultureInfo.InvariantCulture, "-ac {0}", targetChannels))
                    .AddParameter("-c:a pcm_s16le");

                conversion.OnProgress += (sender, args) =>
                {
                    if (args.TotalLength.Ticks > 0)
                    {
                        var ratio = (double)args.Duration.Ticks / args.TotalLength.Ticks;
                        progress?.Report(Math.Clamp(ratio, 0.0, 1.0));
                    }
                };

                await conversion.Start(cancellationToken);

                _logger.LogInformation("Audio extraction completed: {Output}", outputPath);
            }
            else
            {
                throw new InvalidOperationException("No audio streams found in the media file");
            }

            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract audio from {Source}", sourceFilePath);

            // Clean up partial output
            if (File.Exists(outputPath))
            {
                try { File.Delete(outputPath); }
                catch { /* Ignore cleanup errors */ }
            }

            throw;
        }
    }
}
