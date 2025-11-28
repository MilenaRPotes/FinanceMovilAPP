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
            if(_database is not null)
                return;

            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

            //Create table using the new Model name 
            await _database.CreateTableAsync<TransactionModel>();
        }

        // --- TRANSACTION METHODS ---
        //CRUD
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


    }
}
