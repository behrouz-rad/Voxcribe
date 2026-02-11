// © 2026 Behrouz Rad. All rights reserved.

using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using ReactiveUI;
using Voxcribe.Desktop.Models;
using Voxcribe.Desktop.Services;
using Voxcribe.Engine.Configuration;
using Voxcribe.Engine.Contracts;
using Voxcribe.Engine.Domain;

namespace Voxcribe.Desktop.ViewModels;

public class RecordingViewModel : ViewModelBase
{
    private readonly IAudioRecorder _recordingService;
    private readonly ITranscriptionOrchestrator _orchestrator;
    private readonly IFileService _fileService;
    private readonly IDialogService _dialogService;
    private readonly IModelRepository _modelRepository;

    private bool _isRecording;
    private bool _isTranscribing;
    private TimeSpan _recordingDuration;
    private string _recordedFilePath = string.Empty;
    private string _transcriptionText = string.Empty;
    private double _progress;
    private string _statusMessage = "Ready to record";
    private ModelDescriptor? _selectedModel;
    private LanguageOption _selectedLanguage;
    private CancellationTokenSource? _cancellationTokenSource;

    public RecordingViewModel(
        IAudioRecorder recordingService,
        ITranscriptionOrchestrator orchestrator,
        IFileService fileService,
        IDialogService dialogService,
        IModelRepository modelRepository)
    {
        _recordingService = recordingService;
        _orchestrator = orchestrator;
        _fileService = fileService;
        _dialogService = dialogService;
        _modelRepository = modelRepository;

        AvailableModels = new ObservableCollection<ModelDescriptor>();

        AvailableLanguages = new ObservableCollection<LanguageOption>(LanguageOption.GetAllLanguages());
        _selectedLanguage = AvailableLanguages[0]; // Auto

        // Subscribe to recording progress
        _recordingService.RecordingProgressChanged += (_, duration) =>
        {
            RecordingDuration = duration;
        };

        // Commands
        var canStartRecording = this.WhenAnyValue(
            x => x.IsRecording,
            x => x.IsTranscribing,
            (recording, transcribing) => !recording && !transcribing);
        StartRecordingCommand = ReactiveCommand.CreateFromTask(StartRecordingAsync, canStartRecording);

        var canStopRecording = this.WhenAnyValue(x => x.IsRecording);
        StopRecordingCommand = ReactiveCommand.CreateFromTask(StopRecordingAsync, canStopRecording);

        var canCancelRecording = this.WhenAnyValue(x => x.IsRecording);
        CancelRecordingCommand = ReactiveCommand.Create(CancelRecording, canCancelRecording);

        var canTranscribe = this.WhenAnyValue(
            x => x.RecordedFilePath,
            x => x.SelectedModel,
            x => x.IsTranscribing,
            (path, model, transcribing) => !string.IsNullOrEmpty(path) && model?.IsAvailableLocally == true && !transcribing);
        TranscribeCommand = ReactiveCommand.CreateFromTask(TranscribeAsync, canTranscribe);

        var canCopy = this.WhenAnyValue(
            x => x.TranscriptionText,
            text => !string.IsNullOrEmpty(text));
        CopyCommand = ReactiveCommand.CreateFromTask(CopyToClipboardAsync, canCopy);

        ExportCommand = ReactiveCommand.CreateFromTask(ExportAsync, canCopy);

        _ = LoadModelsAsync();
    }

    public ObservableCollection<ModelDescriptor> AvailableModels { get; }
    public ObservableCollection<LanguageOption> AvailableLanguages { get; }

    public bool IsRecording
    {
        get => _isRecording;
        set => this.RaiseAndSetIfChanged(ref _isRecording, value);
    }

    public bool IsTranscribing
    {
        get => _isTranscribing;
        set => this.RaiseAndSetIfChanged(ref _isTranscribing, value);
    }

    public TimeSpan RecordingDuration
    {
        get => _recordingDuration;
        set => this.RaiseAndSetIfChanged(ref _recordingDuration, value);
    }

