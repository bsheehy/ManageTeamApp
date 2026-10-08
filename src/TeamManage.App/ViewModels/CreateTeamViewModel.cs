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

    public CreateTeamViewModel(ITeamApiService teamApi)
    {
        _teamApi = teamApi;
        Title = "New Team";
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

            await _teamApi.CreateTeamAsync(new()
            {
                Name = Name.Trim(),
                Sport = Sport,
                SquadSize = SquadSize
            });

            await Shell.Current.GoToAsync("..");
        });
    }
}
