using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceMovilApp.Models;
using FinanceMovilApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace FinanceMovilApp.ViewModels
{
    public partial class BudgetViewModel : BaseViewModel
    {
        private readonly LocalDbService _dbService;

        [ObservableProperty]
        private ObservableCollection<BudgetModel> budgets;

        [ObservableProperty]
        private decimal totalIncome;

        [ObservableProperty]
        private decimal totalAssigned;

        [ObservableProperty]
        private decimal remainingToAssign;

        [ObservableProperty]
        private double assignedPercentage;

        [ObservableProperty]
        private string currentMonth;

        public BudgetViewModel(LocalDbService dbService)
        {
            _dbService = dbService;
            Budgets = new ObservableCollection<BudgetModel>();
            // Obtenemos el mes
            string nombreMes = DateTime.Now.ToString("MMMM", CultureInfo.CurrentCulture).ToUpper();

            // Asignamos el Título de la página
            Title = $"PRESUPUESTO DE {nombreMes}";

            // Asignamos la propiedad para usarla en el Label de "Ingreso Disponible"
            CurrentMonth = nombreMes;

        }

        [RelayCommand]
        public async Task LoadData()
        {
            if (IsBusy) return;
            IsBusy = true;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // 1. Obtener Ingresos Reales del mes (La base del pastel)
            TotalIncome = await _dbService.GetMonthlyIncomeAsync(currentMonth, currentYear);

            // Si no hay ingresos aún, usamos una base ficticia para no romper los cálculos
            if (TotalIncome == 0) TotalIncome = 1;

            // 2. Obtener Presupuestos
            var budgetList = await _dbService.GetBudgetsForMonthAsync(currentMonth, currentYear);

            Budgets.Clear();
            decimal assignedSum = 0;

            foreach (var item in budgetList)
            {
                Budgets.Add(item);
                assignedSum += item.PlannedAmount;
            }

            // 3. Calcular Resumen
            TotalAssigned = assignedSum;
            RemainingToAssign = TotalIncome - TotalAssigned;
            AssignedPercentage = (double)(TotalAssigned / TotalIncome);

            IsBusy = false;
        }

        [RelayCommand]
        private async Task AddBudget()
        {
            // Pasamos el ingreso mensual para poder calcular porcentajes
            var navParam = new Dictionary<string, object>
            {
                { "MonthlyIncome", TotalIncome }
            };
           
            await Shell.Current.GoToAsync("SetBudgetPage", navParam);
        }

        [RelayCommand]
        private async Task DeleteBudget(BudgetModel budget)
        {
            if (budget == null) return;
            await _dbService.DeleteBudgetAsync(budget);
            await LoadData();
        }

        [RelayCommand]
        private async Task EditBudget(BudgetModel budget)
        {
            var navParam = new Dictionary<string, object>
            {
                { "BudgetToEdit", budget },
                { "MonthlyIncome", TotalIncome }
            };
            await Shell.Current.GoToAsync("SetBudgetPage", navParam);
        }
    }
}
