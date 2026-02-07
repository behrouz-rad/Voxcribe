// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Engine.Domain;

/// <summary>
/// Represents a timed segment of transcribed text.
/// </summary>
public sealed record TextSegment
{
    public required string Text { get; init; }
    public required TimeSpan Start { get; init; }
    public required TimeSpan End { get; init; }
    public TimeSpan Duration => End - Start;
}
