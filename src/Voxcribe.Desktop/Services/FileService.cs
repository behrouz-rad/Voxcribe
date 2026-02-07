// © 2026 Behrouz Rad. All rights reserved.

using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace Voxcribe.Desktop.Services;

public sealed class FileService : IFileService
{
    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    public async Task<string?> PickMediaFileAsync()
    {
        var window = GetMainWindow();
        if (window is null)
        {
            return null;
        }

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Media File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Media Files")
                {
                    Patterns = ["*.mp3", "*.mp4", "*.wav", "*.m4a", "*.mkv", "*.avi", "*.flac", "*.ogg", "*.webm"]
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    public async Task<bool> SaveTranscriptionAsync(string text, string? suggestedFileName = null)
    {
        var window = GetMainWindow();
        if (window is null)
        {
            return false;
        }

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var defaultName = suggestedFileName ?? $"transcription_{DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)}.txt";

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Transcription",
            SuggestedFileName = defaultName,
            DefaultExtension = "txt",
            FileTypeChoices =
            [
                new FilePickerFileType("Text File")
                {
                    Patterns = ["*.txt"]
                }
            ],
            SuggestedStartLocation = await window.StorageProvider.TryGetFolderFromPathAsync(desktopPath)
        });

        if (file != null)
        {
            await File.WriteAllTextAsync(file.Path.LocalPath, text);
            return true;
        }

        return false;
    }

    public async Task CopyToClipboardAsync(string text)
    {
        var window = GetMainWindow();
        if (window?.Clipboard != null)
        {
            await window.Clipboard.SetTextAsync(text);
        }
    }
}
