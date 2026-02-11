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

public class TranscriptionViewModel : ViewModelBase
{
    private readonly ITranscriptionOrchestrator _orchestrator;
    private readonly IFileService _fileService;
    private readonly IDialogService _dialogService;
    private readonly IModelRepository _modelRepository;

    private string? _selectedFilePath;
    private string _transcriptionText = string.Empty;
    private bool _isTranscribing;
    private double _progress;
    private string _statusMessage = "Ready";
    private ModelDescriptor? _selectedModel;
    private LanguageOption _selectedLanguage;
    private CancellationTokenSource? _cancellationTokenSource;

    public TranscriptionViewModel(
        ITranscriptionOrchestrator orchestrator,
        IFileService fileService,
        IDialogService dialogService,
        IModelRepository modelRepository)
    {
        _orchestrator = orchestrator;
        _fileService = fileService;
        _dialogService = dialogService;
        _modelRepository = modelRepository;

        AvailableLanguages = new ObservableCollection<LanguageOption>(LanguageOption.GetAllLanguages());
        _selectedLanguage = AvailableLanguages[0]; // Auto

        // Commands
        var canSelectFile = this.WhenAnyValue(x => x.IsTranscribing, transcribing => !transcribing);
        SelectFileCommand = ReactiveCommand.CreateFromTask(SelectFileAsync, canSelectFile);

        var canTranscribe = this.WhenAnyValue(
            x => x.SelectedFilePath,
            x => x.SelectedModel,
            x => x.IsTranscribing,
            (path, model, transcribing) => !string.IsNullOrEmpty(path) && model?.IsAvailableLocally == true && !transcribing);
        TranscribeCommand = ReactiveCommand.CreateFromTask(TranscribeAsync, canTranscribe);

        var canCancel = this.WhenAnyValue(x => x.IsTranscribing);
        CancelCommand = ReactiveCommand.Create(Cancel, canCancel);

        var canCopy = this.WhenAnyValue(
            x => x.TranscriptionText,
            x => x.IsTranscribing,
            (text, transcribing) => !string.IsNullOrEmpty(text) && !transcribing);
        CopyCommand = ReactiveCommand.CreateFromTask(CopyToClipboardAsync, canCopy);

        var canExport = canCopy;
        ExportCommand = ReactiveCommand.CreateFromTask(ExportAsync, canExport);

        // Available models
        AvailableModels = [];

        // Load models
        _ = LoadModelsAsync();
    }

    public ObservableCollection<ModelDescriptor> AvailableModels { get; }
    public ObservableCollection<LanguageOption> AvailableLanguages { get; }

    public string? SelectedFilePath
    {
        get => _selectedFilePath;
        set => this.RaiseAndSetIfChanged(ref _selectedFilePath, value);
    }

    public string TranscriptionText
    {
        get => _transcriptionText;
        set => this.RaiseAndSetIfChanged(ref _transcriptionText, value);
    }

    public bool IsTranscribing
    {
        get => _isTranscribing;
        set => this.RaiseAndSetIfChanged(ref _isTranscribing, value);
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

    public ReactiveCommand<Unit, Unit> SelectFileCommand { get; }
    public ReactiveCommand<Unit, Unit> TranscribeCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
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

    private async Task SelectFileAsync()
    {
        var filePath = await _fileService.PickMediaFileAsync();
        if (!string.IsNullOrEmpty(filePath))
        {
            SelectedFilePath = filePath;
            StatusMessage = $"Selected: {Path.GetFileName(filePath)}";
        }
    }

    private async Task TranscribeAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath) || SelectedModel == null)
        {
            return;
        }

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
        StatusMessage = "Preparing audio...";

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progressReporter = new Progress<TranscriptionProgress>(p =>
            {
                Progress = p.PercentComplete;
                StatusMessage = p.CurrentPhase;

                // Update with partial text if available
                if (!string.IsNullOrEmpty(p.PartialText))
                {
                    TranscriptionText += p.PartialText + Environment.NewLine;
                }
            });

            var startTime = DateTime.Now;

            var options = new TranscriptionOptions
            {
                Language = SelectedLanguage.WhisperLanguageCode
            };

            var result = await _orchestrator.TranscribeMediaFileAsync(
                SelectedFilePath,
                SelectedModel.Size,
                options,
                progressReporter,
                _cancellationTokenSource.Token);

            var duration = DateTime.Now - startTime;
            TranscriptionText = FormatTranscription(result.FullText);
            StatusMessage = $"Completed in {duration.TotalSeconds.ToString("F1", CultureInfo.InvariantCulture)}s";
            Progress = 100;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Transcription cancelled";
            Progress = 0;
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Transcription Error", ex.Message);
            StatusMessage = "Error occurred";
            Progress = 0;
        }
        finally
        {
            IsTranscribing = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }

    private async Task CopyToClipboardAsync()
    {
        await _fileService.CopyToClipboardAsync(TranscriptionText);
        StatusMessage = "Copied to clipboard";
    }

    private async Task ExportAsync()
    {
        var fileName = SelectedFilePath != null
            ? $"{Path.GetFileNameWithoutExtension(SelectedFilePath)}_transcription.txt"
            : null;

        if (await _fileService.SaveTranscriptionAsync(TranscriptionText, fileName))
        {
            StatusMessage = "Exported successfully";
        }
    }

    /// <summary>
    /// Format transcription with smart paragraphing
    /// </summary>
    private static string FormatTranscription(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        var lines = rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var paragraphs = new List<string>();
        var currentParagraph = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            currentParagraph.Add(trimmed);

            // Create paragraph break on sentence endings with reasonable length
            if ((trimmed.EndsWith('.') || trimmed.EndsWith('!') || trimmed.EndsWith('?'))
                && currentParagraph.Count >= 3)
            {
                paragraphs.Add(string.Join(' ', currentParagraph));
                currentParagraph.Clear();
            }
        }

        // Add remaining
        if (currentParagraph.Count > 0)
        {
            paragraphs.Add(string.Join(' ', currentParagraph));
        }

        return string.Join(Environment.NewLine + Environment.NewLine, paragraphs);
    }
}
