using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceMovilApp.Models;
using FinanceMovilApp.Services;
using System.Threading.Tasks;

namespace FinanceMovilApp.ViewModels
{
    [QueryProperty(nameof(Lesson), "Lesson")]
    public partial class MindsetDetailViewModel : BaseViewModel
    {
        private readonly LocalDbService _dbService;

        [ObservableProperty]
        private MindsetItem lesson;

        public MindsetDetailViewModel(LocalDbService dbService) 
        { 
            _dbService = dbService;
        }

        [RelayCommand]
        private async Task CompleteLesson() 
        {
            if (Lesson == null) return;
            await _dbService.CompleteLessonAsync(Lesson);


            await App.Current.MainPage.DisplayAlert(
                "¡Sabiduría Adquirida!",
                "Has asimilado este conocimiento. El siguiente paso del camino se ha revelado.",
                "Continuar");

            // Volvemos a la lista (que se actualizará mostrando el candado abierto)/ We return to the list (which will update showing the open lock)
            await Shell.Current.GoToAsync("..");
        }

    }
}
