using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.App.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.App.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthApiService _authApi;
    private readonly ISessionService _session;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    public LoginViewModel(IAuthApiService authApi, ISessionService session)
    {
        _authApi = authApi;
        _session = session;
        Title = "Sign In";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        await RunBusyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter your email and password.";
                return;
            }

            var auth = await _authApi.LoginAsync(Email.Trim(), Password);
            await _session.SaveAsync(
                auth.Token,
                auth.UserId,
                auth.FullName,
                auth.Role,
                auth.ExpiresAtUtc,
                auth.RefreshToken,
                auth.RefreshTokenExpiresAtUtc);

            var destination = auth.Role == UserRole.Manager ? "//teams" : "//matches";
            await Shell.Current.GoToAsync(destination);
        });
    }

    [RelayCommand]
    private async Task GoToRegisterAsync() => await Shell.Current.GoToAsync("register");
}
