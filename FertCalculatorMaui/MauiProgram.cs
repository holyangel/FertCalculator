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
            .UseSkiaSharp()  // Add SkiaSharp handlers
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
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
        
        // Register ViewModels
        builder.Services.AddSingleton<MainViewModel>();  // Singleton to maintain state across the app
        builder.Services.AddTransient<AddFertilizerViewModel>();
        builder.Services.AddTransient<SaveMixViewModel>();
        builder.Services.AddTransient<ImportOptionsViewModel>();
        builder.Services.AddTransient<ExportOptionsViewModel>();
        builder.Services.AddTransient<ManageFertilizersViewModel>();
        builder.Services.AddTransient<EditQuantityViewModel>();
        builder.Services.AddTransient<CompareMixViewModel>();
        
        // Register pages with proper dependency injection
        builder.Services.AddTransient<MainPage>(services => 
            new MainPage(
                services.GetRequiredService<FileService>(),
                services.GetRequiredService<IDialogService>()));

        builder.Services.AddTransient<AddFertilizerPage>(services => 
            new AddFertilizerPage(
                services.GetRequiredService<FileService>(),
                services.GetRequiredService<IDialogService>()));

        // For SaveMixPage, we need to provide an empty list of FertilizerQuantity objects
        // since it's typically created with runtime parameters
        builder.Services.AddTransient<SaveMixPage>(services => 
            new SaveMixPage(
                services.GetRequiredService<FileService>(),
                new List<FertilizerQuantity>()));

        builder.Services.AddTransient<ImportOptionsPage>(services => 
            new ImportOptionsPage(
                services.GetRequiredService<FileService>(),
                services.GetRequiredService<MainViewModel>().AvailableFertilizers,
                services.GetRequiredService<MainViewModel>().SavedMixes));

        builder.Services.AddTransient<ExportOptionsPage>(services => 
            new ExportOptionsPage(
                services.GetRequiredService<FileService>(),
                services.GetRequiredService<MainViewModel>().AvailableFertilizers.ToList(),
                services.GetRequiredService<MainViewModel>().SavedMixes));

        builder.Services.AddTransient<ManageFertilizersPage>(services => 
            new ManageFertilizersPage(
                services.GetRequiredService<FileService>(),
                services.GetRequiredService<IDialogService>(),
                services.GetRequiredService<MainViewModel>().AvailableFertilizers));

        // Note: EditQuantityPage is typically created with runtime parameters
        // and not directly from the DI container, so we register it without parameters
        builder.Services.AddTransient<EditQuantityPage>();

        // Register CompareMixPage with FileService and IDialogService
        builder.Services.AddTransient<CompareMixPage>(services => 
            new CompareMixPage(
                services.GetRequiredService<FileService>(),
                services.GetRequiredService<IDialogService>()));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
