// © 2026 Behrouz Rad. All rights reserved.

using Voxcribe.Engine.Configuration;
using Voxcribe.Engine.Domain;

namespace Voxcribe.Engine.Contracts;

/// <summary>
/// High-level orchestration service for complete transcription workflows.
/// Coordinates media processing, model management, and speech recognition.
/// </summary>
public interface ITranscriptionOrchestrator
{
    /// <summary>
    /// Transcribes a media file end-to-end (handles audio extraction automatically).
    /// </summary>
    /// <param name="mediaFilePath">Path to any supported media file.</param>
    /// <param name="modelSize">The model to use.</param>
    /// <param name="options">Transcription options.</param>
    /// <param name="progress">Progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete transcription result.</returns>
    public Task<TranscriptionOutput> TranscribeMediaFileAsync(
        string mediaFilePath,
        ModelSize modelSize,
        TranscriptionOptions? options = null,
        IProgress<TranscriptionProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a model is available locally before transcription.
    /// </summary>
    public Task<bool> ValidateModelAvailabilityAsync(
        ModelSize modelSize,
        CancellationToken cancellationToken = default);
}
