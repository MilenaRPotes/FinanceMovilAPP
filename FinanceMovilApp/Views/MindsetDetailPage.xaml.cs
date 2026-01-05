using FinanceMovilApp.ViewModels;

namespace FinanceMovilApp.Views;

public partial class MindsetDetailPage : ContentPage
{
    public MindsetDetailPage(MindsetDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}