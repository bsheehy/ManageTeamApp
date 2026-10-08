using TeamManage.App.ViewModels;

namespace TeamManage.App.Views;

public partial class TeamsPage : ContentPage
{
    private readonly TeamsViewModel _viewModel;

    public TeamsPage(TeamsViewModel viewModel)
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
