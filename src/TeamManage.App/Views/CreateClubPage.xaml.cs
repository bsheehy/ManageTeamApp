using TeamManage.App.ViewModels;

namespace TeamManage.App.Views;

public partial class CreateClubPage : ContentPage
{
    public CreateClubPage(CreateClubViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
