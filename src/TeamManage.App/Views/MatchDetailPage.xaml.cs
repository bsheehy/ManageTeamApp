using TeamManage.App.ViewModels;

namespace TeamManage.App.Views;

public partial class MatchDetailPage : ContentPage
{
    public MatchDetailPage(MatchDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
