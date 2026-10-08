using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.Application.DTOs.Teams;
using TeamManage.App.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.App.ViewModels;

[QueryProperty(nameof(TeamId), "teamId")]
public partial class TeamDetailViewModel : BaseViewModel
{
    private readonly ITeamApiService _teamApi;
    private readonly ISessionService _session;

    [ObservableProperty]
    private Guid teamId;

    [ObservableProperty]
    private TeamDto? team;

    [ObservableProperty]
    private string newPlayerFullName = string.Empty;

    [ObservableProperty]
    private string newPlayerEmail = string.Empty;

    [ObservableProperty]
    private string? newPlayerJerseyNumber;

    [ObservableProperty]
    private string? lastGeneratedPassword;

    public ObservableCollection<TeamPlayerDto> Players { get; } = [];

    public bool IsManager => _session.Role == UserRole.Manager;

    partial void OnTeamIdChanged(Guid value) => _ = LoadAsync();

    public TeamDetailViewModel(ITeamApiService teamApi, ISessionService session)
    {
        _teamApi = teamApi;
        _session = session;
        Title = "Team";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (TeamId == Guid.Empty)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            Team = await _teamApi.GetTeamAsync(TeamId);
            Title = Team.Name;

            Players.Clear();
            foreach (var player in Team.Players)
            {
                Players.Add(player);
            }
        });
    }

    [RelayCommand]
    private async Task AddPlayerAsync()
    {
        await RunBusyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(NewPlayerFullName) || string.IsNullOrWhiteSpace(NewPlayerEmail))
            {
                ErrorMessage = "Please enter the player's name and email.";
                return;
            }

            int? jerseyNumber = int.TryParse(NewPlayerJerseyNumber, out var jn) ? jn : null;

            var added = await _teamApi.AddPlayerAsync(TeamId, new()
            {
                FullName = NewPlayerFullName.Trim(),
                Email = NewPlayerEmail.Trim(),
                JerseyNumber = jerseyNumber
            });

            Players.Add(added);
            LastGeneratedPassword = added.GeneratedTemporaryPassword;

            NewPlayerFullName = string.Empty;
            NewPlayerEmail = string.Empty;
            NewPlayerJerseyNumber = null;
        });
    }

    [RelayCommand]
    private async Task RemovePlayerAsync(TeamPlayerDto player)
    {
        await RunBusyAsync(async () =>
        {
            await _teamApi.RemovePlayerAsync(TeamId, player.TeamPlayerId);
            Players.Remove(player);
        });
    }

    [RelayCommand]
    private async Task CreateMatchAsync() =>
        await Shell.Current.GoToAsync($"creatematch?teamId={TeamId}");

    [RelayCommand]
    private async Task ViewMatchesAsync() =>
        await Shell.Current.GoToAsync($"matches?teamId={TeamId}");
}
