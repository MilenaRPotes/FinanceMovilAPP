using Microsoft.Extensions.Logging;
using FinanceMovilApp.Services;
using FinanceMovilApp.ViewModels;
using FinanceMovilApp.Views;

namespace FinanceMovilApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
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

            return builder.Build();
        }
    }
}
