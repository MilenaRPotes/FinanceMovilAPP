namespace FinanceMovilApp.Views;
using FinanceMovilApp.ViewModels;

public partial class SetBudgetPage : ContentPage
{
    public SetBudgetPage(SetBudgetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}