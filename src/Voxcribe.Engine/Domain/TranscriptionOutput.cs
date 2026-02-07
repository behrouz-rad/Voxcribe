// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Engine.Domain;

/// <summary>
/// Represents a complete transcription result with metadata.
/// </summary>
public sealed record TranscriptionOutput
{
    public required string FullText { get; init; }
    public required TimeSpan ProcessingDuration { get; init; }
    public required ModelSize ModelUsed { get; init; }
    public required DateTime CompletedAt { get; init; }
    public string? SourceMediaPath { get; init; }
    public IReadOnlyList<TextSegment>? Segments { get; init; }
}
