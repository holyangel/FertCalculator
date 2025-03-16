using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using FertCalculatorMaui.Services;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;

namespace FertCalculatorMaui;

public partial class MainPage : ContentPage
{
    private readonly FileService fileService;
    private readonly IDialogService dialogService;
    private MainPageViewModel viewModel;

    public MainPage(FileService fileService, IDialogService dialogService)
    {
        try
        {
            Debug.WriteLine("MainPage constructor starting");
            InitializeComponent();
            Debug.WriteLine("MainPage InitializeComponent completed");
            
            // Initialize services
            if (fileService == null)
            {
                Debug.WriteLine("WARNING: fileService is null in MainPage constructor");
                fileService = new FileService();
            }
            this.fileService = fileService;
            
            if (dialogService == null)
            {
                Debug.WriteLine("WARNING: dialogService is null in MainPage constructor");
                dialogService = new DialogService();
            }
            this.dialogService = dialogService;
            
            // Initialize ViewModel
            viewModel = new MainPageViewModel(fileService);
            BindingContext = viewModel;
            Debug.WriteLine("ViewModel initialized and set as BindingContext");
            
            // Load saved data
            _ = LoadFertilizersAsync();
            _ = LoadMixesAsync();
            Debug.WriteLine("MainPage constructor completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in MainPage constructor: {ex.Message}\n{ex.StackTrace}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refresh fertilizer list when page becomes visible
        _ = LoadFertilizersAsync();
        _ = LoadMixesAsync();
        
        // Update unit labels when the page appears
        UpdateUnitLabelsInCollectionView();
        
        // Make sure we're registered for messages
        if (!WeakReferenceMessenger.Default.IsRegistered<AddFertilizerToMixMessage>(this))
        {
            WeakReferenceMessenger.Default.Register<AddFertilizerToMixMessage>(this, (r, message) => {
                // Let the ViewModel handle adding the fertilizer to the mix
                // and then refresh the UI
                RefreshMixListView();
            });
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
                        
                        // Import the data using the existing import functionality
                        var importResult = await fileService.ImportDataAsync(resourceStream);
                        
                        if (importResult.Success && importResult.ImportedData != null && 
                            importResult.ImportedData.Fertilizers != null && 
                            importResult.ImportedData.Fertilizers.Count > 0)
                        {
                            Debug.WriteLine($"Successfully imported {importResult.ImportedData.Fertilizers.Count} default fertilizers");
                            
                            // Save the imported fertilizers
                            await fileService.SaveFertilizersAsync(importResult.ImportedData.Fertilizers);
                            
                            // Update our fertilizers list with the imported data
                            fertilizers = importResult.ImportedData.Fertilizers;
                        }
                        else
                        {
                            Debug.WriteLine("Failed to import default fertilizers from embedded resource");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Embedded Fertilizers.xml resource not found");
                    }
                }
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
            await DisplayAlert("Error", $"Failed to load fertilizers: {ex.Message}", "OK");
        }
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
            await DisplayAlert("Error", $"Failed to load mixes: {ex.Message}", "OK");
        }
    }

