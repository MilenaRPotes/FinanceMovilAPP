using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceMovilApp.Models;
using FinanceMovilApp.Services;
using System;
using System.Threading.Tasks;

namespace FinanceMovilApp.ViewModels
{
    //Recibe la Meta a editar/Edicion
    [QueryProperty(nameof(GoalToEdit), "GoalToEdit")]
    public partial class AddGoalViewModel : BaseViewModel
    {
        private readonly LocalDbService _dbService;
        private int _goalId = 0;// Para identificar si es nueva o edicion/To identify if it's new or edit

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private decimal targetAmount;

        [ObservableProperty]
        private DateTime deadline;

        [ObservableProperty]
        private DateTime minDate;

        // Icono representativo de la meta
        [ObservableProperty]
        private string visualIcon = "chest_gold";

        // Propiedad para recibir la meta a editar/Edicion
        public GoalModel GoalToEdit
        {
            set
            {
                if (value != null)
                {
                    _goalId = value.Id;
                    Name = value.Name;
                    TargetAmount = value.TargetAmount;
                    Deadline = value.Deadline;
                    VisualIcon = value.VisualIcon;
                    Title = "Editar Meta";
                }
            }
        }

        public AddGoalViewModel(LocalDbService dbService)
        {
            _dbService = dbService;

            Deadline = DateTime.Now.AddMonths(1); // Establecer una fecha límite predeterminada/ Set a default deadline
            MinDate = DateTime.Now; // La fecha mínima es hoy/ The minimum date is today
            Title = "Nueva Meta"; 
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name) || TargetAmount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ingresa un nombre y monto válido", "OK");
                return;
            }

            var goal = new GoalModel
            {
                Id = _goalId, // si es 0, es nueva; si no, edita existente/ if it's 0, it's new; otherwise, edit existing
                Name = this.Name,
                TargetAmount = this.TargetAmount,
                Deadline = this.Deadline,
                VisualIcon = this.VisualIcon,
                // Mantenemos el monto actual si estamos editando, si es nueva empieza en 0/ We keep the current amount if we're editing, if it's new it starts at 0
                CurrentAmount = (_goalId != 0) ? await GetCurrentAmount(_goalId) : 0,
                IsCompleted = false
            };

            await _dbService.SaveGoalAsync(goal);

            await App.Current.MainPage.DisplayAlert("¡Meta Creada!", "El primer paso a la riqueza es definir el destino.", "OK");
            // Regresar a la pantalla anterior/ Return to the previous screen
            await Shell.Current.GoToAsync("..");
        }

        //recuperar el monto actual si estamos editando (para no perder el ahorro)/ retrieve the current amount if we're editing (to not lose the savings)
        private async Task<decimal> GetCurrentAmount(int id)
        {
            var existing = await _dbService.GetGoalByIdAsync(id);
            return existing != null ? existing.CurrentAmount : 0;
        }
    }
}
