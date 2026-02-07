// © 2026 Behrouz Rad. All rights reserved.

using Microsoft.Extensions.Logging;
using Voxcribe.Engine.Configuration;
using Voxcribe.Engine.Contracts;
using Voxcribe.Engine.Domain;

namespace Voxcribe.Engine.Services;

/// <summary>
/// High-level orchestrator that coordinates the complete transcription workflow.
/// </summary>
public sealed class TranscriptionOrchestrator(
    IModelRepository modelRepository,
    IMediaProcessor mediaProcessor,
    ISpeechRecognitionEngine recognitionEngine,
    ILogger<TranscriptionOrchestrator> logger) : ITranscriptionOrchestrator
{
    public async Task<TranscriptionOutput> TranscribeMediaFileAsync(
        string mediaFilePath,
        ModelSize modelSize,
        TranscriptionOptions? options = null,
        IProgress<TranscriptionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(mediaFilePath))
        {
            throw new FileNotFoundException("Media file not found", mediaFilePath);
        }

        if (!await ValidateModelAvailabilityAsync(modelSize, cancellationToken))
        {
            throw new InvalidOperationException(
                $"Model {modelSize} is not available. Download it first.");
        }

        logger.LogInformation(
            "Starting transcription workflow for {File} with {Model}",
            mediaFilePath, modelSize);

        string? extractedAudioPath = null;

        try
        {
            // Phase 1: Extract/Convert Audio (0% - 20%)
            progress?.Report(new TranscriptionProgress
            {
                CompletionRatio = 0,
                CurrentPhase = "Preparing audio..."
            });

            var extractionProgress = new Progress<double>(ratio =>
            {
                progress?.Report(new TranscriptionProgress
                {
                    CompletionRatio = ratio * 0.2,
                    CurrentPhase = "Converting audio format..."
                });
            });

            extractedAudioPath = await mediaProcessor.ExtractAudioAsync(
                mediaFilePath,
                progress: extractionProgress,
                cancellationToken: cancellationToken);

            // Phase 2: Transcribe (20% - 100%)
            var transcriptionProgress = new Progress<TranscriptionProgress>(p =>
            {
                progress?.Report(new TranscriptionProgress
                {
                    CompletionRatio = 0.2 + (p.CompletionRatio * 0.8),
                    CurrentPhase = p.CurrentPhase,
                    PartialText = p.PartialText
                });
            });

            var result = await recognitionEngine.TranscribeAsync(
                extractedAudioPath,
                modelSize,
                options,
                transcriptionProgress,
                cancellationToken);

            logger.LogInformation("Transcription workflow completed successfully");

            return result with { SourceMediaPath = mediaFilePath };
        }
        finally
        {
            if (extractedAudioPath != null && File.Exists(extractedAudioPath))
            {
                try
                {
                    File.Delete(extractedAudioPath);
                    logger.LogDebug("Cleaned up temporary file: {Path}", extractedAudioPath);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to delete temporary file: {Path}", extractedAudioPath);
                }
            }
        }
    }

    public async Task<bool> ValidateModelAvailabilityAsync(
        ModelSize modelSize,
        CancellationToken cancellationToken = default)
    {
        var descriptor = await modelRepository.GetModelAsync(modelSize, cancellationToken);
        return descriptor.IsAvailableLocally;
    }
}
