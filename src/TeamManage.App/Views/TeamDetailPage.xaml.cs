using TeamManage.App.ViewModels;

namespace TeamManage.App.Views;

public partial class TeamDetailPage : ContentPage
{
    public TeamDetailPage(TeamDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
