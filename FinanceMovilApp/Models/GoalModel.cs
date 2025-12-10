using System;
using SQLite;

namespace FinanceMovilApp.Models
{
    [Table("Goal")]
    public class GoalModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } // Nombre de la meta
        public decimal TargetAmount { get; set; } // Monto objetivo
        public decimal CurrentAmount { get; set; } // Monto actual ahorrado
        public DateTime Deadline { get; set; } // Fecha límite para alcanzar la meta

        public string VisualIcon { get; set; } // Icono representativo de la meta

        public bool IsCompleted { get; set; } // Indica si la meta ha sido alcanzada
        
        public DateTime? CompletionDate { get; set; } // Fecha de exacta de finalización de la meta

        //--Borrado Logico--
        public bool IsArchived { get; set; } // Indica si la meta ha sido archivada (borrado lógico)



        //Propiedad calculada para la barra de progreso(0 a 1)
        [Ignore]
        public double Progress => TargetAmount == 0 ? 0 : (double)(CurrentAmount / TargetAmount);

        [Ignore]
        public string ProgressText => $"{Progress * 100:F0}%";



    }
}
