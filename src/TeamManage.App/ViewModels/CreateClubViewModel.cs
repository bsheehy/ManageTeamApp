using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeamManage.App.Services;

namespace TeamManage.App.ViewModels;

public partial class CreateClubViewModel : BaseViewModel
{
    private readonly IClubApiService _clubApi;

    [ObservableProperty]
    private string name = string.Empty;

    public CreateClubViewModel(IClubApiService clubApi)
    {
        _clubApi = clubApi;
        Title = "New Club";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await RunBusyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Please enter a club name.";
                return;
            }

            await _clubApi.CreateClubAsync(new() { Name = Name.Trim() });
            await Shell.Current.GoToAsync("..");
        });
    }
}
