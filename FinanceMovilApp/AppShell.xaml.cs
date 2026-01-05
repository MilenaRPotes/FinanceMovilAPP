using FinanceMovilApp.Views;

namespace FinanceMovilApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Registro de rutas para las páginas de la aplicación
            // Transacciones
            Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
            
            // Metas
            Routing.RegisterRoute(nameof(AddGoalPage), typeof(AddGoalPage));
            
            // Presupuestos
            Routing.RegisterRoute(nameof(SetBudgetPage), typeof(SetBudgetPage));

            // Mentalidad
            Routing.RegisterRoute(nameof(MindsetDetailPage), typeof(MindsetDetailPage));
        }
    }
}
