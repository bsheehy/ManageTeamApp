using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.App.Services;

namespace TeamManage.App.ViewModels;

[QueryProperty(nameof(TeamId), "teamId")]
public partial class CreateMatchViewModel : BaseViewModel
{
    private readonly IMatchApiService _matchApi;

    [ObservableProperty]
    private Guid teamId;

    [ObservableProperty]
    private string opponent = string.Empty;

    [ObservableProperty]
    private string location = string.Empty;

    [ObservableProperty]
    private DateTime matchDate = DateTime.Today.AddDays(7);

    [ObservableProperty]
    private TimeSpan matchTime = new(15, 0, 0);

    [ObservableProperty]
    private string? notes;

    public CreateMatchViewModel(IMatchApiService matchApi)
    {
        _matchApi = matchApi;
        Title = "New Match";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await RunBusyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(Opponent) || string.IsNullOrWhiteSpace(Location))
            {
                ErrorMessage = "Please enter the opponent and location.";
                return;
            }

            await _matchApi.CreateMatchAsync(new()
            {
                TeamId = TeamId,
                Opponent = Opponent.Trim(),
                Location = Location.Trim(),
                KickOff = MatchDate.Date + MatchTime,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
            });

            await Shell.Current.GoToAsync("..");
        });
    }
}
