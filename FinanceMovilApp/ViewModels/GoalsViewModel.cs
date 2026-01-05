using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceMovilApp.Models;
using FinanceMovilApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FinanceMovilApp.ViewModels
{
    public partial class GoalsViewModel : BaseViewModel
    {
        private readonly LocalDbService _dbService;

        [ObservableProperty]
        private ObservableCollection<GoalModel> goals;

        //muestra cuanto se lleva ahorrado en total entre todas las metas/ Shows how much has been saved in total among all goals
        [ObservableProperty]
        private decimal totalSaved;

        public GoalsViewModel(LocalDbService dbService)
        {
            _dbService = dbService;
            Goals = new ObservableCollection<GoalModel>();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            if (IsBusy) return;
            IsBusy = true;
           
            var goalist = await _dbService.GetGoalsAsync();
            Goals.Clear();
            decimal sum = 0;
            foreach (var goal in goalist)
            {
                Goals.Add(goal);
                sum += goal.CurrentAmount;
            }
            TotalSaved = sum;

            IsBusy = false;
        }

        //COMANDO: para crear nueva meta / Command to create a new goal
        [RelayCommand]
        private async Task GoToAddGoal() 
        { 
            await Shell.Current.GoToAsync(nameof(Views.AddGoalPage)); // Pagina para crear y registrar/ Page to create and register
        }

        //COMANDO: abonar dinero a una meta seleccionada/ Command: add money to a selected goal
        [RelayCommand]
        private async Task AddFunds(GoalModel goal) 
        {
            // Validacion inicial: Si es nulo o completado, no hacer nada/ Initial validation: If null or completed, do nothing
            if (goal == null || goal.IsCompleted) return;

            //Prompt nativo para pedir cantidad a abonar rapido / Native prompt to quickly ask for amount to add
            string result = await App.Current.MainPage.DisplayPromptAsync(
                "Abonar a Meta", 
                $"¿Cuánto deseas agregar a {goal.Name}?", 
                maxLength: 10, 
                keyboard: Keyboard.Numeric);

            if(decimal.TryParse(result, out decimal amountToAdd) && amountToAdd > 0 ) 
            {
                // Validacion: No permitir que se exceda el monto objetivo/ Validation: Do not allow exceeding the target amount
                decimal remaining = goal.TargetAmount - goal.CurrentAmount;
                if (amountToAdd > remaining) 
                {
                    //Notificar al usuario que el valor es superior al restante / Notify the user that the value is higher than the remaining amount
                    await App.Current.MainPage.DisplayAlert(
                        "Excede la Meta",
                        $"No puedes abonar {amountToAdd:C0} porque solo te faltan {remaining:C0} para completar tu objetivo. Ajusta el monto.", "OK");
                    return;
                }

                // Si pasa la primera Validacion, proceder a abonar/ If it passes the first validation, proceed to add funds
                goal.CurrentAmount += amountToAdd;

                // Guardar en la BD(el servicio de encarga de verifiar si se completo)/ Save to the DB (the service takes care of verifying if it's completed)
                await _dbService.SaveGoalAsync(goal);

                // Recargar la lista para reflejar cambios/ Reload the list to reflect changes
                await LoadData();

                //Feedback al usuario/ User feedback
                if (goal.IsCompleted)
                {
                    await App.Current.MainPage.DisplayAlert("¡Felicidades!", $"Has alcanzado tu meta {goal.Name}!", "OK");
                }
                else 
                {
                    await App.Current.MainPage.DisplayAlert("Abonado", $"Se agregaron {amountToAdd:C0} a tu meta {goal.Name}.", "OK");
                }
            }

        }

        //COMANDO: eliminar meta seleccionada/ Command: delete selected goal
        [RelayCommand]
        private async Task DeleteGoal(GoalModel goal)
        {
            if (goal == null) return;

            string title;
            string message;
            string BtnConfirm;

            // 1. Meta Cumplida (Mensaje Positivo de archivado)/ 1. Completed Goal (Positive archiving message)
            if (goal.IsCompleted) 
            {
                title = "Archivar Meta Cumplida";
                message = $"¡Excelente trabajo! ¿Deseas archivar {goal.Name} en tu historial de éxitos? No se borrará de tus estadísticas.";
                BtnConfirm = "Sí, archivar";
            }
            //2. Meta incompleta (Mensaje de Advertencia de Borrado)/ 2. Incomplete Goal (Deletion Warning Message)
            else
            {
                title = "Eliminar Meta";
                message = $"¿Estás seguro de eliminar {goal.Name}? Al no estar completa, se borrará permanentemente y perderás este progreso.";
                BtnConfirm = "Sí, eliminar";
            
            }
            // Se Muestra la alerta dinamicamente/ The alert is shown dynamically
            bool confirm = await App.Current.MainPage.DisplayAlert(
               title,
               message,
               BtnConfirm,
               "Cancelar");


            if (confirm)
            {
                await _dbService.DeleteGoalAsync(goal);
                await LoadData(); // Recargar lista/ Reload list
            }
        }

        [RelayCommand]
        private async Task EditGoal(GoalModel goal)
        {
            if (goal == null) return;

            // Navegamos al formulario pasando el objeto GoalToEdit/ We navigate to the form passing the GoalToEdit object
            var navParam = new Dictionary<string, object>
            {
                { "GoalToEdit", goal }
            };

            await Shell.Current.GoToAsync(nameof(Views.AddGoalPage), navParam);
        }


    }
}
