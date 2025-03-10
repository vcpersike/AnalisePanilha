using AnalisePanilha.Services;
using AnalisePanilha.Shared.Services;
using AnalisePanilha.Shared.Services.Interfaces;
using AnalisePanilha.Shared.ViewModels;
using Microsoft.Extensions.Logging;

namespace AnalisePanilha
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
                });

            // Add device-specific services used by the AnalisePanilha.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddSingleton<IExcelComparisonService, ExcelComparisonService>();
            builder.Services.AddTransient<ExcelComparisonViewModel>();
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
