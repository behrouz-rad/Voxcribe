// © 2026 Behrouz Rad. All rights reserved.

using System.Diagnostics;
using System.Globalization;
using System.Reactive;
using System.Reflection;
using ReactiveUI;

namespace Voxcribe.Desktop.ViewModels;

public class AboutViewModel : ViewModelBase
{
    public AboutViewModel()
    {
        OpenGitHubCommand = ReactiveCommand.Create(OpenGitHub);
    }

    public static string AppName => "Voxcribe";

    public static string Description => "Offline, cross-platform speech-to-text desktop application. " +
        "Transcribes audio, video, and live microphone input locally using OpenAI's Whisper model. " +
        "No cloud, no tracking, full privacy.";

    public static string Version
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version != null
                ? string.Format(CultureInfo.InvariantCulture, "Version {0}.{1}.{2}", version.Major, version.Minor, version.Build)
                : "Version 1.0.0";
        }
    }

    public static string Author => "Behrouz Rad";

    public static string Copyright => $"© {DateTime.Now.Year.ToString(CultureInfo.InvariantCulture)} Behrouz Rad. All rights reserved.";

    public static string GitHubUrl => "https://github.com/behrouz-rad/Voxcribe";

    public ReactiveCommand<Unit, Unit> OpenGitHubCommand { get; }

    private static void OpenGitHub()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = GitHubUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if browser cannot be opened
        }
    }
}
