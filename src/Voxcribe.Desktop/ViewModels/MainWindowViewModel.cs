// © 2026 Behrouz Rad. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Voxcribe.Desktop.Services;
using Voxcribe.Engine.Contracts;

namespace Voxcribe.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    private ViewModelBase _currentPage;
    private int _selectedTabIndex;
    private bool _isInitializing;
    private string _initializationStatus = string.Empty;

    public MainWindowViewModel(
        IDialogService dialogService,
        TranscriptionViewModel transcriptionViewModel,
        ModelManagementViewModel modelManagementViewModel,
        RecordingViewModel recordingViewModel,
        AboutViewModel aboutViewModel)
    {
        _dialogService = dialogService;

        TranscriptionPage = transcriptionViewModel;
        RecordingPage = recordingViewModel;
        ModelsPage = modelManagementViewModel;
        AboutPage = aboutViewModel;

        _currentPage = TranscriptionPage;

        // Initialize on startup
        _ = InitializeAsync();
    }

    public TranscriptionViewModel TranscriptionPage { get; }
    public RecordingViewModel RecordingPage { get; }
    public ModelManagementViewModel ModelsPage { get; }
    public AboutViewModel AboutPage { get; }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedTabIndex, value);
    }

    public bool IsInitializing
    {
        get => _isInitializing;
        set => this.RaiseAndSetIfChanged(ref _isInitializing, value);
    }

    public string InitializationStatus
    {
        get => _initializationStatus;
        set => this.RaiseAndSetIfChanged(ref _initializationStatus, value);
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Ensure FFmpeg is downloaded in background
            var mediaProcessor = App.Services?.GetService<IMediaProcessor>();
            if (mediaProcessor != null)
            {
                var progressReporter = new Progress<string>(status =>
                {
                    IsInitializing = true;
                    InitializationStatus = status;
                });

                await Task.Run(async () => await mediaProcessor.InitializeAsync(progressReporter));
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Initialization Error",
                $"Failed to initialize: {ex.Message}");
        }
        finally
        {
            IsInitializing = false;
            InitializationStatus = string.Empty;
        }
    }
}
