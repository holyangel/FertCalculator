using System.Diagnostics;
using FertCalculatorMaui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FertCalculatorMaui;

public partial class AppShell : Shell
{
    private readonly MainPage mainPage;
    private readonly IServiceProvider serviceProvider;
    
    public AppShell(IServiceProvider serviceProvider)
    {
        try
        {
            Debug.WriteLine("AppShell constructor starting");
            InitializeComponent();
            Debug.WriteLine("AppShell InitializeComponent completed");
            
            this.serviceProvider = serviceProvider;
            
            // Get the MainPage from the service provider
            mainPage = serviceProvider.GetRequiredService<MainPage>();
            
            // Register routes for navigation
            Routing.RegisterRoute(nameof(AddFertilizerPage), typeof(AddFertilizerPage));
            Routing.RegisterRoute(nameof(SaveMixPage), typeof(SaveMixPage));
            Routing.RegisterRoute(nameof(CompareMixPage), typeof(CompareMixPage));
            Routing.RegisterRoute(nameof(ManageFertilizersPage), typeof(ManageFertilizersPage));
            Routing.RegisterRoute(nameof(ImportOptionsPage), typeof(ImportOptionsPage));
            Routing.RegisterRoute(nameof(ExportOptionsPage), typeof(ExportOptionsPage));
            Debug.WriteLine("AppShell routes registered");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in AppShell constructor: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
    }
    
    private async void OnManageFertilizersClicked(object sender, EventArgs e)
    {
        try
        {
            await mainPage.NavigateToManageFertilizers();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OnManageFertilizersClicked: {ex.Message}\n{ex.StackTrace}");
        }
        // Close the flyout after selection
        FlyoutIsPresented = false;
    }
    
    private async void OnLoadMixClicked(object sender, EventArgs e)
    {
        try
        {
            await mainPage.LoadMixFromMenu();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OnLoadMixClicked: {ex.Message}\n{ex.StackTrace}");
        }
        FlyoutIsPresented = false;
    }
    
    private async void OnSaveMixClicked(object sender, EventArgs e)
    {
        try
        {
            await mainPage.SaveMixFromMenu();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OnSaveMixClicked: {ex.Message}\n{ex.StackTrace}");
        }
        FlyoutIsPresented = false;
    }
    
    private async void OnClearMixClicked(object sender, EventArgs e)
    {
        try
        {
            await mainPage.ClearMixFromMenu();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OnClearMixClicked: {ex.Message}\n{ex.StackTrace}");
        }
        FlyoutIsPresented = false;
    }
    
    private async void OnCompareMixesClicked(object sender, EventArgs e)
    {
        try
        {
            await mainPage.CompareMixesFromMenu();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OnCompareMixesClicked: {ex.Message}\n{ex.StackTrace}");
        }
        FlyoutIsPresented = false;
    }
    
    private void OnToggleMixVisibilityClicked(object sender, EventArgs e)
    {
        mainPage?.ToggleMixVisibilityFromMenu();
        FlyoutIsPresented = false;
    }
    
    private async void OnImportClicked(object sender, EventArgs e)
    {
        try
        {
            await mainPage.ImportFromMenu();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OnImportClicked: {ex.Message}\n{ex.StackTrace}");
        }
        FlyoutIsPresented = false;
    }
    
    private async void OnExportClicked(object sender, EventArgs e)
    {
        try
        {
            await mainPage.ExportFromMenu();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OnExportClicked: {ex.Message}\n{ex.StackTrace}");
        }
        FlyoutIsPresented = false;
    }
}
