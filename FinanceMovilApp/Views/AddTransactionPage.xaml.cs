using FinanceMovilApp.ViewModels;

namespace FinanceMovilApp.Views;

public partial class AddTransactionPage : ContentPage
{
	public AddTransactionPage(AddTransactionViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}