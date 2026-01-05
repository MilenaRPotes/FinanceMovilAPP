using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace FinanceMovilApp.Models
{
    [Table("Transactions")]
    public class TransactionModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }
        public DateTime Date { get; set; }

        //Category details // Detalles de la categoría
        public string CategoryName { get; set; }   
        public string CategoryIcon { get; set; } // bus, car, food, etc. 

        public bool IsIncome { get; set; } // true for income, false for expense /  true para ingreso, false para gasto

        // --- Feedback additions ---
        public bool IsRecurring { get; set; } // Indica si la transacción es recurrente / Indicates if the transaction is recurring
        public PaymentFrequency Frequency { get; set; } // Frecuencia de pago  / Payment frequency

    }
}
