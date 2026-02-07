# Voxcribe

**A modern, cross-platform desktop application for offline audio/video transcription using AI**

[![CI/CD](https://github.com/behrouz-rad/Voxcribe/actions/workflows/ci.yml/badge.svg)](https://github.com/behrouz-rad/Voxcribe/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Windows](https://img.shields.io/badge/platform-Windows-blue?logo=windows)
![Linux](https://img.shields.io/badge/platform-Linux-black?logo=linux)
![License](https://img.shields.io/badge/license-MIT-green)

## 📖 Overview

Voxcribe is a privacy-focused desktop application that transcribes audio and video files completely offline using OpenAI's Whisper models. Built with modern .NET and Avalonia UI, it provides a native cross-platform experience with no cloud dependencies.

### ✨ Key Features

- **100% Offline** - All processing happens locally on your machine
- **Multi-Source Input** - Transcribe files or record directly from your microphone
- **Universal Format Support** - MP3, MP4, WAV, MKV, AVI, FLAC, OGG, WebM, M4A, and more
- **Multiple AI Models** - Choose from Tiny to Large V3 based on your speed/accuracy needs
- **Multilingual** - Supports 99+ languages via Whisper
- **Smart Formatting** - Intelligent paragraph detection and sentence-aware output
- **Real-time Progress** - Live updates as transcription processes
- **Easy Export** - Copy to clipboard or save as text file
- **Privacy First** - No data collection, no telemetry, no internet required after setup

## 📸 Screenshots

### File Transcription
<img width="1822" height="1417" alt="t_form" src="https://github.com/user-attachments/assets/ee91fd23-904c-42e4-9e70-8b2a46f74ef2" />

### Microphone Recording
<img width="1838" height="1491" alt="r_form" src="https://github.com/user-attachments/assets/c8464eee-62b1-486a-ba2b-eca51a6133ac" />

### Model Management
<img width="1804" height="1674" alt="m_form" src="https://github.com/user-attachments/assets/b5311ffb-df34-443a-b65a-dcf00a1e41cc" />

## 🚀 Quick Start

### 📥 Download

Head to the [Releases](https://github.com/behrouz-rad/Voxcribe/releases) page and download the latest version for your platform (Windows or Linux).

### 🛠️ Building from Source (Developers Only)

If you want to build from source:

```bash
# Prerequisites: .NET 10 SDK
# https://dotnet.microsoft.com/download/dotnet/10.0

# Clone the repository
git clone https://github.com/behrouz-rad/Voxcribe.git
cd Voxcribe

# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run
dotnet run --project src/Voxcribe.Desktop/Voxcribe.Desktop.csproj
```

### 🎯 First Run

1. **Download a Model**: On first launch, go to the "Manage Models" tab and download a model
2. **Select Media**: Navigate to "Transcribe File" and select an audio or video file
3. **Transcribe**: Click "Transcribe" and wait for processing
4. **Export**: Copy the result or save it to a file

## 📚 Usage

### 🎬 File Transcription

1. Go to "Transcribe File" tab
2. Click "Browse" and select your media file
3. Choose an AI model
4. Click "Transcribe"
5. Copy or export the result

### 🎤 Microphone Recording

1. Go to "Record Audio" tab
2. Click "Start Recording"
3. Speak into your microphone
4. Click "Stop Recording"
5. Click "Transcribe Recording"
6. Copy or export the result

### 🤖 Model Selection Guide

| Model | Size | Best For |
|-------|------|----------|
| **Tiny** | ~77 MB | Quick testing |
| **Base** | ~148 MB | English, balanced speed/quality |
| **Small** | ~488 MB | Multilingual, good quality |
| **Medium** | ~1.5 GB | High accuracy multilingual |
| **Large V3** | ~3.1 GB | Maximum accuracy |

## 🔧 Technology Stack

- **.NET 10** - Modern C# 14 features
- **Avalonia UI** - Cross-platform desktop framework
- **ReactiveUI** - Reactive MVVM framework
- **Whisper.net** - Local AI transcription engine
- **NAudio** - Cross-platform audio recording
- **Xabe.FFmpeg** - Media processing and format conversion

## 📁 File Locations

### Models
- **Windows**: `%LOCALAPPDATA%\Voxcribe\Models`
- **Linux**: `~/.local/share/Voxcribe/Models`

### FFmpeg
- **Windows**: `%LOCALAPPDATA%\Voxcribe\FFmpeg`
- **Linux**: `~/.local/share/Voxcribe/FFmpeg`

## 🔍 Troubleshooting

### Model Download Fails
- Check your internet connection
- Ensure sufficient disk space
- Try downloading a smaller model first

### Slow Transcription
- Use a smaller model (Tiny or Base)
- Longer files naturally take more time

### Microphone Not Working
- **Windows**: Settings > Privacy & security > Microphone > Allow access to Voxcribe
- **Linux**: Check ALSA/PulseAudio configuration

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Follow existing code patterns (MVVM, ReactiveUI, Clean Architecture)
4. Write clear commit messages
5. Ensure cross-platform compatibility
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
