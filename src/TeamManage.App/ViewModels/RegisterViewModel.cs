using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.App.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.App.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthApiService _authApi;
    private readonly ISessionService _session;

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isManager;

    public RegisterViewModel(IAuthApiService authApi, ISessionService session)
    {
        _authApi = authApi;
        _session = session;
        Title = "Create Account";
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        await RunBusyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please fill in all fields.";
                return;
            }

            var role = IsManager ? UserRole.Manager : UserRole.Player;
            var auth = await _authApi.RegisterAsync(FullName.Trim(), Email.Trim(), Password, role);
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
}
