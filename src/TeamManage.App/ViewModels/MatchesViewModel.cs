using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.Application.DTOs.Matches;
using TeamManage.App.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.App.ViewModels;

[QueryProperty(nameof(TeamId), "teamId")]
public partial class MatchesViewModel : BaseViewModel
{
    private const string CacheKey = "matches";

    private readonly IMatchApiService _matchApi;
    private readonly ISessionService _session;
    private readonly ILocalCacheService _cache;

    [ObservableProperty]
    private Guid? teamId;

    [ObservableProperty]
    private bool isOffline;

    public ObservableCollection<MatchSummaryDto> Matches { get; } = [];

    public bool IsManager => _session.Role == UserRole.Manager;

    public string WelcomeMessage => $"Welcome, {_session.FullName}";

    partial void OnTeamIdChanged(Guid? value) => _ = LoadAsync();

    public MatchesViewModel(IMatchApiService matchApi, ISessionService session, ILocalCacheService cache)
    {
        _matchApi = matchApi;
        _session = session;
        _cache = cache;
        Title = "Matches";
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
                    ErrorMessage = "You're offline and no cached matches are available.";
                }

                return;
            }

            try
            {
                var matches = await _matchApi.GetMyMatchesAsync(TeamId);
                var ordered = matches.OrderBy(m => m.KickOff).ToList();
                Matches.Clear();
                foreach (var match in ordered)
                {
                    Matches.Add(match);
                }

                await _cache.SaveAsync(CacheKey, ordered);
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
        var cached = await _cache.LoadAsync<List<MatchSummaryDto>>(CacheKey);
        if (cached is null)
        {
            return false;
        }

        Matches.Clear();
        foreach (var match in cached)
        {
            Matches.Add(match);
        }

        IsOffline = true;
        return true;
    }

    [RelayCommand]
    private async Task OpenMatchAsync(MatchSummaryDto match) =>
        await Shell.Current.GoToAsync($"matchdetail?matchId={match.Id}");

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _session.ClearAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
