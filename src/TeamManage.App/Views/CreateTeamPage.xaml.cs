using TeamManage.App.ViewModels;

namespace TeamManage.App.Views;

public partial class CreateTeamPage : ContentPage
{
    public CreateTeamPage(CreateTeamViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
