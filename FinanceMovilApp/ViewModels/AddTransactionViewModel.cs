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
using FinanceMovilApp.Helpers;

namespace FinanceMovilApp.ViewModels
{
    //Permite recibir una transaccion para editar / Edit
    [QueryProperty(nameof(TransactionToEdit), "TransactionToEdit")]
    public partial class AddTransactionViewModel : BaseViewModel
    {
        private readonly LocalDbService _dbService;
        private int _transactionId = 0; // 0 = Nueva, >0 = Editar / 0 = New, >0 = Edit

        // Propiedades de enlace para los detalles de la transacción// Binding properties for the transaction details
        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private DateTime date;

        // Al cambiar IsIncome, se actualiza la lista de categorías automáticamente/ When IsIncome changes, the category list is updated automatically
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TransactionTypeLabel))] // Updates the label when toggled// Actualiza la etiqueta al cambiar
        private bool isIncome;

        partial void OnIsIncomeChanged(bool value)
        {
            UpdateCategories();
        }

        //-- Categorias --
        //Lista que vera el usuario en el Picker // List that the user will see in the Picker
        [ObservableProperty]
        private ObservableCollection<string> categories;

        [ObservableProperty]
        private string selectedCategory;

        //--Recurrencia--
        [ObservableProperty]
        private bool isRecurring;

        // Lista de opciones para el picker (Semanal, Mensual...)/ List of options for the picker (Weekly, Monthly...)
        public List<string> Frequencies { get; } = Enum.GetNames(typeof(PaymentFrequency)).ToList();
        [ObservableProperty]
        private string selectedFrequency;

        //Propiedad para recibir la transaccion a editar / Edicion
        public TransactionModel TransactionToEdit
        {
            set 
            {
                if (value != null) 
                {
                    _transactionId = value.Id;
                    Amount = value.Amount;
                    Description = value.Description;
                    Date = value.Date;
                    IsIncome = value.IsIncome;
                    IsRecurring = value.IsRecurring;
                    SelectedCategory = value.CategoryName;

                    if (value.IsRecurring) 
                    { 
                        SelectedFrequency = value.Frequency.ToString();
                    }
                    
                    Title = "Editar Transacción";
                    UpdateCategories();

                }
            }
        }

        // Calculated property for the UI label/ Etiqueta calculada para la UI
        public string TransactionTypeLabel => IsIncome ? "Ingreso" : "Gasto";

        public AddTransactionViewModel(LocalDbService dbService)
        {
            _dbService = dbService; //asignacion inicial del servicio de base de datos/ initial assignment of the database service
            //Inicia el formulario limpio / Start with a clean form
            ClearForm();
            //Inicializar categorias/ Initialize categories
            UpdateCategories();
        }

        //Metodo para limpiar el formulario/ Method to clear the form
        private void ClearForm()
        {
            _transactionId = 0;
            Amount = 0;
            Description = string.Empty;
            Date = DateTime.Now;
            IsIncome = false;
            IsRecurring = false;
            SelectedFrequency = PaymentFrequency.None.ToString();
            Title = "Nueva Transacción";
        }

        private void UpdateCategories()
        {
            //se alimenta de las listas estaticas del helper/ it is fed from the static lists of the helper

            if (IsIncome)
            {
                Categories = new ObservableCollection<string>(CategoryHelper.IncomeCategories);
            }
            else
            {
                Categories = new ObservableCollection<string>(CategoryHelper.ExpenseCategories);
            }

            // Solo resetear si no estamos editando o si la categoría actual no está en la lista nueva/ Only reset if we're not editing or if the current category is not in the new list
            if (string.IsNullOrEmpty(SelectedCategory) || !Categories.Contains(SelectedCategory))
            {
                SelectedCategory = Categories.FirstOrDefault();
            }
        }


        [RelayCommand]
        private async Task Save()
        {
            if (Amount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "El monto debe ser mayor a 0", "OK");
                return;
            }

            if (string.IsNullOrEmpty(SelectedCategory))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Debes seleccionar una categoría", "OK");
                return;
            }

            //Convertir el string seleccionado del Picker al Enum correspondiente/ Convert the selected string from the Picker to the corresponding Enum
            PaymentFrequency freEnum = PaymentFrequency.None;
            if (IsRecurring && !string.IsNullOrEmpty(SelectedFrequency)) 
            {
                Enum.TryParse(SelectedFrequency, out freEnum);
            }

            var newTransaction = new TransactionModel
            {   
                Id=_transactionId, // Mantener el Id para edición/ Keep the Id for editing
                Amount = this.Amount,
                Description = this.Description,
                Date = this.Date,
                IsIncome = this.IsIncome,
                IsRecurring = this.IsRecurring,
                CategoryName = this.SelectedCategory,
                CategoryIcon = "tag",
                Frequency = freEnum
            };

            await _dbService.SaveTransactionAsync(newTransaction);
            // si es edicion , volver atras. si es nueva se sigue agregando./ if it's editing, go back. if it's new, keep adding.
            if (_transactionId != 0) 
            {
                await App.Current.MainPage.DisplayAlert("Actualizado", "El movimiento ha sido corregido.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else 
            {
                await App.Current.MainPage.DisplayAlert("¡Hecho!", "Transacción registrada correctamente.", "OK");
                //Limpiar el formulario después de guardar/ Clear the form after saving
                ClearForm();
            }
                
        }

    }
}
