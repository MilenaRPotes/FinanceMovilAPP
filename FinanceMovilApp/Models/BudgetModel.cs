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

        public string CategoryName { get; set; } // Nombre de la categoría presupuestaria

        public decimal PlannedAmount { get; set; } // Monto presupuestado para la categoría

        public int Month { get; set; } // Mes al que corresponde el presupuesto (1-12)
        public int Year { get; set; } // Año al que corresponde el presupuesto

        //-- PROPIEDADES CALCULADAS (No se guardan en BD)

        // Cuanto se ha gastado realmente en esta categoria durante el mes y año especificado (Cruza con transacciones)
        [Ignore]
        public decimal ActualSpent { get; set; }

        // Cuanto me queda del presupuesto (PlannedAmount - ActualSpent)
        [Ignore]
        public decimal RemainingAmount => PlannedAmount - ActualSpent;

        // Porcentaje de ejecución (0.0 a 1.0)
        [Ignore]
        public double Progress => PlannedAmount == 0 ? 0 : (double)(ActualSpent / PlannedAmount);

        // Color de la barra: Verde si estoy dentro del presupuesto, Rojo si lo he excedido
        [Ignore]
        public string StatusColor => ActualSpent > PlannedAmount ? "#E57373" : "#4CAF50"; // RojoCoral : VerdeCrecimiento


        //Propiedad para mostrar alerta cuando se excede el presupuesto
        [Ignore]
        public bool IsOverspent => ActualSpent > PlannedAmount;

    }
}
