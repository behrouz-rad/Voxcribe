// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Engine.Domain;

/// <summary>
/// Represents different Whisper model variants with their performance characteristics.
/// </summary>
public enum ModelSize
{
    /// <summary>Fastest inference, lowest accuracy (~75 MB)</summary>
    Tiny,

    /// <summary>Fast inference, good for English (~142 MB)</summary>
    Base,

    /// <summary>Balanced speed/quality, better for multilingual (~466 MB)</summary>
    Small,

    /// <summary>High quality, excellent for complex audio (~1.5 GB)</summary>
    Medium,

    /// <summary>Best quality, slowest inference (~3.1 GB)</summary>
    LargeV3
}
