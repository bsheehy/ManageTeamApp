using TeamManage.App.ViewModels;

namespace TeamManage.App.Views;

public partial class CreateMatchPage : ContentPage
{
    public CreateMatchPage(CreateMatchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
