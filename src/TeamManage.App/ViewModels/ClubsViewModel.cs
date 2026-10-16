using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.App.Services;
using TeamManage.Application.DTOs.Clubs;

namespace TeamManage.App.ViewModels;

public partial class ClubsViewModel : BaseViewModel
{
    private readonly IClubApiService _clubApi;

    public ObservableCollection<ClubDto> Clubs { get; } = [];

    public ClubsViewModel(IClubApiService clubApi)
    {
        _clubApi = clubApi;
        Title = "My Clubs";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunBusyAsync(async () =>
        {
            var clubs = await _clubApi.GetMyClubsAsync();
            Clubs.Clear();
            foreach (var club in clubs)
            {
                Clubs.Add(club);
            }
        });
    }

    [RelayCommand]
    private async Task NewClubAsync() =>
        await Shell.Current.GoToAsync("createclub");
}
