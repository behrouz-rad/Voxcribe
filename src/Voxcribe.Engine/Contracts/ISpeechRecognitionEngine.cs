// © 2026 Behrouz Rad. All rights reserved.

using Voxcribe.Engine.Configuration;
using Voxcribe.Engine.Domain;

namespace Voxcribe.Engine.Contracts;

/// <summary>
/// Performs speech-to-text transcription on audio data.
/// </summary>
public interface ISpeechRecognitionEngine
{
    /// <summary>
    /// Transcribes audio from a file.
    /// </summary>
    /// <param name="audioFilePath">Path to the audio file (must be compatible format).</param>
    /// <param name="modelSize">The model to use for transcription.</param>
    /// <param name="options">Optional transcription options.</param>
    /// <param name="progress">Progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transcription result.</returns>
    public Task<TranscriptionOutput> TranscribeAsync(
        string audioFilePath,
        ModelSize modelSize,
        TranscriptionOptions? options = null,
        IProgress<TranscriptionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
