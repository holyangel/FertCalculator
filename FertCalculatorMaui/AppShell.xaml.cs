using System.Diagnostics;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

public partial class AppShell : Shell
{
    private MainPage mainPage;
    
    public AppShell()
    {
        try
        {
            Debug.WriteLine("AppShell constructor starting");
            InitializeComponent();
            Debug.WriteLine("AppShell InitializeComponent completed");
            
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
        
        // Get reference to the MainPage when it's loaded
        if (CurrentPage is MainPage page)
        {
            mainPage = page;
        }
    }
    
    private async void OnManageFertilizersClicked(object sender, EventArgs e)
    {
        if (mainPage != null)
        {
            await mainPage.NavigateToManageFertilizers();
        }
        // Close the flyout after selection
        FlyoutIsPresented = false;
    }
    
    private async void OnLoadMixClicked(object sender, EventArgs e)
    {
        if (mainPage != null)
        {
            await mainPage.LoadMixFromMenu();
        }
        FlyoutIsPresented = false;
    }
    
    private async void OnSaveMixClicked(object sender, EventArgs e)
    {
        if (mainPage != null)
        {
            await mainPage.SaveMixFromMenu();
        }
        FlyoutIsPresented = false;
    }
    
    private async void OnClearMixClicked(object sender, EventArgs e)
    {
        if (mainPage != null)
        {
            await mainPage.ClearMixFromMenu();
        }
        FlyoutIsPresented = false;
    }
    
    private async void OnCompareMixesClicked(object sender, EventArgs e)
    {
        if (mainPage != null)
        {
            await mainPage.CompareMixesFromMenu();
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
        if (mainPage != null)
        {
            await mainPage.ImportFromMenu();
        }
        FlyoutIsPresented = false;
    }
    
    private async void OnExportClicked(object sender, EventArgs e)
    {
        if (mainPage != null)
        {
            await mainPage.ExportFromMenu();
        }
        FlyoutIsPresented = false;
    }
}
