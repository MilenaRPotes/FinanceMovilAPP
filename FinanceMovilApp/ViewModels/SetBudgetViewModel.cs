using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceMovilApp.Models;
using FinanceMovilApp.Services;
using FinanceMovilApp.Helpers; // Para las categorías
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace FinanceMovilApp.ViewModels
{
    [QueryProperty(nameof(BudgetToEdit), " BudgetToEdit")]
    [QueryProperty(nameof(MonthlyIncome), "MonthlyIncome")]
    public partial class SetBudgetViewModel : BaseViewModel
    {
        private readonly LocalDbService _dbService;
        private int _budgetId = 0; // 0 = Nuevo, >0 = Editar

        //Base para los calculos (Ingreso del mes)
        [ObservableProperty]
        private decimal monthlyIncome;

        [ObservableProperty]
        private ObservableCollection<string> categories;

        [ObservableProperty]
        private string selectedCategory;

        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private double percentage;

        public BudgetModel BudgetToEdit
        {
            set
            {
                if (value != null)
                {
                    _budgetId = value.Id;
                    SelectedCategory = value.CategoryName;
                    Amount = value.PlannedAmount;
                    //El porcentaje se calcula automaticamente al setear Amount si hay MonthlyIncome
                    Title = "Editar Presupuesto";
                }
            }
        }

        public SetBudgetViewModel(LocalDbService dbService)
        {
            _dbService = dbService;
            //Cargar categorias de gastos
            Categories = new ObservableCollection<string>(CategoryHelper.ExpenseCategories);
            SelectedCategory = Categories.FirstOrDefault();
            Title = "Asignar Presupuesto"; //Titulo por defecto
        }

        // Al cambiar el monto, se recalcula el porcentaje
        partial void OnAmountChanged(decimal value)
        {
            if (MonthlyIncome > 0 && !_isUpdating)
            {
                _isUpdating = true;
                Percentage = (double)(value / MonthlyIncome) * 100;
                _isUpdating = false;
            }
        }

        // Si cambio el porcentaje, se recalcula el monto
        partial void OnPercentageChanged(double value)
        {
            if (MonthlyIncome > 0 && !_isUpdating)
            {
                _isUpdating = true;
                Amount = MonthlyIncome * (decimal)(value / 100);
                _isUpdating = false;
            }
        }

        private bool _isUpdating = false; // Para evitar loops al actualizar monto/porcentaje


        [RelayCommand]
        private async Task Save()
        {
            if (Amount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "El monto debe ser mayor a 0", "OK");
                return;
            }

            var budget = new BudgetModel
            {
                Id = _budgetId,
                CategoryName = SelectedCategory,
                PlannedAmount = Amount,
                Month = DateTime.Now.Month, // se asume mes actual por ahora
                Year = DateTime.Now.Year
            };

            await _dbService.SaveBudgetAsync(budget);
            await Shell.Current.GoToAsync("..");
        }

    }
}
