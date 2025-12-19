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

            //Create table using the new Model name 
            await _database.CreateTableAsync<TransactionModel>();

            //Crea la tabla de metas
            await _database.CreateTableAsync<GoalModel>();

            //crea la tabla de presupuestos
            await _database.CreateTableAsync<BudgetModel>();
            //crea la tabla de mentalidad
            await _database.CreateTableAsync<MindsetItem>();
        }

        // --- TRANSACTION METHODS ---
        //METODOS CRUD
        public async Task<List<TransactionModel>> GetTransactionsAsync()
        {
            await Init();
            //Order by Date descending
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
        // 1. Obtener solo los últimos 'count' movimientos (ej. 5)
        public async Task<List<TransactionModel>> GetRecentTransactionsAsync(int count)
        {
            await Init();
            return await _database.Table<TransactionModel>()
                            .OrderByDescending(t => t.Date) // Ordenar por fecha (más nuevo primero)
                            .Take(count)                    // Tomar solo la cantidad solicitada
                            .ToListAsync();
        }

        // 2. Calcular el balance total directamente (sin cargar todo a la memoria visual)
        public async Task<decimal> GetTotalBalanceAsync()
        {
            await Init();
            // Traemos todo solo para sumar 
            var allTransactions = await _database.Table<TransactionModel>().ToListAsync();

            var totalIncome = allTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            var totalExpenses = allTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);

            return totalIncome - totalExpenses;
        }

        //--- METODOS PARA METAS ---
        public async Task<List<GoalModel>> GetGoalsAsync()
        {
            await Init();
            // Ordenar por estado de completitud y luego por fecha limite
            //Solo se muestran las activas(no archivadas)
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
            //marca la meta como completada con fecha historica 
            if (item.CurrentAmount >= item.TargetAmount)
            {
                // Si recien se esta completando
                // Si recién se está completando (antes no lo estaba)
                if (!item.IsCompleted)
                {
                    item.IsCompleted = true;
                    item.CompletionDate = DateTime.Now; // ¡Guardamos la fecha del logro!
                }

                item.CurrentAmount = item.TargetAmount; //Tope visual
            }
            else
            {
                item.IsCompleted = false;
                item.CompletionDate = null; // Si sacó dinero y dejó de estar completa, borramos la fecha
            }

            if (item.Id != 0)
                return await _database.UpdateAsync(item);
            else
                return await _database.InsertAsync(item);
        }

        //-- LOGICA DE ELIMINACION --
        public async Task<int> DeleteGoalAsync(GoalModel item)
        {
            await Init();
            //1. Meta Completa -> se archiva (Soft Delete) se mantiene el registro Histórico
            if (item.IsCompleted)
            {
                item.IsArchived = true;
                return await _database.UpdateAsync(item);
            }
            //2. Meta Incompleta -> se elimina fisicamente(Hard Delete)
            // Eliminación sin terminar meta  no  cuenta en las estadisticas
            else
            {
                return await _database.DeleteAsync(item);
            }
        }

        //--- METODOS PARA PRESUPUESTO ---
        //Calcular Ingresos Totales de un mes específico (Para base del presupuesto)
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
            //calcular el gasto real cruzando con transacciones
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

            //Evitar duplicados para misma categoria/mes/año
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

        //--- METODOS MENTALIDAD  ---
        private async Task SeedMindsetDataAsync() 
        {
            var count = await _database.Table<MindsetItem>().CountAsync();
            if (count == 0)
            {
               var lessons = new List<MindsetItem>
               {
                   //LECCION 1: ABIERTA (Islocked = false)
                    new MindsetItem
                    {
                        Title = "1. Empieza a llenar tu bolsa",
                        Subtitle = "De cada diez monedas que ganes, gasta solo nueve.",
                        Content = "Esta es la regla de oro fundamental. Págate a ti mismo primero. \n\nEsa décima moneda que guardas es la semilla de la que brotará tu árbol de riqueza. No importa cuán poco ganes, una parte debe ser siempre tuya para conservar.",
                        Icon = "money.png",
                        IsRead = false,
                        IsLocked = false // <--- ¡ABIERTA!
                    },

                    // LECCIONES 2-7: BLOQUEADAS (IsLocked = true)
                   // LECCIÓN 2
                    new MindsetItem
                    {
                        Title = "2. Controla tus gastos",
                        Subtitle = "Diferencia entre tus necesidades y tus deseos.",
                        Content = "No confundas gastos necesarios con deseos. El presupuesto no es para limitarte, sino para liberarte de la ansiedad.\n\nEs la lámpara que ilumina los agujeros de tu bolsa y te permite gastar en lo que realmente valoras.",
                        Icon = "shield.png",
                        IsRead = false,
                        IsLocked = true
                    },

                  // LECCIÓN 3
                    new MindsetItem
                    {
                        Title = "3. Haz que tu oro se multiplique",
                        Subtitle = "Pon a trabajar cada moneda para que se reproduzca.",
                        Content = "La riqueza no está en las monedas que llevas en la bolsa, sino en el flujo constante que llega a ella.\n\nHaz que cada moneda trabaje para ti como un obrero fiel, generando hijos que también trabajen para ti.",
                        Icon = "plant.png",
                        IsRead = false,
                        IsLocked = true
                    },

                    // LECCIÓN 4
                    new MindsetItem
                    {
                        Title = "4. Protege tus tesoros",
                        Subtitle = "Invierte solo donde tu capital esté seguro.",
                        Content = "El primer principio de la inversión es la seguridad del capital. No te dejes cegar por ganancias fantásticas pero irreales.\n\nConsulta a los sabios: no le preguntes al panadero sobre las estrellas, pregúntale al astrónomo.",
                        Icon = "lock.png", // Sugerencia: Icono de candado o caja fuerte
                        IsRead = false,
                        IsLocked = true
                    },

                    // LECCIÓN 5
                    new MindsetItem
                    {
                        Title = "5. Haz de tu morada una inversión",
                        Subtitle = "Poseer tu propio techo da confianza y reduce gastos.",
                        Content = "Nadie puede disfrutar plenamente de la vida si no tiene un terreno donde sus hijos jueguen seguros.\n\nTener tu propio refugio reduce el costo de vida y libera tu corazón para emprender mayores proyectos.",
                        Icon = "house.png", // Sugerencia: Icono de casa
                        IsRead = false,
                        IsLocked = true
                    },

                    // LECCIÓN 6
                    new MindsetItem
                    {
                        Title = "6. Asegura un ingreso futuro",
                        Subtitle = "Prevé para los días en que ya no puedas trabajar.",
                        Content = "La vida tiene estaciones. Es tu responsabilidad prever ingresos para su vejez y para la protección de tu familia si faltas.\n\nUn pequeño flujo seguro hoy es mejor que una gran fortuna incierta mañana.",
                        Icon = "umbrella.png", // Sugerencia: Icono de paraguas o escudo
                        IsRead = false,
                        IsLocked = true
                    },

                    // LECCIÓN 7
                    new MindsetItem
                    {
                        Title = "7. Aumenta tu habilidad para adquirir",
                        Subtitle = "Tu mayor activo eres tú mismo y tu sabiduría.",
                        Content = "El deseo debe preceder a la realización. Cultiva tus propias facultades, estudia y vuélvete más sabio.\n\nCuanto más conocimientos adquieras, más riqueza podrás ganar. El hombre que busca aprender siempre será recompensado.",
                        Icon = "book.png", // Sugerencia: Icono de libro o cerebro
                        IsRead = false,
                        IsLocked = true
                    }
               };

                await _database.InsertAllAsync(lessons);
            }

        }

        //METODO PARA DESBLOQUEAR LECCIONES
        public async Task CompleteLessonAsync(MindsetItem currentLesson)
        {
            await Init();

            // 1. Marcar la actual como LEÍDA
            currentLesson.IsRead = true;
            await _database.UpdateAsync(currentLesson);

            // 2. Buscar la SIGUIENTE lección (ID + 1)
            var nextLessonId = currentLesson.Id + 1;
            var nextLesson = await _database.Table<MindsetItem>()
                                            .Where(l => l.Id == nextLessonId)
                                            .FirstOrDefaultAsync();

            // 3. Si existe la siguiente, la DESBLOQUEAMOS
            if (nextLesson != null && nextLesson.IsLocked)
            {
                nextLesson.IsLocked = false;
                await _database.UpdateAsync(nextLesson);
            }
        }

    }
}