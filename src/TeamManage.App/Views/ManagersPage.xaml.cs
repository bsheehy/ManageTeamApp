using TeamManage.App.ViewModels;

namespace TeamManage.App.Views;

public partial class ManagersPage : ContentPage
{
    private readonly ManagersViewModel _viewModel;

    public ManagersPage(ManagersViewModel viewModel)
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
