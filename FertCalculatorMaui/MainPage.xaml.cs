using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using FertCalculatorMaui.Services;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using FertCalculatorMaui.ViewModels;

namespace FertCalculatorMaui;

public partial class MainPage : ContentPage
{
    private readonly FileService fileService;
    private readonly IDialogService dialogService;
    private readonly MainViewModel viewModel;

    public MainPage(FileService fileService, IDialogService dialogService, MainViewModel viewModel)
    {
        try
        {
            Debug.WriteLine("MainPage constructor starting");
            InitializeComponent();
            Debug.WriteLine("MainPage InitializeComponent completed");
            
            // Initialize services using dependency injection
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            
            // Use the injected MainViewModel
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            BindingContext = this.viewModel;
            Debug.WriteLine("Injected ViewModel set as BindingContext");
            
            // Register for unit change messages
            WeakReferenceMessenger.Default.Register<UnitChangedMessage>(this, OnUnitChanged);
            Debug.WriteLine("Registered for UnitChangedMessage");
            
            // Load saved data asynchronously
            _ = InitializeDataAsync();
            Debug.WriteLine("MainPage constructor completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in MainPage constructor: {ex.Message}\n{ex.StackTrace}");
            throw; // Rethrow to let the DI container handle it
        }
    }

    private void OnUnitChanged(object recipient, UnitChangedMessage message)
    {
        try
        {
            Debug.WriteLine($"MainPage received UnitChangedMessage: UseImperialUnits = {message.Value}");
            
            // Ensure we're on the UI thread
            MainThread.BeginInvokeOnMainThread(() => {
                // Refresh the UI with the new unit values
                viewModel.UpdateUnitDisplay();
                viewModel.UpdateNutrientTotals();
                Debug.WriteLine("Updated UI after unit change");
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnUnitChanged: {ex.Message}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = RefreshDataAsync();
    }

    private async Task InitializeDataAsync()
    {
        try
        {
            await Task.WhenAll(
                LoadFertilizersAsync(),
                LoadMixesAsync()
            );
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", 
                "Failed to initialize data. Please restart the application. Error: " + ex.Message, "OK");
        }
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            await Task.WhenAll(
                LoadFertilizersAsync(),
                LoadMixesAsync()
            );
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", 
                "Failed to refresh data: " + ex.Message, "OK");
        }
    }

