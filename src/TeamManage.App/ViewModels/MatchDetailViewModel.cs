using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.Application.DTOs.Matches;
using TeamManage.Application.DTOs.Teams;
using TeamManage.App.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.App.ViewModels;

/// <summary>
/// Wraps a <see cref="MatchPlayerDto"/> with mutable, bindable allocation state
/// for the manager's lineup editor (position picker, starter/sub toggle).
/// </summary>
public partial class MatchPlayerRowViewModel : ObservableObject
{
    public MatchPlayerDto Source { get; }

    public Guid MatchPlayerId => Source.MatchPlayerId;

    public string PlayerName => Source.PlayerName;

    public AttendanceStatus AttendanceStatus => Source.AttendanceStatus;

    public string AttendanceLabel => Source.AttendanceStatus switch
    {
        AttendanceStatus.Confirmed => "Confirmed",
        AttendanceStatus.Declined => "Declined",
        _ => "Awaiting response"
    };

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isStarter;

    [ObservableProperty]
    private PositionDto? position;

    public MatchPlayerRowViewModel(MatchPlayerDto source, PositionDto? position)
    {
        Source = source;
        isSelected = source.IsSelected;
        isStarter = source.IsStarter;
        this.position = position;
    }
}

[QueryProperty(nameof(MatchId), "matchId")]
public partial class MatchDetailViewModel : BaseViewModel
{
    private readonly IMatchApiService _matchApi;
    private readonly ISessionService _session;

    [ObservableProperty]
    private Guid matchId;

    [ObservableProperty]
    private MatchDto? match;

    [ObservableProperty]
    private AttendanceStatus myAttendanceStatus;

    public ObservableCollection<MatchPlayerRowViewModel> Rows { get; } = [];

    public List<PositionDto> AvailablePositions { get; private set; } = [];

    public bool IsManager => _session.Role == UserRole.Manager;

    public bool IsPlayer => _session.Role == UserRole.Player;

    partial void OnMatchIdChanged(Guid value) => _ = LoadAsync();

    public MatchDetailViewModel(IMatchApiService matchApi, ISessionService session)
    {
        _matchApi = matchApi;
        _session = session;
        Title = "Match";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (MatchId == Guid.Empty)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            Match = await _matchApi.GetMatchAsync(MatchId);
            Title = $"vs {Match.Opponent}";
            AvailablePositions = Match.TeamPositions;

            var mine = Match.Selections.FirstOrDefault(s => s.PlayerId == _session.UserId);
            MyAttendanceStatus = mine?.AttendanceStatus ?? AttendanceStatus.Pending;

            Rows.Clear();
            foreach (var selection in Match.Selections)
            {
                var position = AvailablePositions.FirstOrDefault(p => p.Id == selection.PositionId);
                Rows.Add(new MatchPlayerRowViewModel(selection, position));
            }
        });
    }

    [RelayCommand]
    private async Task ConfirmAsync() => await RespondAsync(AttendanceStatus.Confirmed);

    [RelayCommand]
    private async Task DeclineAsync() => await RespondAsync(AttendanceStatus.Declined);

    private async Task RespondAsync(AttendanceStatus status)
    {
        await RunBusyAsync(async () =>
        {
            await _matchApi.SetAttendanceAsync(MatchId, status);
            MyAttendanceStatus = status;
        });
    }

    [RelayCommand]
    private async Task SaveSelectionsAsync()
    {
        await RunBusyAsync(async () =>
        {
            var allocations = Rows.Select(r => new PlayerAllocationRequest
            {
                MatchPlayerId = r.MatchPlayerId,
                PositionId = r.Position?.Id,
                IsStarter = r.IsStarter,
                IsSelected = r.IsSelected
            }).ToList();

            Match = await _matchApi.AllocateSelectionsAsync(MatchId, allocations);
        });
    }
}
