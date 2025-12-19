using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceMovilApp.Models;
using FinanceMovilApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FinanceMovilApp.ViewModels
{
    public partial class MindsetViewModel : BaseViewModel
    {
        private readonly LocalDbService _dbService;

        //Lista de las 7 lecciones 
        [ObservableProperty]
        private ObservableCollection<MindsetItem> lessons;

        // Lista de la Biblioteca sugerida
        //[ObservableProperty]
        //private ObservableCollection<BookRecommendation> books;

        //propiedades para la (frase del dia)
        [ObservableProperty]
        private

    }
}
