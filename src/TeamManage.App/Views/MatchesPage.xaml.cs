using TeamManage.App.ViewModels;

namespace TeamManage.App.Views;

public partial class MatchesPage : ContentPage
{
    private readonly MatchesViewModel _viewModel;

    public MatchesPage(MatchesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
