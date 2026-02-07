// © 2026 Behrouz Rad. All rights reserved.

using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Voxcribe.Desktop.Services;
using Voxcribe.Engine.Contracts;
using Voxcribe.Engine.Domain;

namespace Voxcribe.Desktop.ViewModels;

public class ModelManagementViewModel : ViewModelBase
{
    private readonly IModelRepository _modelRepository;
    private readonly IDialogService _dialogService;

    private ObservableCollection<ModelItemViewModel> _models = [];
    private bool _isLoading;

    public ModelManagementViewModel(
        IModelRepository modelRepository,
        IDialogService dialogService)
    {
        _modelRepository = modelRepository;
        _dialogService = dialogService;

        RefreshCommand = ReactiveCommand.CreateFromTask(LoadModelsAsync);

        // Load models on initialization
        _ = LoadModelsAsync();
    }

    public ObservableCollection<ModelItemViewModel> Models
    {
        get => _models;
        set => this.RaiseAndSetIfChanged(ref _models, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    private async Task LoadModelsAsync()
    {
        IsLoading = true;

        try
        {
            var modelInfos = await _modelRepository.GetAllModelsAsync();

            Models.Clear();
            foreach (var info in modelInfos)
            {
                Models.Add(new ModelItemViewModel(info, _modelRepository, _dialogService));
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Error Loading Models", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class ModelItemViewModel : ViewModelBase
{
    private readonly IModelRepository _modelRepository;
    private readonly IDialogService _dialogService;
    private ModelDescriptor _modelInfo;
    private bool _isDownloading;
    private double _downloadProgress;
    private CancellationTokenSource? _cancellationTokenSource;

    public ModelItemViewModel(
        ModelDescriptor modelInfo,
        IModelRepository modelRepository,
        IDialogService dialogService)
    {
        _modelInfo = modelInfo;
        _modelRepository = modelRepository;
        _dialogService = dialogService;

        var canDownload = this.WhenAnyValue(
            x => x.IsDownloaded,
            x => x.IsDownloading,
            (downloaded, downloading) => !downloaded && !downloading);
        DownloadCommand = ReactiveCommand.CreateFromTask(DownloadAsync, canDownload);

        var canCancel = this.WhenAnyValue(x => x.IsDownloading);
        CancelDownloadCommand = ReactiveCommand.Create(CancelDownload, canCancel);

        var canDelete = this.WhenAnyValue(
            x => x.IsDownloaded,
            x => x.IsDownloading,
            (downloaded, downloading) => downloaded && !downloading);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, canDelete);
    }

    public ModelDescriptor ModelInfo
    {
        get => _modelInfo;
        set => this.RaiseAndSetIfChanged(ref _modelInfo, value);
    }

    public string DisplayName => ModelInfo.Name;
    public string Description => ModelInfo.Description;
    public string Size => ModelInfo.FormattedSize;

    public bool IsDownloaded
    {
        get => ModelInfo.IsAvailableLocally;
        set
        {
            if (ModelInfo.IsAvailableLocally != value)
            {
                ModelInfo = ModelInfo with { IsAvailableLocally = value };
                this.RaisePropertyChanged();
            }
        }
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        set => this.RaiseAndSetIfChanged(ref _isDownloading, value);
    }

    public double DownloadProgress
    {
        get => _downloadProgress;
        set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
    }

    public ReactiveCommand<Unit, Unit> DownloadCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelDownloadCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    private async Task DownloadAsync()
    {
        IsDownloading = true;
        DownloadProgress = 0;
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new Progress<double>(p =>
            {
                DownloadProgress = p * 100;
            });

            await _modelRepository.AcquireModelAsync(
                ModelInfo.Size,
                progress,
                _cancellationTokenSource.Token);

            IsDownloaded = true;
            await _dialogService.ShowInfoAsync("Download Complete",
                $"{ModelInfo.Name} model downloaded successfully!");
        }
        catch (OperationCanceledException)
        {
            // Clean up partial download
            try { await _modelRepository.RemoveModelAsync(ModelInfo.Size); } catch { }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Download Error", ex.Message);
        }
        finally
        {
            IsDownloading = false;
            DownloadProgress = 0;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void CancelDownload()
    {
        _cancellationTokenSource?.Cancel();
    }

    private async Task DeleteAsync()
    {
        var confirmed = await _dialogService.ShowConfirmAsync(
            "Delete Model",
            $"Are you sure you want to delete the {ModelInfo.Name} model?");

        if (!confirmed) return;

        try
        {
            await _modelRepository.RemoveModelAsync(ModelInfo.Size);
            IsDownloaded = false;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Delete Error", ex.Message);
        }
    }
}
