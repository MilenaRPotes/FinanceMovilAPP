using Microsoft.Extensions.Logging;
using FinanceMovilApp.Services;
using FinanceMovilApp.ViewModels;
using FinanceMovilApp.Views;
using CommunityToolkit.Maui;
using SkiaSharp.Extended.UI;
using SkiaSharp.Views.Maui.Controls.Hosting;



namespace FinanceMovilApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()


                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Lora-Regular.ttf", "LoraFont"); //fuentes personalizadas
                    fonts.AddFont("Inter-Regular.ttf", "InterFont");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            //--- 1. Register Services (Singleton) ---
            builder.Services.AddSingleton<LocalDbService>();

            //--- 2. Register ViewModels (Transient) ---
            builder.Services.AddTransient<AddTransactionPage>();
            builder.Services.AddTransient<AddTransactionViewModel>();

            //--- 3. Dashboard Page and ViewModel ---
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<DashboardViewModel>();


            //--- 4. Goals Page and ViewModel ---
            builder.Services.AddTransient<GoalsPage>();
            builder.Services.AddTransient<GoalsViewModel>(); // ViewModel Principal
            builder.Services.AddTransient<AddGoalPage>();    // Vista Formulario
            builder.Services.AddTransient<AddGoalViewModel>(); // ViewModel Formulario

            // --- 5. Budget Page and ViewModel ---
            builder.Services.AddTransient<BudgetPage>();
            builder.Services.AddTransient<BudgetViewModel>();
            builder.Services.AddTransient<SetBudgetPage>();
            builder.Services.AddTransient<SetBudgetViewModel>();

            // --- 6.Mindset ---
            builder.Services.AddTransient<MindsetPage>();
            builder.Services.AddTransient<MindsetViewModel>();

            // Mindset Detail and ViewModel
            builder.Services.AddTransient<MindsetDetailPage>();
            builder.Services.AddTransient<MindsetDetailViewModel>();

            return builder.Build();
        }
    }
}
