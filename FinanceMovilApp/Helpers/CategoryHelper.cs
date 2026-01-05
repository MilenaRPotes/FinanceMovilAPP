using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceMovilApp.Helpers
{

    public static class CategoryHelper
    {
        //Lista de categorias GASTOS (Para Transacciones y Presupuesto) /  Expenses Category List (For Transactions and Budget)
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

        //Lista de categorias INGRESOS (Para Transacciones) / Income Category List (For Transactions)
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

