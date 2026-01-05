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

        //Lista de las 7 lecciones / List of the 7 lessons
        [ObservableProperty]
        private ObservableCollection<MindsetItem> lessons;

        // Lista de la Biblioteca sugerida / List of the Suggested Library
        [ObservableProperty]
        private ObservableCollection<BookRecommendation> books;

        //propiedades para la (frase del dia)/ properties for the (quote of the day)
        [ObservableProperty]
        private string randomMessage;

        [ObservableProperty]
        private string randomAuthor;

        // Banco de frases motivacionales internas/ Internal bank of motivational phrases
        private List<string[]> _quotesLibrary = new List<string[]>
        {
            // --- BLOQUE FINANCIERO (El Hombre Más Rico de Babilonia)/FINANCIAL BLOCK (The Richest Man in Babylon)---

            new[] { "La riqueza, como un árbol, crece a partir de una pequeña semilla.", "Arkad" },
            new[] { "El oro se reserva para aquellos que conocen sus leyes.", "Babilonia" },
            new[] { "La oportunidad es una diosa altiva que no pierde tiempo con los que no están preparados.", "George S. Clason" },
            new[] { "Nuestros actos no pueden ser más sabios que nuestros pensamientos.", "El Hombre Más Rico" },
            new[] { "Es mejor un poco de precaución que un gran remordimiento.", "Dabasir" },
            new[] { "La suerte favorece a la mente preparada.", "Proverbio Antiguo" },
            new[] { "Donde está la determinación, el camino puede ser encontrado.", "Arkad" },
            new[] { "El dinero es abundante para aquellos que entienden las reglas simples de su adquisición.", "George S. Clason" },

            // --- BLOQUE MENTALIDAD Y BIENESTAR (Inspirado en Marian Rojas Estapé)/MINDSET AND WELLBEING BLOCK (Inspired by Marian Rojas Estapé) ---
            new[] { "La felicidad no es lo que te pasa, sino cómo interpretas lo que te pasa.", "Marian Rojas Estapé" },
            new[] { "No te inquietes por las dificultades de la vida, sino por cómo reaccionas ante ellas.", "Epicteto" },
            new[] { "Si cambias tu atención, cambias tu realidad.", "Neurociencia" },
            new[] { "El 90% de las cosas que nos preocupan jamás suceden.", "Marian Rojas Estapé" },
            new[] { "Para que te pasen cosas buenas, tienes que activar tu sistema reticular: enfócate.", "Marian Rojas Estapé" },
            new[] { "Aprender a gestionar tus emociones es el activo financiero más rentable.", "Inteligencia Emocional" },

            // --- BLOQUE HÁBITOS Y DISCIPLINA (Atomic Habits / Crecimiento)/HABITS AND DISCIPLINE BLOCK (Atomic Habits / Growth) ---
            new[] { "Somos lo que hacemos repetidamente. La excelencia, entonces, no es un acto, sino un hábito.", "Aristóteles" },
            new[] { "La disciplina es elegir entre lo que quieres ahora y lo que quieres más.", "Abraham Lincoln" },
            new[] { "Los hábitos son el interés compuesto de la superación personal.", "James Clear" },
            new[] { "Un viaje de mil millas comienza con un solo paso... y una sola moneda.", "Lao-Tse" },
            new[] { "La constancia vence lo que la dicha no alcanza.", "Dicho Popular" },
            new[] { "El éxito es la suma de pequeños esfuerzos repetidos día tras día.", "Robert Collier" },
            new[] { "No necesitas ser brillante, solo necesitas ser constante.", "Mentalidad " },

            // --- BLOQUE ESTOICISMO (Disciplina y Control Mental)/ STOICISM BLOCK (Discipline and Mental Control)---
            new[] { "No es pobre el que tiene poco, sino el que ansía más.", "Séneca" },
            new[] { "La riqueza no consiste en tener grandes posesiones, sino en tener pocos deseos.", "Epicteto" },
            new[] { "Sufrimos más a menudo en la imaginación que en la realidad.", "Séneca" },
            new[] { "Tienes poder sobre tu mente, no sobre los acontecimientos externos. Date cuenta de esto y encontrarás la fuerza.", "Marco Aurelio" },
            new[] { "Haz cada cosa en la vida como si fuera lo último que fueras a hacer.", "Marco Aurelio" },
            new[] { "Ninguna cantidad de dinero te hará feliz si no eres feliz contigo mismo.", "Filosofía Estoica" },
            new[] { "Lo que te inquieta no son las cosas (deudas, precios), sino tus opiniones sobre ellas.", "Epicteto" },
            new[] { "Si quieres ser rico, no te afanes en aumentar tus bienes, sino en disminuir tu codicia.", "Epicuro" }, // (Técnicamente epicúreo, pero muy afín)
            new[] { "El hombre que gasta más de lo que tiene, siembra vientos de miseria.", "Sabiduría Antigua" },
            new[] { "Domina tus apetitos antes de que ellos te dominen a ti.", "El Pequeño Libro del Estoicismo" }
        };

        public MindsetViewModel(LocalDbService dbService)
        {
            _dbService = dbService;

            // Inicializamos las listas vacías para evitar errores de "NullReference"/ We initialize empty lists to avoid "NullReference" errors
            Lessons = new ObservableCollection<MindsetItem>();
            Books = new ObservableCollection<BookRecommendation>();
            Title = "Templo del Saber"; // Título de la pantalla
        }

        [RelayCommand]
        public async Task LoadData()
        {
            if (IsBusy) return;
            IsBusy = true;

            try 
            {
                //1. Cargar las lecciones (7)  desde la base de datos local/ 1. Load the lessons (7) from the local database
                var lessonList = await _dbService.GetMindsetItemsAsync();
                lessons.Clear();
                foreach (var item in lessonList)
                {
                    Lessons.Add(item);
                }

                // 2. Cargar Libros Sugeridos de la BD/ 2. Load Suggested Books from the DB
                var bookList = await _dbService.GetBooksAsync();
                Books.Clear();
                foreach (var item in bookList)
                {
                    Books.Add(item);
                }

                // 3. Generar Mensaje Aleatorio / 3. Generate Random Message
                var random = new Random();
                var index = random.Next(_quotesLibrary.Count);
                RandomMessage = _quotesLibrary[index][0];
                RandomAuthor = $"- {_quotesLibrary[index][1]}";


            }
            finally 
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OpenLesson(MindsetItem lesson) 
        {
            if (lesson == null) return;

            //-- LOGICA DE JUEGO: Candado validacion/-- GAME LOGIC: Lock validation --

            // Si la lección está bloqueada (IsLocked = true), no dejamos pasar al usuario./ If the lesson is locked (IsLocked = true), we do not let the user pass.
            if (lesson.IsLocked)
            {
                // Feedback visual explicando por qué no puede entrar/ Visual feedback explaining why they cannot enter
                await App.Current.MainPage.DisplayAlert(
                    "Lección Bloqueada 🔒",
                    "Debes dominar la lección anterior para desbloquear este conocimiento.",
                    "Entendido");
                return; // Cancelamos la navegación/ We cancel the navigation
            }

            // Si está desbloqueada, navegamos al detalle pasando la lección seleccionada/ If it is unlocked, we navigate to the detail passing the selected lesson
            var navParam = new Dictionary<string, object>
            {
                { "Lesson", lesson }
            };

            await Shell.Current.GoToAsync("MindsetDetailPage", navParam);

        }

    }
}


