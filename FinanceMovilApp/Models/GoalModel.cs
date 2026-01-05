using System;
using SQLite;

namespace FinanceMovilApp.Models
{
    [Table("Goal")]
    public class GoalModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } // Nombre de la meta / name of goal 
        public decimal TargetAmount { get; set; } // Monto objetivo / Target Amount
        public decimal CurrentAmount { get; set; } // Monto actual ahorrado / Current Amount Saved
        public DateTime Deadline { get; set; } // Fecha límite para alcanzar la meta / Deadline to reach the goal

        public string VisualIcon { get; set; } // Icono representativo de la meta / Representative icon for the goal

        public bool IsCompleted { get; set; } // Indica si la meta ha sido alcanzada / Indicates if the goal has been reached"

        public DateTime? CompletionDate { get; set; } // Fecha de exacta de finalización de la meta / Exact completion date of the goal

        public bool IsArchived { get; set; } // Indica si la meta ha sido archivada (borrado lógico) / Indicates if the goal has been archived (soft delete)

        //Propiedad calculada para la barra de progreso(0 a 1) / Calculated property for the progress bar (0 to 1)
        [Ignore]
        public double Progress => TargetAmount == 0 ? 0 : (double)(CurrentAmount / TargetAmount);

        [Ignore]
        public string ProgressText => $"{Progress * 100:F0}%";



    }
}
