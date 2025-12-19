using FinanceMovilApp.ViewModels;

namespace FinanceMovilApp.Views;

public partial class BudgetPage : ContentPage
{
    public BudgetPage(BudgetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}