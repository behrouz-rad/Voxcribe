// © 2026 Behrouz Rad. All rights reserved.

using Voxcribe.Engine.Domain;

namespace Voxcribe.Engine.Contracts;

/// <summary>
/// Records audio from input devices.
/// </summary>
public interface IAudioRecorder
{
    /// <summary>
    /// Indicates whether recording is currently active.
    /// </summary>
    public bool IsRecording { get; }

    /// <summary>
    /// Starts recording audio from the default input device.
    /// </summary>
    /// <param name="config">Recording configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recording session information.</returns>
    public Task<AudioRecordingSession> StartRecordingAsync(
        AudioRecordingConfig? config = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the current recording and saves to file.
    /// </summary>
    /// <returns>Path to the saved audio file.</returns>
    /// <exception cref="InvalidOperationException">Thrown if not currently recording.</exception>
    public Task<string> StopRecordingAsync();

    /// <summary>
    /// Cancels the current recording without saving.
    /// </summary>
    public void CancelRecording();

    /// <summary>
    /// Event raised periodically during recording with elapsed time.
    /// </summary>
    public event EventHandler<TimeSpan>? RecordingProgressChanged;
}