    private async Task LoadFertilizersAsync()
    {
        try
        {
            var fertilizers = await fileService.LoadFertilizersAsync();
            
            // If no fertilizers are found, try to import the default ones
            if (fertilizers == null || fertilizers.Count == 0)
            {
                Debug.WriteLine("No fertilizers found. Attempting to import default fertilizers.");
                fertilizers = await ImportDefaultFertilizersAsync();
            }
            
            if (fertilizers != null)
            {
                viewModel.AvailableFertilizers.Clear();
                // Sort fertilizers alphabetically by name before adding to the collection
                foreach (var fertilizer in fertilizers.OrderBy(f => f.Name))
                {
                    viewModel.AvailableFertilizers.Add(fertilizer);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading fertilizers: {ex.Message}\n{ex.StackTrace}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to load fertilizers: {ex.Message}", "OK");
        }
    }

    private async Task<List<Fertilizer>> ImportDefaultFertilizersAsync()
    {
        try
        {
            // List all embedded resources to help diagnose issues
            var resourceNames = GetType().Assembly.GetManifestResourceNames();
            Debug.WriteLine($"Available embedded resources ({resourceNames.Length}):");
            foreach (var resourceName in resourceNames)
            {
                Debug.WriteLine($"  - {resourceName}");
            }
            
            // Get the embedded Fertilizers.xml resource
            using (Stream resourceStream = GetType().Assembly.GetManifestResourceStream("FertCalculatorMaui.Fertilizers.xml"))
            {
                if (resourceStream != null)
                {
                    Debug.WriteLine("Found embedded Fertilizers.xml resource. Importing...");
                    
                    var importResult = await fileService.ImportDataAsync(resourceStream);
                    
                    if (importResult.Success && importResult.ImportedData?.Fertilizers?.Count > 0)
                    {
                        Debug.WriteLine($"Successfully imported {importResult.ImportedData.Fertilizers.Count} default fertilizers");
                        
                        // Save the imported fertilizers
                        await fileService.SaveFertilizersAsync(importResult.ImportedData.Fertilizers);
                        return importResult.ImportedData.Fertilizers;
                    }
                    
                    Debug.WriteLine("Failed to import default fertilizers from embedded resource");
                }
                else
                {
                    Debug.WriteLine("Embedded Fertilizers.xml resource not found");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error importing default fertilizers: {ex.Message}\n{ex.StackTrace}");
            await dialogService.DisplayAlertAsync("Error", 
                "Failed to import default fertilizers. The application may have limited functionality.", "OK");
        }
        
        return new List<Fertilizer>();
    }

    private async Task LoadMixesAsync()
    {
        try
        {
            var mixes = await fileService.LoadMixesAsync();
            if (mixes != null)
            {
                viewModel.SavedMixes.Clear();
                foreach (var mix in mixes)
                {
                    viewModel.SavedMixes.Add(mix);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading mixes: {ex.Message}\n{ex.StackTrace}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to load mixes: {ex.Message}", "OK");
        }
    }

    private async void OnEditQuantityTapped(object sender, EventArgs e)
    {
        if (sender is Label label && label.BindingContext is FertilizerQuantity fertilizer)
        {
            var editPage = new EditQuantityPage(fertilizer.FertilizerName, fertilizer.Quantity, 
                viewModel.UseImperialUnits ? "g/gal" : "g/L", viewModel.UseImperialUnits);
            
            editPage.QuantityChanged += (s, args) =>
            {
                var item = viewModel.CurrentMix.FirstOrDefault(f => f.FertilizerName == args.FertilizerName);
                if (item != null)
                {
                    item.Quantity = args.NewQuantity;
                    viewModel.UpdateNutrientTotals();
                }
            };
            
            await Navigation.PushAsync(editPage);
        }
    }

    // Menu command methods for AppShell
    public async Task NavigateToManageFertilizers()
    {
        try
        {
            Debug.WriteLine("NavigateToManageFertilizers called");
            await viewModel.ManageFertilizersCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in NavigateToManageFertilizers: {ex.Message}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to navigate to manage fertilizers: {ex.Message}", "OK");
        }
    }

    public async Task LoadMixFromMenu()
    {
        try
        {
            Debug.WriteLine("LoadMixFromMenu called");
            await viewModel.LoadMixCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in LoadMixFromMenu: {ex.Message}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to load mix: {ex.Message}", "OK");
        }
    }

    public async Task SaveMixFromMenu()
    {
        try
        {
            Debug.WriteLine("SaveMixFromMenu called");
            await viewModel.SaveMixCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SaveMixFromMenu: {ex.Message}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to save mix: {ex.Message}", "OK");
        }
    }

    public async Task ClearMixFromMenu()
    {
        try
        {
            Debug.WriteLine("ClearMixFromMenu called");
            await viewModel.ClearMixCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ClearMixFromMenu: {ex.Message}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to clear mix: {ex.Message}", "OK");
        }
    }

    public async Task CompareMixesFromMenu()
    {
        try
        {
            Debug.WriteLine("CompareMixesFromMenu called");
            await viewModel.CompareMixCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in CompareMixesFromMenu: {ex.Message}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to compare mixes: {ex.Message}", "OK");
        }
    }

    public void ToggleMixVisibilityFromMenu()
    {
        try
        {
            Debug.WriteLine("ToggleMixVisibilityFromMenu called");
            viewModel.ToggleMixVisibilityCommand.Execute(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ToggleMixVisibilityFromMenu: {ex.Message}");
            dialogService.DisplayAlert("Error", $"Failed to toggle mix visibility: {ex.Message}", "OK");
        }
    }

    public async Task ImportFromMenu()
    {
        try
        {
            Debug.WriteLine("ImportFromMenu called");
            await viewModel.ImportCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ImportFromMenu: {ex.Message}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to import data: {ex.Message}", "OK");
        }
    }

    public async Task ExportFromMenu()
    {
        try
        {
            Debug.WriteLine("ExportFromMenu called");
            await viewModel.ExportCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ExportFromMenu: {ex.Message}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to export data: {ex.Message}", "OK");
        }
    }
}
