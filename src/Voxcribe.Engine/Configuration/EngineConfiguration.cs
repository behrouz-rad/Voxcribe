// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Engine.Configuration;

/// <summary>
/// Configuration for the Voxcribe Engine.
/// </summary>
public sealed class EngineConfiguration
{
    /// <summary>
    /// Root directory for storing models and temporary files.
    /// Default: %LocalAppData%/Voxcribe
    /// </summary>
    public string StorageRootPath { get; set; } = GetDefaultStorageRoot();

    /// <summary>
    /// Directory where models are stored.
    /// Default: {StorageRootPath}/Models
    /// </summary>
    public string ModelsDirectory => Path.Combine(StorageRootPath, "Models");

    /// <summary>
    /// Directory where FFmpeg binaries are stored.
    /// Default: {StorageRootPath}/FFmpeg
    /// </summary>
    public string FFmpegDirectory => Path.Combine(StorageRootPath, "FFmpeg");

    /// <summary>
    /// Directory for temporary files.
    /// Default: {StorageRootPath}/Temp
    /// </summary>
    public string TemporaryFilesDirectory => Path.Combine(StorageRootPath, "Temp");

    /// <summary>
    /// Timeout for model downloads (default: 1 hour).
    /// </summary>
    public TimeSpan DownloadTimeout { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Buffer size for file operations (default: 8KB).
    /// </summary>
    public int FileBufferSize { get; set; } = 8192;

    private static string GetDefaultStorageRoot()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "Voxcribe");
    }

    /// <summary>
    /// Ensures all required directories exist.
    /// </summary>
    public void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(ModelsDirectory);
        Directory.CreateDirectory(FFmpegDirectory);
        Directory.CreateDirectory(TemporaryFilesDirectory);
    }
}
