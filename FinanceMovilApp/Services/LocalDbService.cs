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
    }
}
