// © 2026 Behrouz Rad. All rights reserved.

using Microsoft.Extensions.Logging;
using Voxcribe.Engine.Configuration;
using Voxcribe.Engine.Contracts;
using Voxcribe.Engine.Domain;
using Whisper.net;

namespace Voxcribe.Engine.Infrastructure;

/// <summary>
/// Speech recognition engine using Whisper.net.
/// </summary>
public sealed class SpeechRecognitionEngine(
    IModelRepository modelRepository,
    ILogger<SpeechRecognitionEngine> logger) : ISpeechRecognitionEngine
{
    public async Task<TranscriptionOutput> TranscribeAsync(
        string audioFilePath,
        ModelSize modelSize,
        TranscriptionOptions? options = null,
        IProgress<TranscriptionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new TranscriptionOptions();

        if (!File.Exists(audioFilePath))
        {
            throw new FileNotFoundException("Audio file not found", audioFilePath);
        }

        var modelPath = await modelRepository.GetModelFilePathAsync(modelSize, cancellationToken);

        logger.LogInformation(
            "Starting transcription of {File} using {Model}",
            audioFilePath, modelSize);

        var startTime = DateTime.Now;
        var segments = new List<TextSegment>();
        var fullTextBuilder = new List<string>();

        try
        {
            using var factory = WhisperFactory.FromPath(modelPath);

            var builder = factory.CreateBuilder()
                .WithLanguage(options.Language ?? "auto");

            using var processor = builder.Build();
            await using var audioStream = File.OpenRead(audioFilePath);

            var fileLength = audioStream.Length;

            progress?.Report(new TranscriptionProgress
            {
                CompletionRatio = 0,
                CurrentPhase = "Initializing transcription..."
            });

            await foreach (var segment in processor.ProcessAsync(audioStream, cancellationToken))
            {
                var textSegment = new TextSegment
                {
                    Text = segment.Text.Trim(),
                    Start = segment.Start,
                    End = segment.End
                };

                fullTextBuilder.Add(textSegment.Text);

                if (options.IncludeSegments)
                {
                    segments.Add(textSegment);
                }

                options.OnSegmentDetected?.Invoke(textSegment);

                // Estimate progress based on stream position
                if (fileLength > 0)
                {
                    var estimatedProgress = Math.Clamp(
                        (double)audioStream.Position / fileLength,
                        0.0,
                        1.0);

                    progress?.Report(new TranscriptionProgress
                    {
                        CompletionRatio = estimatedProgress,
                        CurrentPhase = "Transcribing...",
                        PartialText = textSegment.Text
                    });
                }
            }

            var completedAt = DateTime.Now;
            var fullText = string.Join(Environment.NewLine, fullTextBuilder);

            logger.LogInformation(
                "Transcription completed in {Duration:F2}s. Generated {CharCount} characters",
                (completedAt - startTime).TotalSeconds,
                fullText.Length);

            return new TranscriptionOutput
            {
                FullText = fullText,
                ProcessingDuration = completedAt - startTime,
                ModelUsed = modelSize,
                CompletedAt = completedAt,
                SourceMediaPath = audioFilePath,
                Segments = options.IncludeSegments ? segments : null
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transcription failed for {File}", audioFilePath);
            throw;
        }
    }
}
