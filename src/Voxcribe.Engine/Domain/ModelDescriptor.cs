// © 2026 Behrouz Rad. All rights reserved.

using System.Globalization;

namespace Voxcribe.Engine.Domain;

/// <summary>
/// Metadata and status information for a speech recognition model.
/// </summary>
public sealed record ModelDescriptor
{
    public required ModelSize Size { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required long SizeInBytes { get; init; }
    public required bool IsAvailableLocally { get; init; }
    public string? LocalFilePath { get; init; }

    /// <summary>
    /// Gets a human-readable representation of the model size.
    /// </summary>
    public string FormattedSize
    {
        get
        {
            ReadOnlySpan<string> units = ["B", "KB", "MB", "GB", "TB"];
            double size = SizeInBytes;
            int unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:F2} {1}", size, units[unitIndex]);
        }
    }
}
