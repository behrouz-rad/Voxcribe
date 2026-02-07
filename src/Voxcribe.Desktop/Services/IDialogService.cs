// © 2026 Behrouz Rad. All rights reserved.

namespace Voxcribe.Desktop.Services;

public interface IDialogService
{
    public Task ShowErrorAsync(string title, string message);
    public Task ShowInfoAsync(string title, string message);
    public Task<bool> ShowConfirmAsync(string title, string message);
}