    public string RecordedFilePath
    {
        get => _recordedFilePath;
        set => this.RaiseAndSetIfChanged(ref _recordedFilePath, value);
    }

    public string TranscriptionText
    {
        get => _transcriptionText;
        set => this.RaiseAndSetIfChanged(ref _transcriptionText, value);
    }

    public double Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public ModelDescriptor? SelectedModel
    {
        get => _selectedModel;
        set => this.RaiseAndSetIfChanged(ref _selectedModel, value);
    }

    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
    }

    public ReactiveCommand<Unit, Unit> StartRecordingCommand { get; }
    public ReactiveCommand<Unit, Unit> StopRecordingCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelRecordingCommand { get; }
    public ReactiveCommand<Unit, Unit> TranscribeCommand { get; }
    public ReactiveCommand<Unit, Unit> CopyCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCommand { get; }

    private async Task LoadModelsAsync()
    {
        try
        {
            var models = await _modelRepository.GetAllModelsAsync();
            AvailableModels.Clear();
            foreach (var model in models)
            {
                AvailableModels.Add(model);
            }

            // Select first downloaded model or Base if available
            SelectedModel = AvailableModels.FirstOrDefault(m => m.IsAvailableLocally)
                         ?? AvailableModels.FirstOrDefault(m => m.Size == ModelSize.Base);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Error Loading Models", ex.Message);
        }
    }

    private async Task StartRecordingAsync()
    {
        try
        {
            RecordingDuration = TimeSpan.Zero;
            RecordedFilePath = string.Empty;
            TranscriptionText = string.Empty;
            StatusMessage = "Recording...";

            await _recordingService.StartRecordingAsync();
            IsRecording = true;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Recording Error", ex.Message);
            StatusMessage = "Failed to start recording";
        }
    }

    private async Task StopRecordingAsync()
    {
        try
        {
            var filePath = await _recordingService.StopRecordingAsync();
            RecordedFilePath = filePath;
            IsRecording = false;
            StatusMessage = $"Recording saved ({RecordingDuration.ToString(@"mm\:ss", CultureInfo.InvariantCulture)})";

            // Reload models and select first available when recording completes
            await LoadModelsAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Recording Error", ex.Message);
            StatusMessage = "Failed to stop recording";
            IsRecording = false;
        }
    }

    private void CancelRecording()
    {
        _recordingService.CancelRecording();
        IsRecording = false;
        RecordingDuration = TimeSpan.Zero;
        StatusMessage = "Recording cancelled";
    }

    private async Task TranscribeAsync()
    {
        if (string.IsNullOrEmpty(RecordedFilePath) || SelectedModel == null) return;

        // Double-check if model exists
        if (!SelectedModel.IsAvailableLocally)
        {
            await _dialogService.ShowErrorAsync("Model Not Downloaded",
                $"The {SelectedModel.Name} model has not been downloaded. Please download it from the Models tab first.");
            return;
        }

        IsTranscribing = true;
        Progress = 0;
        TranscriptionText = string.Empty;
        StatusMessage = "Transcribing...";

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progressReporter = new Progress<TranscriptionProgress>(p =>
            {
                Progress = p.PercentComplete;
                StatusMessage = p.CurrentPhase;
            });

            var options = new TranscriptionOptions
            {
                Language = SelectedLanguage.WhisperLanguageCode
            };

            var result = await _orchestrator.TranscribeMediaFileAsync(
                RecordedFilePath,
                SelectedModel.Size,
                options,
                progressReporter,
                _cancellationTokenSource.Token);

            TranscriptionText = result.FullText;
            StatusMessage = "Transcription complete";
            Progress = 100;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Transcription Error", ex.Message);
            StatusMessage = "Transcription failed";
        }
        finally
        {
            IsTranscribing = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private async Task CopyToClipboardAsync()
    {
        await _fileService.CopyToClipboardAsync(TranscriptionText);
        StatusMessage = "Copied to clipboard";
    }

    private async Task ExportAsync()
    {
        if (await _fileService.SaveTranscriptionAsync(TranscriptionText, "recording_transcription.txt"))
        {
            StatusMessage = "Exported successfully";
        }
    }
}
