// © 2026 Behrouz Rad. All rights reserved.

using NAudio.Wave;
using Voxcribe.Engine.Contracts;
using Voxcribe.Engine.Domain;

namespace Voxcribe.Desktop.Services;

/// <summary>
/// Cross-platform audio recording using NAudio
/// Records directly to 16kHz mono WAV format for Whisper
/// </summary>
public class AudioRecordingService : IAudioRecorder
{
    private WaveInEvent? _waveIn;
    private WaveFileWriter? _writer;
    private string? _outputPath;
    private AudioRecordingSession? _currentSession;
    private System.Timers.Timer? _progressTimer;

    public bool IsRecording { get; private set; }

    public event EventHandler<TimeSpan>? RecordingProgressChanged;

    public Task<AudioRecordingSession> StartRecordingAsync(
        AudioRecordingConfig? config = null,
        CancellationToken cancellationToken = default)
    {
        if (IsRecording)
        {
            throw new InvalidOperationException("Already recording");
        }

        config ??= new AudioRecordingConfig();

        // Create output file
        _outputPath = Path.Combine(Path.GetTempPath(), $"recording_{Guid.NewGuid()}.wav");

        // Setup audio input using config
        var waveFormat = new WaveFormat(config.SampleRate, config.Channels);
        _waveIn = new WaveInEvent
        {
            WaveFormat = waveFormat,
            BufferMilliseconds = 100
        };

        _writer = new WaveFileWriter(_outputPath, waveFormat);

        _waveIn.DataAvailable += (s, e) =>
        {
            _writer?.Write(e.Buffer, 0, e.BytesRecorded);
        };

        _waveIn.RecordingStopped += (s, e) =>
        {
            _writer?.Dispose();
            _writer = null;
        };

        var startedAt = DateTime.Now;
        _currentSession = new AudioRecordingSession
        {
            SessionId = Guid.NewGuid().ToString(),
            StartedAt = startedAt,
            OutputFilePath = _outputPath
        };

        _waveIn.StartRecording();
        IsRecording = true;

        _progressTimer = new System.Timers.Timer(100);
        _progressTimer.Elapsed += (s, e) =>
        {
            var duration = DateTime.Now - startedAt;
            RecordingProgressChanged?.Invoke(this, duration);
        };
        _progressTimer.Start();

        return Task.FromResult(_currentSession);
    }

    public async Task<string> StopRecordingAsync()
    {
        if (!IsRecording || _waveIn == null || _outputPath == null)
        {
            throw new InvalidOperationException("Not recording");
        }

        _progressTimer?.Stop();
        _progressTimer?.Dispose();
        _progressTimer = null;

        _waveIn.StopRecording();
        _waveIn.Dispose();
        _waveIn = null;

        IsRecording = false;

        if (_currentSession != null)
        {
            _currentSession = _currentSession with { StoppedAt = DateTime.Now };
        }

        // Wait a bit for file to be written
        await Task.Delay(100);


        var result = _outputPath;
        _outputPath = null;
        _currentSession = null;

        return result;
    }

    public void CancelRecording()
    {
        if (!IsRecording)
        {
            return;
        }

        _progressTimer?.Stop();
        _progressTimer?.Dispose();
        _progressTimer = null;

        _waveIn?.StopRecording();
        _waveIn?.Dispose();
        _waveIn = null;

        _writer?.Dispose();
        _writer = null;

        if (_outputPath != null && File.Exists(_outputPath))
        {
            try
            {
                File.Delete(_outputPath);
            }
            catch { }
        }

        _outputPath = null;
        _currentSession = null;
        IsRecording = false;
    }
}
