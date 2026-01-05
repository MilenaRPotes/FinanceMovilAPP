using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace FinanceMovilApp.Models
{
    [Table("Budget")]
    public partial class BudgetModel
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string CategoryName { get; set; } // Nombre de la categoría presupuestaria / Budget category name

        public decimal PlannedAmount { get; set; } // Monto presupuestado para la categoría / Budgeted amount for the category

        public int Month { get; set; } // Mes al que corresponde el presupuesto (1-12) / Month the budget corresponds to (1-12)
        public int Year { get; set; } // Año al que corresponde el presupuesto / Year the budget corresponds to

        //-- PROPIEDADES CALCULADAS (No se guardan en BD) /-- CALCULATED PROPERTIES (Not stored in DB) --

        // Cuanto se ha gastado realmente en esta categoria durante el mes y año especificado (Cruza con transacciones) / How much has actually been spent in this category during the specified month and year (Cross-references with transactions)
        [Ignore]
        public decimal ActualSpent { get; set; }

        // Cuanto me queda del presupuesto (PlannedAmount - ActualSpent) / How much budget is remaining (PlannedAmount - ActualSpent)
        [Ignore]
        public decimal RemainingAmount => PlannedAmount - ActualSpent;

        // Porcentaje de ejecución (0.0 a 1.0) / Execution percentage (0.0 to 1.0)
        [Ignore]
        public double Progress => PlannedAmount == 0 ? 0 : (double)(ActualSpent / PlannedAmount);

        // Color de la barra: Verde si estoy dentro del presupuesto, Rojo si lo he excedido / Bar color: Green if within budget, Red if exceeded
        [Ignore]
        public string StatusColor => ActualSpent > PlannedAmount ? "#E57373" : "#4CAF50"; // RojoCoral : VerdeCrecimiento


        //Propiedad para mostrar alerta cuando se excede el presupuesto / Property to show alert when budget is exceeded
        [Ignore]
        public bool IsOverspent => ActualSpent > PlannedAmount;

    }
}
