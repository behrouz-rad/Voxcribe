// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Engine.Contracts;

/// <summary>
/// Converts media files to audio formats suitable for speech recognition.
/// </summary>
public interface IMediaProcessor
{
    /// <summary>
    /// Ensures the media processing dependencies are available.
    /// </summary>
    /// <param name="progress">Progress reporter for initialization tasks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task InitializeAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts and converts audio from a media file to the specified format.
    /// </summary>
    /// <param name="sourceFilePath">Path to the input media file.</param>
    /// <param name="targetSampleRate">Target sample rate in Hz (default: 16000).</param>
    /// <param name="targetChannels">Target number of channels (default: 1 for mono).</param>
    /// <param name="progress">Progress reporter (0.0 to 1.0).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Path to the extracted audio file.</returns>
    public Task<string> ExtractAudioAsync(
        string sourceFilePath,
        int targetSampleRate = 16000,
        int targetChannels = 1,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}
