// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Engine.Domain;

/// <summary>
/// Represents the current state of a transcription operation.
/// </summary>
public sealed record TranscriptionProgress
{
    public required double CompletionRatio { get; init; }
    public required string CurrentPhase { get; init; }
    public string? PartialText { get; init; }

    /// <summary>
    /// Gets the completion percentage (0-100).
    /// </summary>
    public double PercentComplete => Math.Clamp(CompletionRatio * 100, 0, 100);
}
