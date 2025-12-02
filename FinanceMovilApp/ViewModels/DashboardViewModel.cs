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

        //Lista de transacciones 5 elementos mas recientes
        [ObservableProperty]
        private ObservableCollection<TransactionModel> transactions;

        //El saldo total de la Fortaleza Financiera 
        [ObservableProperty]
        private decimal financialStrength;

        //Texto formateado para la moneda 
        [ObservableProperty]
        private string financialStrengthText;

        public DashboardViewModel(LocalDbService dbService)
        {
            _dbService = dbService;
            Transactions = new ObservableCollection<TransactionModel>();
        }

        //Metodo para cargar datos se llama cada vez que la pantalla aparece
        [RelayCommand]
        public async Task LoadData() 
        {

            // Evitar cargas concurrentes
            if (IsBusy) return;
            IsBusy = true;

            try 
            {
                // 1. Calcular la Fortaleza Financiera (Saldo total de todo el historial)
                FinancialStrength = await _dbService.GetTotalBalanceAsync();
                FinancialStrengthText = $"{FinancialStrength:C0}"; // Formato de moneda local
                
                // 2. Obtener las 5 transacciones más recientes
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

        //Navegar a la pantalla de agregar transaccion
        [RelayCommand]
        private async Task GoToAddTransaction()
        {
            await Shell.Current.GoToAsync("AddTransactionPage");
        }


    }
}
