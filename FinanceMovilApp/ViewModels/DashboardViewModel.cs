using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceMovilApp.Models;
using FinanceMovilApp.Services;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceMovilApp.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly LocalDbService _dbService;

        //Lista de transacciones 5 elementos mas recientes/ List of transactions 5 most recent items
        [ObservableProperty]
        private ObservableCollection<TransactionModel> transactions;

        //El saldo total de la Fortaleza Financiera / The total balance of Financial Strength
        [ObservableProperty]
        private decimal financialStrength;

        //Texto formateado para la moneda / Formatted text for currency
        [ObservableProperty]
        private string financialStrengthText;

        public DashboardViewModel(LocalDbService dbService)
        {
            _dbService = dbService;
            Transactions = new ObservableCollection<TransactionModel>();
        }

        //Metodo para cargar datos se llama cada vez que la pantalla aparece/ Method to load data is called every time the screen appears
        [RelayCommand]
        public async Task LoadData()
        {

            // Evitar cargas concurrentes/ Prevent concurrent loads
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // 1. Calcular la Fortaleza Financiera (Saldo total de todo el historial)/ 1. Calculate Financial Strength (Total balance of the entire history)
                FinancialStrength = await _dbService.GetTotalBalanceAsync();
                FinancialStrengthText = $"{FinancialStrength:C0}"; // Formato de moneda local/ Local currency format

                // 2. Obtener las 5 transacciones más recientes/ 2. Get the 5 most recent transactions
                var recentTransactions = await _dbService.GetRecentTransactionsAsync(5);

                Transactions.Clear();
                foreach (var transaction in recentTransactions)
                {
                    Transactions.Add(transaction);
                }

            }
            finally
            {
                IsBusy = false;
            }
        }

        //Navegar a la pantalla de agregar transaccion/ Navigate to the add transaction screen
        [RelayCommand]
        private async Task GoToAddTransaction()
        {
            await Shell.Current.GoToAsync("AddTransactionPage");
        }

        //-- Eliminar y Editar transaciones --/-- Delete and Edit transactions --

        [RelayCommand]
        private async Task DeleteTransaction(TransactionModel transaction)
        {
            if (transaction == null) return;

            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Eliminar",
                 $"¿Borrar {transaction.Description} de {transaction.Amount:C0}?",
                 "Sí, borrar", "Cancelar");

            if (confirm)
            {
                await _dbService.DeleteTransactionAsync(transaction);
                await LoadData(); // Recargar datos después de eliminar (saldo y lista)/ Reload data after deleting (balance and list)

            }
        }

        [RelayCommand]
        private async Task EditTransaction(TransactionModel transaction)
        {
            if (transaction == null) return;
            // Navegar a la página de edición, pasando la transacción como parámetro/ Navigate to the edit page, passing the transaction as a parameter
            var navParam = new Dictionary<string, object>
            {
                { "TransactionToEdit", transaction }
            };
            await Shell.Current.GoToAsync(nameof(Views.AddTransactionPage), navParam);
        }
    }
}
