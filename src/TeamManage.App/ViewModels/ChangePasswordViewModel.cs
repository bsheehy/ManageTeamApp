using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.App.Services;

namespace TeamManage.App.ViewModels;

public partial class ChangePasswordViewModel : BaseViewModel
{
    private readonly IAuthApiService _authApi;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    public ChangePasswordViewModel(IAuthApiService authApi)
    {
        _authApi = authApi;
        Title = "Set Your Password";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await RunBusyAsync(async () =>
        {
            if (NewPassword.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters long.";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            await _authApi.ChangePasswordAsync(NewPassword);
            await Shell.Current.GoToAsync("..");
        });
    }
}
