// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Desktop.Services;

public interface IFileService
{
    /// <summary>
    /// Open file picker for media files
    /// </summary>
    public Task<string?> PickMediaFileAsync();

    /// <summary>
    /// Save transcription to file
    /// </summary>
    public Task<bool> SaveTranscriptionAsync(string text, string? suggestedFileName = null);

    /// <summary>
    /// Copy text to clipboard
    /// </summary>
    public Task CopyToClipboardAsync(string text);
}
