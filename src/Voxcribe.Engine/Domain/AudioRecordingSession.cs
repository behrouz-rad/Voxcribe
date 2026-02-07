// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Engine.Domain;

/// <summary>
/// Represents audio recording state and metadata.
/// </summary>
public sealed record AudioRecordingSession
{
    public required string SessionId { get; init; }
    public required DateTime StartedAt { get; init; }
    public DateTime? StoppedAt { get; init; }
    public string? OutputFilePath { get; init; }
    public TimeSpan Duration => (StoppedAt ?? DateTime.Now) - StartedAt;
}
