using Microsoft.Extensions.Logging;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

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
            });

        // Register services
        builder.Services.AddSingleton<FileService>();
        
        // Register pages
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<AddFertilizerPage>();
        builder.Services.AddTransient<SaveMixPage>();
        builder.Services.AddTransient<CompareMixPage>();
        builder.Services.AddTransient<ImportOptionsPage>();
        builder.Services.AddTransient<ExportOptionsPage>();
        builder.Services.AddTransient<ManageFertilizersPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
