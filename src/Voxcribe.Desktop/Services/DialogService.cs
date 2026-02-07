// © 2026 Behrouz Rad. All rights reserved.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Voxcribe.Desktop.Services;

public class DialogService : IDialogService
{
    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Error);
        await box.ShowWindowDialogAsync(GetMainWindow()!);
    }

    public async Task ShowInfoAsync(string title, string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Info);
        await box.ShowWindowDialogAsync(GetMainWindow()!);
    }

    public async Task<bool> ShowConfirmAsync(string title, string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNo, Icon.Question);
        var result = await box.ShowWindowDialogAsync(GetMainWindow()!);
        return result == ButtonResult.Yes;
    }
}
