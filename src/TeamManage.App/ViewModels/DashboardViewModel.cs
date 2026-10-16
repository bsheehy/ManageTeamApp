using CommunityToolkit.Mvvm.Input;
using TeamManage.App.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.App.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ISessionService _session;

    public bool IsManager => _session.IsClubManager;

    public string WelcomeMessage => $"Welcome, {_session.FullName}";

    public DashboardViewModel(ISessionService session)
    {
        _session = session;
        Title = "Dashboard";
    }

    /// <summary>
    /// Re-evaluates session-derived display properties. Dashboard is a top-level
    /// ShellContent, so its view model instance is cached and reused by Shell across
    /// navigations rather than recreated - this must be called on each appearance to
    /// pick up session changes (e.g. after logout/login as a different user).
    /// </summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(IsManager));
        OnPropertyChanged(nameof(WelcomeMessage));
    }

    [RelayCommand]
    private async Task GoToTeamsAsync() =>
        await Shell.Current.GoToAsync("teams");

    [RelayCommand]
    private async Task GoToClubsAsync() =>
        await Shell.Current.GoToAsync("clubs");

    [RelayCommand]
    private async Task GoToManagersAsync() =>
        await Shell.Current.GoToAsync("managers");

    [RelayCommand]
    private async Task GoToCreateClubAsync() =>
        await Shell.Current.GoToAsync("createclub");

    [RelayCommand]
    private async Task GoToCreateTeamAsync() =>
        await Shell.Current.GoToAsync("createteam");

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _session.ClearAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
