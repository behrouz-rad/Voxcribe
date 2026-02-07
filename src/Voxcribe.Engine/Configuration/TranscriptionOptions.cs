// © 2026 Behrouz Rad. All rights reserved.

using Voxcribe.Engine.Domain;

namespace Voxcribe.Engine.Configuration;

/// <summary>
/// Options for controlling transcription behavior.
/// </summary>
public sealed record TranscriptionOptions
{
    /// <summary>Language code (e.g., "en", "de"). Null for auto-detection.</summary>
    public string? Language { get; init; }

    /// <summary>Whether to include detailed segment timestamps.</summary>
    public bool IncludeSegments { get; init; }

    /// <summary>Callback invoked when each segment is detected (real-time).</summary>
    public Action<TextSegment>? OnSegmentDetected { get; init; }
}
