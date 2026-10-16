using TeamManage.App.ViewModels;

namespace TeamManage.App.Views;

public partial class ClubsPage : ContentPage
{
    private readonly ClubsViewModel _viewModel;

    public ClubsPage(ClubsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
