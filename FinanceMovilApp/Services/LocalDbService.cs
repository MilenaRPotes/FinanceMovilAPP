using System;
using SQLite;
using FinanceMovilApp.Models;
using FinanceMovilApp.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FinanceMovilApp.Services
{
    public class LocalDbService
    {
        private SQLiteAsyncConnection _database;

        public LocalDbService()
        {
        }

        async public Task Init()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

            //Crea la tabla usando el nuevo nombre del modelo / Create table using the new Model name 
            await _database.CreateTableAsync<TransactionModel>();

            //Crea la tabla de metas / Create the goals table
            await _database.CreateTableAsync<GoalModel>();

            //crea la tabla de presupuestos / create the budget table
            await _database.CreateTableAsync<BudgetModel>();

            //crea la tabla de mentalidad / create the mindset table
            await _database.CreateTableAsync<MindsetItem>();
            await _database.CreateTableAsync<BookRecommendation>();

            //Cargar datos iniciales para mentalidad // Load initial data for mindset
            await SeedMindsetDataAsync();
            await SeedBookDataAsync();
        }

        // --- TRANSACTION METHODS ---
        //METODOS CRUD
        public async Task<List<TransactionModel>> GetTransactionsAsync()
        {
            await Init();
            //Order by Date descending // Ordenar por fecha descendente
            return await _database.Table<TransactionModel>().OrderByDescending(t => t.Date).ToListAsync();
        }

        public async Task<TransactionModel> GetTransactionByIdAsync(int id)
        {
            await Init();
            return await _database.Table<TransactionModel>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }
        public async Task<int> SaveTransactionAsync(TransactionModel item)
        {
            await Init();
            if (item.Id != 0)
                return await _database.UpdateAsync(item);
            else
                return await _database.InsertAsync(item);
        }

        public async Task<int> DeleteTransactionAsync(TransactionModel item)
        {
            await Init();
            return await _database.DeleteAsync(item);
        }

        //--- METODOS ADICIONALES PARA DASHBOARD ---
        // 1. Obtener solo los últimos 'count' movimientos (ej. 5) / Get only the last 'count' transactions (e.g., 5)
        public async Task<List<TransactionModel>> GetRecentTransactionsAsync(int count)
        {
            await Init();
            return await _database.Table<TransactionModel>()
                            .OrderByDescending(t => t.Date) // Ordenar por fecha (más nuevo primero) / Order by date (newest first)
                            .Take(count)                    // Tomar solo la cantidad solicitada / Take only the requested amount
                            .ToListAsync();
        }

        // 2. Calcular el balance total directamente (sin cargar todo a la memoria visual) / Calculate the total balance directly (without loading everything into visual memory)
        public async Task<decimal> GetTotalBalanceAsync()
        {
            await Init();
            // Traemos todo solo para sumar / We bring everything just to sum
            var allTransactions = await _database.Table<TransactionModel>().ToListAsync();

            var totalIncome = allTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            var totalExpenses = allTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);

            return totalIncome - totalExpenses;
        }

        //--- METODOS PARA METAS / GOAL METHODS  ---
        public async Task<List<GoalModel>> GetGoalsAsync()
        {
            await Init();
            // Ordenar por estado de completitud y luego por fecha limite / Sort by completion status and then by deadline
            //Solo se muestran las activas(no archivadas) / Only active are shown (not archived)
            return await _database.Table<GoalModel>()
                .Where(g => !g.IsArchived)
                .OrderBy(g => g.IsCompleted)
                .ThenBy(g => g.Deadline)
                .ToListAsync();
        }

        public async Task<List<GoalModel>> GetAllHistoryGoalsAsync()
        {
            await Init();
            return await _database.Table<GoalModel>().ToListAsync();

        }

        public async Task<GoalModel> GetGoalByIdAsync(int id)
        {
            await Init();
            return await _database.Table<GoalModel>().Where(g => g.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveGoalAsync(GoalModel item)
        {
            await Init();
            //marca la meta como completada con fecha historica / mark the goal as completed with historical date
            if (item.CurrentAmount >= item.TargetAmount)
            { 
                // Si recién se está completando (antes no lo estaba)/ If it is just being completed (it wasn't before)
                if (!item.IsCompleted)
                {
                    item.IsCompleted = true;
                    item.CompletionDate = DateTime.Now; // ¡Guardamos la fecha del logro!/ We save the date of achievement!
                }

                item.CurrentAmount = item.TargetAmount; //Tope visual/ Visual cap
            }
            else
            {
                item.IsCompleted = false;
                item.CompletionDate = null; // Si sacó dinero y dejó de estar completa, borramos la fecha/ If he took out money and stopped being complete, we delete the date
            }

            if (item.Id != 0)
                return await _database.UpdateAsync(item);
            else
                return await _database.InsertAsync(item);
        }

        //-- LOGICA DE ELIMINACION/DELETION LOGIC --
        public async Task<int> DeleteGoalAsync(GoalModel item)
        {
            await Init();
            //1. Meta Completa -> se archiva (Soft Delete) se mantiene el registro Histórico/ Goal Completed -> it is archived (Soft Delete) the Historical record is maintained   
            if (item.IsCompleted)
            {
                item.IsArchived = true;
                return await _database.UpdateAsync(item);
            }
            //2. Meta Incompleta -> se elimina fisicamente(Hard Delete)/ Incomplete Goal -> it is physically deleted (Hard Delete)
            // Eliminación sin terminar meta  no  cuenta en las estadisticas/ Deleting unfinished goal does not count in statistics
            else
            {
                return await _database.DeleteAsync(item);
            }
        }

        //--- METODOS PARA PRESUPUESTO ---
        //Calcular Ingresos Totales de un mes específico (Para base del presupuesto)/ Calculate Total Income for a specific month (For budget base)
        public async Task<decimal> GetMonthlyIncomeAsync(int mont, int year)
        {
            await Init();

            var startOfMonth = new DateTime(year, mont, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var transactions = await _database.Table<TransactionModel>()
                                              .Where(t => t.IsIncome && t.Date >= startOfMonth && t.Date <= endOfMonth)
                                             .ToListAsync();

            return transactions.Sum(t => t.Amount);

        }

        public async Task<List<BudgetModel>> GetBudgetsForMonthAsync(int month, int year)
        {
            await Init();
            var budgets = await _database.Table<BudgetModel>()
                                        .Where(b => b.Month == month && b.Year == year)
                                        .ToListAsync();
            //calcular el gasto real cruzando con transacciones/ calculate actual spending by cross-referencing with transactions
            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var transactions = await _database.Table<TransactionModel>()
                                              .Where(t => !t.IsIncome && t.Date >= startOfMonth && t.Date <= endOfMonth)
                                              .ToListAsync();

            foreach (var budget in budgets)
            {
                var spent = transactions
                                .Where(t => t.CategoryName == budget.CategoryName)
                                .Sum(t => t.Amount);
                budget.ActualSpent = spent;
            }

            return budgets;
        }

        public async Task<int> SaveBudgetAsync(BudgetModel item)
        {
            await Init();

            //Evitar duplicados para misma categoria/mes/año/ Avoid duplicates for same category/month/year
            var existing = await _database.Table<BudgetModel>()
                          .Where(b => b.CategoryName == item.CategoryName && b.Month == item.Month && b.Year == item.Year)
                          .FirstOrDefaultAsync();

            if (existing != null) 
            { 
                item.Id = existing.Id;
                return await _database.UpdateAsync(item);
            }

            return await _database.InsertAsync(item);
        }

        public async Task<int> DeleteBudgetAsync(BudgetModel item)
        {
            await Init();
            return await _database.DeleteAsync(item);
        }

        //--- METODOS MENTALIDAD/ MINDSET METHODS  ---
        private async Task SeedMindsetDataAsync() 
        {
            var count = await _database.Table<MindsetItem>().CountAsync();
            if (count == 0)
            {
               var lessons = new List<MindsetItem>
               {
                   //LECCION 1: ABIERTA (Islocked = false)// LESSON 1: OPEN (Islocked = false)
                    new MindsetItem
                    {
                        Title = "1. Empieza a llenar tu bolsa",
                        Subtitle = "De cada diez monedas que ganes, gasta solo nueve.",
                        Content = "Esta es la regla de oro fundamental. Págate a ti mismo primero. \n\nEsa décima moneda que guardas es la semilla de la que brotará tu árbol de riqueza. No importa cuán poco ganes, una parte debe ser siempre tuya para conservar.",
                        Icon = "money.png",
                        IsRead = false,
                        IsLocked = false 
                    },

                    // LECCIONES 2-7: BLOQUEADAS (IsLocked = true) // LESSONS 2-7: LOCKED (IsLocked = true)
                   // LECCIÓN 2 // LESSON 2
                    new MindsetItem
                    {
                        Title = "2. Controla tus gastos",
                        Subtitle = "Diferencia entre tus necesidades y tus deseos.",
                        Content = "No confundas gastos necesarios con deseos. El presupuesto no es para limitarte, sino para liberarte de la ansiedad.\n\nEs la lámpara que ilumina los agujeros de tu bolsa y te permite gastar en lo que realmente valoras.",
                        Icon = "shield.png",
                        IsRead = false,
                        IsLocked = true
                    },

                  // LECCIÓN 3 // LESSON 3
                    new MindsetItem
                    {
                        Title = "3. Haz que tu oro se multiplique",
                        Subtitle = "Pon a trabajar cada moneda para que se reproduzca.",
                        Content = "La riqueza no está en las monedas que llevas en la bolsa, sino en el flujo constante que llega a ella.\n\nHaz que cada moneda trabaje para ti como un obrero fiel, generando hijos que también trabajen para ti.",
                        Icon = "plant.png",
                        IsRead = false,
                        IsLocked = true
                    },

                    // LECCIÓN 4 // LESSON 4
                    new MindsetItem
                    {
                        Title = "4. Protege tus tesoros",
                        Subtitle = "Invierte solo donde tu capital esté seguro.",
                        Content = "El primer principio de la inversión es la seguridad del capital. No te dejes cegar por ganancias fantásticas pero irreales.\n\nConsulta a los sabios: no le preguntes al panadero sobre las estrellas, pregúntale al astrónomo.",
                        Icon = "lock.png", 
                        IsRead = false,
                        IsLocked = true
                    },

                    // LECCIÓN 5 // LESSON 5
                    new MindsetItem
                    {
                        Title = "5. Haz de tu morada una inversión",
                        Subtitle = "Poseer tu propio techo da confianza y reduce gastos.",
                        Content = "Nadie puede disfrutar plenamente de la vida si no tiene un terreno donde sus hijos jueguen seguros.\n\nTener tu propio refugio reduce el costo de vida y libera tu corazón para emprender mayores proyectos.",
                        Icon = "house.png",
                        IsRead = false,
                        IsLocked = true
                    },

                    // LECCIÓN 6 // LESSON 6
                    new MindsetItem
                    {
                        Title = "6. Asegura un ingreso futuro",
                        Subtitle = "Prevé para los días en que ya no puedas trabajar.",
                        Content = "La vida tiene estaciones. Es tu responsabilidad prever ingresos para su vejez y para la protección de tu familia si faltas.\n\nUn pequeño flujo seguro hoy es mejor que una gran fortuna incierta mañana.",
                        Icon = "umbrella.png", 
                        IsRead = false,
                        IsLocked = true
                    },

                    // LECCIÓN 7 // LESSON 7
                    new MindsetItem
                    {
                        Title = "7. Aumenta tu habilidad para adquirir",
                        Subtitle = "Tu mayor activo eres tú mismo y tu sabiduría.",
                        Content = "El deseo debe preceder a la realización. Cultiva tus propias facultades, estudia y vuélvete más sabio.\n\nCuanto más conocimientos adquieras, más riqueza podrás ganar. El hombre que busca aprender siempre será recompensado.",
                        Icon = "psychology.png", 
                        IsRead = false,
                        IsLocked = true
                    }
               };

                await _database.InsertAllAsync(lessons);
            }

        }

        //METODO PARA OBTENER LAS LECCIONES// METHOD TO GET LESSONS
        public async Task<List<MindsetItem>> GetMindsetItemsAsync()
        {
            await Init();
            return await _database.Table<MindsetItem>().ToListAsync();
        }


        //METODO PARA DESBLOQUEAR LECCIONES// METHOD TO UNLOCK LESSONS
        public async Task CompleteLessonAsync(MindsetItem currentLesson)
        {
            await Init();

            // 1. Marcar la actual como LEÍDA/ Mark the current one as READ
            currentLesson.IsRead = true;
            await _database.UpdateAsync(currentLesson);

            // 2. Buscar la SIGUIENTE lección (ID + 1)/ Find the NEXT lesson (ID + 1)
            var nextLessonId = currentLesson.Id + 1;
            var nextLesson = await _database.Table<MindsetItem>()
                                            .Where(l => l.Id == nextLessonId)
                                            .FirstOrDefaultAsync();

            // 3. Si existe la siguiente, la DESBLOQUEAMOS/ If the next one exists, we UNLOCK it
            if (nextLesson != null && nextLesson.IsLocked)
            {
                nextLesson.IsLocked = false;
                await _database.UpdateAsync(nextLesson);
            }
        }

        //--- METODOS PARA LIBROS RECOMENDADOS/RECOMMENDED BOOKS METHODS ---
        private async Task SeedBookDataAsync()
        {
            var count = await _database.Table<BookRecommendation>().CountAsync();
            if (count == 0)
            {
                var books = new List<BookRecommendation>
                {
                    new BookRecommendation { Title = "El Hombre Más Rico de Babilonia", Author = "George S. Clason", Description = "Los principios fundamentales de la riqueza.", Category = "Finanzas", HexStart = "#B88746",HexEnd = "#FDF5A6" },
                    new BookRecommendation { Title = "La psicología del dinero", Author = "Morgan Housel", Description = "Derriba el mito de los ingresos elevados.", Category = "Finanzas", HexStart = "#4568DC", HexEnd = "#B06AB3"},
                    new BookRecommendation { Title = "Piense y Hágase Rico", Author = "Napoleon Hill", Description = "La filosofía del éxito.", Category = "Mentalidad",HexStart = "#FF9966",HexEnd = "#FF5E62"},
                    new BookRecommendation { Title = "Cómo hacer que te pasen cosas buenas", Author = "Marian Rojas Estapé", Description = "Pequeños cambios, resultados extraordinarios.", Category = "Mentalidad",HexStart = "#DA4453", HexEnd = "#89216B"}
                };
                await _database.InsertAllAsync(books);
            }
        }

        public async Task<List<BookRecommendation>> GetBooksAsync()
        {
            await Init();
            return await _database.Table<BookRecommendation>().ToListAsync();
        }

    }
}