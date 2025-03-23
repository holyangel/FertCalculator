using Microsoft.Extensions.Logging;
using FertCalculatorMaui.Services;
using FertCalculatorMaui.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FertCalculatorMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()  // Add SkiaSharp handlers for both LiveCharts and ColorPicker
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            })
            .ConfigureMauiHandlers(handlers =>
            {
                // Register any custom handlers here
            });

        // Configure LiveCharts
        LiveCharts.Configure(config =>
            config
                .AddSkiaSharp()
                .AddDefaultMappers()
                .AddLightTheme());

        // Register services as singletons for state persistence
        builder.Services.AddSingleton<FileService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<AppSettings>();
        
        // Register App class
        builder.Services.AddTransient<App>();
        
        // Register AppShell
        builder.Services.AddTransient<AppShell>();
        
        // Register ViewModels
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<AddFertilizerViewModel>();  // Singleton to maintain state across the app
        builder.Services.AddTransient<CompareMixViewModel>();
        builder.Services.AddTransient<EditQuantityViewModel>();
        builder.Services.AddTransient<ManageFertilizersViewModel>();
        builder.Services.AddTransient<SaveMixViewModel>();
        builder.Services.AddTransient<ImportOptionsViewModel>();
        builder.Services.AddTransient<ExportOptionsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        
        // Register Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ManageFertilizersPage>();
        builder.Services.AddTransient<SaveMixPage>();
        builder.Services.AddTransient<CompareMixPage>();
        builder.Services.AddTransient<ImportOptionsPage>();
        builder.Services.AddTransient<ExportOptionsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<AddFertilizerPage>();
        builder.Services.AddTransient<EditQuantityPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