    private void OnQuantityTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is FertilizerQuantity item)
        {
            // Get the text value using GetValue
            var textValue = entry.GetValue(Entry.TextProperty) as string;
            
            // Ensure the quantity is a valid number
            if (double.TryParse(textValue, out double quantity))
            {
                // Update the quantity in the model
                item.Quantity = quantity;
                
                // Update nutrient calculations
                viewModel.UpdateNutrientTotals();
            }
            else
            {
                // If not a valid number, revert to previous value
                entry.SetValue(Entry.TextProperty, item.Quantity.ToString());
            }
        }
    }

    private async void OnAddFertilizerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddFertilizerPage(fileService));
    }
    
    private async void OnEditFertilizerClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var fertilizerName = button.GetValue(Button.CommandParameterProperty) as string;
            if (!string.IsNullOrEmpty(fertilizerName))
            {
                var fertilizer = viewModel.AvailableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
                if (fertilizer != null)
                {
                    await Navigation.PushAsync(new AddFertilizerPage(fileService, fertilizer));
                }
            }
        }
    }
    
    private void OnRemoveFromMixClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var fertilizerName = button.GetValue(Button.CommandParameterProperty) as string;
            if (!string.IsNullOrEmpty(fertilizerName))
            {
                var itemToRemove = viewModel.CurrentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
                if (itemToRemove != null)
                {
                    viewModel.CurrentMix.Remove(itemToRemove);
                    viewModel.UpdateNutrientTotals();
                }
            }
        }
    }

    private async void OnUnitsToggled(object sender, ToggledEventArgs e)
    {
#if IOS || MACCATALYST
        viewModel.UseImperialUnits = e.Value;
#else
        // For other platforms, access the Switch control directly
        if (sender is Microsoft.Maui.Controls.Switch switchControl)
        {
            viewModel.UseImperialUnits = switchControl.IsToggled;
        }
#endif
        viewModel.UpdateUnitDisplay();
        viewModel.UpdateNutrientTotals();
        
        // Update all unit labels in the collection view
        UpdateUnitLabelsInCollectionView();
    }

    private void UpdateUnitLabelsInCollectionView()
    {
        // This method will be called when the units are toggled or when the collection view is populated
        if (MixListView != null)
        {
            foreach (var item in MixListView.GetVisualTreeDescendants())
            {
                if (item is Label label && label.StyleId == "UnitLabel")
                {
                    label.Text = viewModel.UnitsTypeLabel;
                }
            }
        }
    }

    private async void OnSaveMixClicked(object sender, EventArgs e)
    {
        if (viewModel.CurrentMix.Count == 0)
        {
            await DisplayAlert("Error", "Cannot save an empty mix", "OK");
            return;
        }
        
        // Navigate to save mix page
        var saveMixPage = new SaveMixPage(fileService, viewModel.CurrentMix.ToList());
        
        // Subscribe to the SaveCompletedEvent to refresh the mixes list after saving
        saveMixPage.ViewModel.SaveCompletedEvent += async (s, args) =>
        {
            if (args.Success)
            {
                // Reload the mixes after saving
                await LoadMixesAsync();
            }
        };
        
        await Navigation.PushAsync(saveMixPage);
    }

    private async void OnLoadMixClicked(object sender, EventArgs e)
    {
        await LoadMixFromPopupAsync();
    }

    private async void OnClearMixClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm", 
            "Are you sure you want to clear the current mix?", 
            "Yes", "No");
        
        if (confirm)
        {
            viewModel.CurrentMix.Clear();
            viewModel.UpdateNutrientTotals();
        }
    }

    private async void OnCompareMixesClicked(object sender, EventArgs e)
    {
        if (viewModel.CurrentMix.Count == 0)
        {
            await DisplayAlert("Error", "Current mix is empty. Add some fertilizers before comparing.", "OK");
            return;
        }
        
        await Navigation.PushAsync(new CompareMixPage(fileService, viewModel.CurrentMix.ToList(), viewModel.UseImperialUnits));
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ImportOptionsPage(fileService, viewModel.AvailableFertilizers, viewModel.SavedMixes));
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ExportOptionsPage(fileService, viewModel.AvailableFertilizers.ToList(), viewModel.SavedMixes));
    }

    private async void OnManageFertilizersClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ManageFertilizersPage(fileService, dialogService, viewModel.AvailableFertilizers));
    }
    
    private void OnIncrementSmallClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var fertilizerName = button.GetValue(Button.CommandParameterProperty) as string;
            if (!string.IsNullOrEmpty(fertilizerName))
            {
                var mixItem = viewModel.CurrentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
                
                if (mixItem != null)
                {
                    mixItem.Quantity += 0.1; // Increment by 0.1 gram
                    mixItem.Quantity = Math.Round(mixItem.Quantity, 1); // Round to 1 decimal place for precision
                    viewModel.UpdateNutrientTotals();
                }
            }
        }
    }
    
    private void OnDecrementSmallClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var fertilizerName = button.GetValue(Button.CommandParameterProperty) as string;
            if (!string.IsNullOrEmpty(fertilizerName))
            {
                var mixItem = viewModel.CurrentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
                
                if (mixItem != null && mixItem.Quantity > 0.1)
                {
                    mixItem.Quantity -= 0.1; // Decrement by 0.1 gram
                    mixItem.Quantity = Math.Round(mixItem.Quantity, 1); // Round to 1 decimal place for precision
                    viewModel.UpdateNutrientTotals();
                }
            }
        }
    }
    
    private void OnIncrementGramClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var fertilizerName = button.GetValue(Button.CommandParameterProperty) as string;
            if (!string.IsNullOrEmpty(fertilizerName))
            {
                var mixItem = viewModel.CurrentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
                
                if (mixItem != null)
                {
                    mixItem.Quantity += 1.0; // Increment by 1 gram
                    viewModel.UpdateNutrientTotals();
                }
            }
        }
    }
    
    private void OnDecrementGramClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var fertilizerName = button.GetValue(Button.CommandParameterProperty) as string;
            if (!string.IsNullOrEmpty(fertilizerName))
            {
                var mixItem = viewModel.CurrentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
                
                if (mixItem != null && mixItem.Quantity >= 1.0)
                {
                    mixItem.Quantity -= 1.0; // Decrement by 1 gram
                    viewModel.UpdateNutrientTotals();
                }
            }
        }
    }

    private async void OnFertilizerTapped(object sender, TappedEventArgs e)
    {
#if IOS || MACCATALYST
        string fertilizerName = e.Parameter as string;
#else
        // For other platforms, try to get the parameter from the binding context or tag
        string fertilizerName = null;
        if (sender is Grid grid && grid.BindingContext is FertilizerQuantity fertQuantity)
        {
            fertilizerName = fertQuantity.FertilizerName;
        }
#endif

        if (!string.IsNullOrEmpty(fertilizerName))
        {
            // Navigate to edit quantity page
            var fertilizer = viewModel.AvailableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
            var existingQuantity = viewModel.CurrentMix.FirstOrDefault(fq => fq.FertilizerName == fertilizerName);
            
            if (fertilizer != null && existingQuantity != null)
            {
                var editPage = new EditQuantityPage(
                    fertilizerName,
                    existingQuantity.Quantity,
                    viewModel.UnitsTypeLabel,
                    viewModel.UseImperialUnits);
                
                editPage.QuantityChanged += (s, args) =>
                {
                    // Update the quantity in the mix
                    existingQuantity.Quantity = args.NewQuantity;
                    viewModel.UpdateNutrientTotals();
                };
                
                await Navigation.PushAsync(editPage);
            }
        }
    }

    private void OnToggleMixVisibilityClicked(object sender, EventArgs e)
    {
        bool isVisible = MixButtonsLayout.IsVisible;
        
        // Toggle visibility by setting IsVisible to actually collapse the space
        MixButtonsLayout.IsVisible = !isVisible;
        MixListView.IsVisible = !isVisible;
        
        // Update button text
        ToggleMixVisibilityButton.SetValue(Button.TextProperty, isVisible ? "▲" : "▼");
    }

    // Public methods for AppShell menu integration
    public async Task NavigateToManageFertilizers()
    {
        await Navigation.PushAsync(new ManageFertilizersPage(fileService, dialogService, viewModel.AvailableFertilizers));
    }
    
    public async Task LoadMixFromMenu()
    {
        await LoadMixFromPopupAsync();
    }
    
    public async Task SaveMixFromMenu()
    {
        if (viewModel.CurrentMix.Count == 0)
        {
            await DisplayAlert("Error", "Cannot save an empty mix", "OK");
            return;
        }
        
        // Navigate to save mix page
        var saveMixPage = new SaveMixPage(fileService, viewModel.CurrentMix.ToList());
        
        // Subscribe to the SaveCompletedEvent to refresh the mixes list after saving
        saveMixPage.ViewModel.SaveCompletedEvent += async (s, args) =>
        {
            if (args.Success)
            {
                // Reload the mixes after saving
                await LoadMixesAsync();
            }
        };
        
        await Navigation.PushAsync(saveMixPage);
    }
    
    public async Task ClearMixFromMenu()
    {
        bool confirm = await DisplayAlert("Confirm", 
            "Are you sure you want to clear the current mix?", 
            "Yes", "No");
        
        if (confirm)
        {
            viewModel.CurrentMix.Clear();
            viewModel.UpdateNutrientTotals();
        }
    }
    
    public async Task CompareMixesFromMenu()
    {
        if (viewModel.CurrentMix.Count == 0)
        {
            await DisplayAlert("Error", "Current mix is empty. Add some fertilizers before comparing.", "OK");
            return;
        }
        
        await Navigation.PushAsync(new CompareMixPage(fileService, viewModel.CurrentMix.ToList(), viewModel.UseImperialUnits));
    }
    
    public void ToggleMixVisibilityFromMenu()
    {
        OnToggleMixVisibilityClicked(null, EventArgs.Empty);
    }
    
    public async Task ImportFromMenu()
    {
        await Navigation.PushAsync(new ImportOptionsPage(fileService, viewModel.AvailableFertilizers, viewModel.SavedMixes));
    }
    
    public async Task ExportFromMenu()
    {
        await Navigation.PushAsync(new ExportOptionsPage(fileService, viewModel.AvailableFertilizers.ToList(), viewModel.SavedMixes));
    }

    private async Task LoadMixFromPopupAsync()
    {
        var mixes = viewModel.SavedMixes;
        if (mixes.Count == 0)
        {
            await DisplayAlert("No Mixes", "No saved mixes found.", "OK");
            return;
        }

        var mixNames = mixes.Select(m => m.Name).ToArray();
        string selectedMix = await DisplayActionSheet("Select Mix", "Cancel", null, mixNames);
        
        if (selectedMix != "Cancel" && !string.IsNullOrEmpty(selectedMix))
        {
            var mix = mixes.FirstOrDefault(m => m.Name == selectedMix);
            if (mix != null)
            {
                await LoadMix(mix);
            }
        }
    }

    private async Task LoadMix(FertilizerMix mix)
    {
        // Ask if user wants to replace or add to current mix
        bool replaceCurrentMix = await DisplayAlert("Load Mix", 
            "Do you want to replace the current mix?", 
            "Replace", "Add to Current");
        
        if (replaceCurrentMix)
        {
            viewModel.CurrentMix.Clear();
        }
        
        // Add each ingredient from the saved mix
        foreach (var ingredient in mix.Ingredients)
        {
            // Check if fertilizer exists
            if (viewModel.AvailableFertilizers.Any(f => f.Name == ingredient.FertilizerName))
            {
                // Check if it's already in the current mix when adding
                var existingItem = viewModel.CurrentMix.FirstOrDefault(i => i.FertilizerName == ingredient.FertilizerName);
                
                if (existingItem != null && !replaceCurrentMix)
                {
                    // Add to existing quantity
                    existingItem.Quantity += ingredient.Quantity;
                }
                else
                {
                    // Add as new ingredient
                    viewModel.CurrentMix.Add(new FertilizerQuantity
                    {
                        FertilizerName = ingredient.FertilizerName,
                        Quantity = ingredient.Quantity
                    });
                }
            }
            else
            {
                await DisplayAlert("Warning", 
                    $"Fertilizer '{ingredient.FertilizerName}' not found in available fertilizers. It will be skipped.", 
                    "OK");
            }
        }
        
        // Update nutrient totals
        viewModel.UpdateNutrientTotals();
        
        // Force refresh the UI
        RefreshMixListView();
    }

    private async void OnLoadMixFromPopupClicked(object sender, EventArgs e)
    {
        await LoadMixFromPopupAsync();
    }

    private async void OnCompareMixClicked(object sender, EventArgs e)
    {
        // Create a list of fertilizer quantities from the current mix
        var currentMixList = viewModel.CurrentMix.ToList();
        
        // Navigate to the CompareMixPage, passing the current mix and available fertilizers
        await Navigation.PushAsync(new CompareMixPage(fileService, currentMixList, viewModel.UseImperialUnits));
    }

    private void RefreshMixListView()
    {
        // Force refresh the UI by temporarily clearing and resetting the ItemsSource
        MixListView.ItemsSource = null;
        MixListView.ItemsSource = viewModel.CurrentMix;
    }
}
