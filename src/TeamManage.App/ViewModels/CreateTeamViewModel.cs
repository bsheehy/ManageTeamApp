using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.App.Services;
using TeamManage.Domain.Enums;

namespace TeamManage.App.ViewModels;

public partial class CreateTeamViewModel : BaseViewModel
{
    private readonly ITeamApiService _teamApi;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private SportType sport = SportType.Gaelic_Football;

    [ObservableProperty]
    private int squadSize = 15;

    public List<SportType> SportOptions { get; } = Enum.GetValues<SportType>().ToList();

    private readonly IClubApiService _clubApi;

    [ObservableProperty]
    private TeamManage.Application.DTOs.Clubs.ClubDto? selectedClub;

    public List<TeamManage.Application.DTOs.Clubs.ClubDto> Clubs { get; private set; } = [];

    private readonly IManagerApiService _managerApi;

    [ObservableProperty]
    private TeamManage.Application.DTOs.Managers.TeamManagerSummaryDto? selectedManager;

    public List<TeamManage.Application.DTOs.Managers.TeamManagerSummaryDto> Managers { get; private set; } = [];

    public CreateTeamViewModel(ITeamApiService teamApi, IClubApiService clubApi, IManagerApiService managerApi)
    {
        _teamApi = teamApi;
        _clubApi = clubApi;
        _managerApi = managerApi;
        Title = "New Team";
        _ = LoadClubsAsync();
    }

    private async Task LoadClubsAsync()
    {
        await RunBusyAsync(async () =>
        {
            Clubs = await _clubApi.GetMyClubsAsync();
            OnPropertyChanged(nameof(Clubs));
            SelectedClub = Clubs.FirstOrDefault();
            Managers = await _managerApi.GetManagersAsync();
            OnPropertyChanged(nameof(Managers));
        });
    }

    partial void OnSportChanged(SportType value)
    {
        // Suggest sensible default squad sizes for common sports; manager can still override.
        SquadSize = value switch
        {
            SportType.Gaelic_Football or SportType.Hurling => 15,
            SportType.Soccer => 11,
            SportType.Rugby => 15,
            SportType.Basketball => 5,
            _ => SquadSize
        };
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await RunBusyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Please enter a team name.";
                return;
            }

            if (SquadSize <= 0)
            {
                ErrorMessage = "Squad size must be greater than zero.";
                return;
            }

            if (SelectedClub is null)
            {
                ErrorMessage = "Please create a club first, then select it.";
                return;
            }

            await _teamApi.CreateTeamAsync(new()
            {
                ClubId = SelectedClub.Id,
                ManagerId = SelectedManager?.Id,
                Name = Name.Trim(),
                Sport = Sport,
                SquadSize = SquadSize
            });

            await Shell.Current.GoToAsync("..");
        });
    }
}
