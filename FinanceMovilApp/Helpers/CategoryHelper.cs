using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceMovilApp.Helpers
{

    public static class CategoryHelper
    {
      //Lista de categorias GASTOS (Para Transacciones y Presupuesto)
        public static List<string> ExpenseCategories = new List<string>
        {
            "Alimentacion",
            "Transporte",
            "Vivienda",
            "Servicios Públicos",
            "Salud",
            "Educación",
            "Entretenimiento",
            "Ropa",
            "Impuestos",
            "Deudas",
            "Otros Gastos"
        };

        //Lista de categorias INGRESOS (Para Transacciones)
        public static List<string> IncomeCategories = new List<string>
        {
            "Salario",
            "Negocio/Ventas",
            "Inversiones",
            "Regalos",
            "Otros Ingresos"
        };

    }
}

