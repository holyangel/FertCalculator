using System.Diagnostics;
using FertCalculatorMaui.Converters;
using FertCalculatorMaui.Messages;
using FertCalculatorMaui.Models;
using FertCalculatorMaui.Services;
using FertCalculatorMaui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FertCalculatorMaui;

public partial class AppShell : Shell
{
    private readonly MainPage mainPage;
    private readonly IServiceProvider serviceProvider;
    private readonly MainViewModel mainViewModel;
    
    public AppShell(IServiceProvider serviceProvider)
    {
        try
        {
            Debug.WriteLine("AppShell constructor starting");
            InitializeComponent();
            Debug.WriteLine("AppShell InitializeComponent completed");
            
            if (serviceProvider == null)
            {
                Debug.WriteLine("ERROR: serviceProvider is null in AppShell constructor");
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            
            this.serviceProvider = serviceProvider;
            
            try
            {
                // Get the MainViewModel from the service provider
                mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                
                // Set the binding context for the shell to access the MainViewModel properties
                BindingContext = mainViewModel;
                
                // Get the MainPage from the service provider - this is optional and can be skipped if it causes issues
                try
                {
                    mainPage = serviceProvider.GetRequiredService<MainPage>();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Warning: Could not get MainPage from service provider: {ex.Message}");
                    // Continue without MainPage reference
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting services in AppShell constructor: {ex.Message}");
                throw; // Rethrow to let the DI container handle it
            }
            
            // Ensure the flyout is visible
            Debug.WriteLine($"Shell.FlyoutBehavior is set to: {FlyoutBehavior}");
            FlyoutBehavior = FlyoutBehavior.Flyout;
            
            // Register routes for navigation
            Routing.RegisterRoute(nameof(AddFertilizerPage), typeof(AddFertilizerPage));
            Routing.RegisterRoute(nameof(SaveMixPage), typeof(SaveMixPage));
            Routing.RegisterRoute(nameof(CompareMixPage), typeof(CompareMixPage));
            Routing.RegisterRoute(nameof(ManageFertilizersPage), typeof(ManageFertilizersPage));
            Routing.RegisterRoute(nameof(ImportOptionsPage), typeof(ImportOptionsPage));
            Routing.RegisterRoute(nameof(ExportOptionsPage), typeof(ExportOptionsPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Debug.WriteLine("AppShell routes registered");
            
            // Explicitly set the current item to ensure we have an active shell item
            if (Items.Count > 0)
            {
                CurrentItem = Items[0];
                Debug.WriteLine($"Set CurrentItem to {CurrentItem?.Title ?? "unknown"}");
            }
            else
            {
                Debug.WriteLine("WARNING: No Shell items found to set as CurrentItem");
            }
            
            Debug.WriteLine("AppShell constructor completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in AppShell constructor: {ex.Message}\n{ex.StackTrace}");
            throw; // Rethrow to let the DI container handle it
        }
    }
    
    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("AppShell.OnAppearing called");
        
        // Ensure flyout behavior is set
        FlyoutBehavior = FlyoutBehavior.Flyout;
        Debug.WriteLine($"AppShell.FlyoutBehavior set to {FlyoutBehavior} in OnAppearing");
        
        // Make sure we have a current item
        if (Items.Count > 0 && CurrentItem == null)
        {
            CurrentItem = Items[0];
            Debug.WriteLine($"Set CurrentItem to {CurrentItem?.Title ?? "unknown"} in OnAppearing");
        }
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
    
    private void OnToggleUnitsClicked(object sender, EventArgs e)
    {
        try
        {
            // Call the ToggleUnits command on the MainViewModel
            mainViewModel.ToggleUnitsCommand.Execute(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OnToggleUnitsClicked: {ex.Message}\n{ex.StackTrace}");
        }
        FlyoutIsPresented = false;
    }
    
    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in OnSettingsClicked: {ex.Message}\n{ex.StackTrace}");
        }
        FlyoutIsPresented = false;
    }
}
