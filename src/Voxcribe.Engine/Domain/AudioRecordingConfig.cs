// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Engine.Domain;

/// <summary>
/// Configuration for audio recording operations.
/// </summary>
public sealed record AudioRecordingConfig
{
    /// <summary>Sample rate in Hz (default: 16000 for Whisper compatibility)</summary>
    public int SampleRate { get; init; } = 16000;

    /// <summary>Number of audio channels (default: 1 for mono)</summary>
    public int Channels { get; init; } = 1;

    /// <summary>Bits per sample (default: 16)</summary>
    public int BitsPerSample { get; init; } = 16;
}
