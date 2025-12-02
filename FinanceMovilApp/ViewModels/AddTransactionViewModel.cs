using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceMovilApp.Models;
using FinanceMovilApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceMovilApp.ViewModels
{
    public partial class AddTransactionViewModel : BaseViewModel
    {
        private readonly LocalDbService _dbService;

        // Binding properties for the transaction details
        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private DateTime date;

        // Al cambiar IsIncome, se actualiza la lista de categorías automáticamente
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TransactionTypeLabel))] // Updates the label when toggled
        private bool isIncome;

        partial void OnIsIncomeChanged(bool value)
        {
            UpdateCategories();
        }

        //-- Categorias --
        //Lista que vera el usuario en el Picker 
        [ObservableProperty]
        private ObservableCollection<string> categories;

        [ObservableProperty]
        private string selectedCategory;

        //--Recurrencia--
        [ObservableProperty]
        private bool isRecurring;

        // Lista de opciones para el picker (Semanal, Mensual...)
        public List<string> Frequencies { get; } = Enum.GetNames(typeof(PaymentFrequency)).ToList();
        [ObservableProperty]
        private string selectedFrequency;

        // Calculated property for the UI label
        public string TransactionTypeLabel => IsIncome ? "Ingreso" : "Gasto";

        public AddTransactionViewModel(LocalDbService dbService)
        {
            _dbService = dbService; //asignacion inicial del servicio de base de datos
            //Inicia el formulario limpio 
            ClearForm();
            //Inicializar categorias
            UpdateCategories();
        }

        //Metodo para limpiar el formulario
        private void ClearForm()
        {
            Amount = 0;
            Description = string.Empty;
            Date = DateTime.Now;
            IsIncome = false;
            IsRecurring = false;
            SelectedFrequency = PaymentFrequency.None.ToString();
        }

        private void UpdateCategories()
        {
            //Aqui se pueden agregar categorias segun si es ingreso o gasto
       
            if (IsIncome)
            {
                Categories = new ObservableCollection<string>
                {
                    "Salario",
                    "Negocio/Ventas",
                    "Inversiones",
                    "Regalos",
                    "Otros Ingresos"
                };
            }
            else
            {
                Categories = new ObservableCollection<string>
                {
                    "Vivienda",
                    "Alimentación",
                    "Transporte",
                    "Salud",
                    "Educación",
                    "Ocio",
                    "Pago de Deudas",
                    "Otros Gastos"
                };
            }
            // Seleccionar la primera categoría por defecto
            SelectedCategory = Categories.FirstOrDefault();
        }


        [RelayCommand]
        private async Task Save()
        {
            if (Amount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "El monto debe ser mayor a 0", "OK");
                return;
            }

            //Convertir el string seleccionado del Picker al Enum correspondiente
            PaymentFrequency freEnum = PaymentFrequency.None;
            if (IsRecurring && !string.IsNullOrEmpty(SelectedFrequency)) 
            {
                Enum.TryParse(SelectedFrequency, out freEnum);
            }

            var newTransaction = new TransactionModel
            {
                Amount = this.Amount,
                Description = this.Description,
                Date = this.Date,
                IsIncome = this.IsIncome,
                IsRecurring = this.IsRecurring,
                CategoryName = this.SelectedCategory,
                CategoryIcon = "tag", //Icono por defecto
                Frequency = freEnum
            };

            await _dbService.SaveTransactionAsync(newTransaction);
            await App.Current.MainPage.DisplayAlert("¡Hecho!", "Transacción guardada correctamente.", "OK");
            //Limpiar el formulario después de guardar
            ClearForm();
        }

    }
}
