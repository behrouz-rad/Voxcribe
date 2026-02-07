// © 2026 Behrouz Rad. All rights reserved.

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Voxcribe.Desktop.Services;
using Voxcribe.Desktop.ViewModels;
using Voxcribe.Desktop.Views;
using Voxcribe.Engine;
using Voxcribe.Engine.Contracts;

namespace Voxcribe.Desktop;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Setup Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddVoxcribeEngine();

        // Desktop-specific services
        services.AddSingleton<IAudioRecorder, AudioRecordingService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IDialogService, DialogService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<TranscriptionViewModel>();
        services.AddTransient<ModelManagementViewModel>();
        services.AddTransient<RecordingViewModel>();
    }
}
