using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.Application.DTOs.Teams;
using TeamManage.App.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.App.ViewModels;

public partial class TeamsViewModel : BaseViewModel
{
    private const string CacheKey = "teams";

    private readonly ITeamApiService _teamApi;
    private readonly ISessionService _session;
    private readonly ILocalCacheService _cache;

    [ObservableProperty]
    private bool isOffline;

    public ObservableCollection<TeamSummaryDto> Teams { get; } = [];

    public bool IsManager => _session.Role == UserRole.Manager;

    public string WelcomeMessage => $"Welcome, {_session.FullName}";

    public TeamsViewModel(ITeamApiService teamApi, ISessionService session, ILocalCacheService cache)
    {
        _teamApi = teamApi;
        _session = session;
        _cache = cache;
        Title = "My Teams";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await RunBusyAsync(async () =>
        {
            IsOffline = false;

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                if (!await LoadFromCacheAsync())
                {
                    ErrorMessage = "You're offline and no cached teams are available.";
                }

                return;
            }

            try
            {
                var teams = await _teamApi.GetMyTeamsAsync();
                Teams.Clear();
                foreach (var team in teams)
                {
                    Teams.Add(team);
                }

                await _cache.SaveAsync(CacheKey, teams);
            }
            catch (Exception ex) when (ex is ApiException or HttpRequestException or TaskCanceledException)
            {
                if (!await LoadFromCacheAsync())
                {
                    throw;
                }
            }
        });
    }

    private async Task<bool> LoadFromCacheAsync()
    {
        var cached = await _cache.LoadAsync<List<TeamSummaryDto>>(CacheKey);
        if (cached is null)
        {
            return false;
        }

        Teams.Clear();
        foreach (var team in cached)
        {
            Teams.Add(team);
        }

        IsOffline = true;
        return true;
    }

    [RelayCommand]
    private async Task OpenTeamAsync(TeamSummaryDto team) =>
        await Shell.Current.GoToAsync($"teamdetail?teamId={team.Id}");

    [RelayCommand]
    private async Task CreateTeamAsync() =>
        await Shell.Current.GoToAsync("createteam");

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _session.ClearAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
