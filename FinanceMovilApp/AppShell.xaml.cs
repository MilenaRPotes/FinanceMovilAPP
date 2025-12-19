using FinanceMovilApp.Views;

namespace FinanceMovilApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Registro de rutas para las páginas de la aplicación
            Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));

            Routing.RegisterRoute(nameof(AddGoalPage), typeof(AddGoalPage));

            Routing.RegisterRoute(nameof(SetBudgetPage), typeof(SetBudgetPage));
        }
    }
}
