using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Serialization;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

public partial class MainPage : ContentPage
{
    private ObservableCollection<Fertilizer> availableFertilizers;
    private ObservableCollection<FertilizerMix> savedMixes;
    private ObservableCollection<FertilizerQuantity> currentMix;
    private bool isMetricUnits = true; // true = per liter, false = per gallon
    private const double GALLON_TO_LITER = 3.78541;
    private string fertilizerDbPath = "Fertilizers.xml";
    private string mixesDbPath = "Mixes.xml";
    private readonly FileService fileService;
    private string unitLabelText = "g/L"; // Default unit text

    public MainPage(FileService fileService)
    {
        try
        {
            Debug.WriteLine("MainPage constructor starting");
            InitializeComponent();
            Debug.WriteLine("MainPage InitializeComponent completed");
            
            // Initialize service
            if (fileService == null)
            {
                Debug.WriteLine("WARNING: fileService is null in MainPage constructor");
                fileService = new FileService();
            }
            this.fileService = fileService;
            Debug.WriteLine("FileService initialized");
            
            // Initialize collections
            availableFertilizers = new ObservableCollection<Fertilizer>();
            savedMixes = new ObservableCollection<FertilizerMix>();
            currentMix = new ObservableCollection<FertilizerQuantity>();
            Debug.WriteLine("Collections initialized");
            
            // Load saved data
            _ = LoadFertilizersAsync();
            _ = LoadMixesAsync();
            
            // Set up data bindings
            FertilizerListView.ItemsSource = availableFertilizers;
            MixListView.ItemsSource = currentMix;
            Debug.WriteLine("Data bindings set up");
            
            // Set up unit toggle
            UpdateUnitDisplay();
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
    }

    private async Task LoadFertilizersAsync()
    {
        try
        {
            var fertilizers = await fileService.LoadFromXmlAsync<List<Fertilizer>>(fertilizerDbPath);
            if (fertilizers != null)
            {
                availableFertilizers.Clear();
                // Sort fertilizers alphabetically by name before adding to the collection
                foreach (var fertilizer in fertilizers.OrderBy(f => f.Name))
                {
                    availableFertilizers.Add(fertilizer);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error loading fertilizers: {ex.Message}", "OK");
        }
    }

    private async Task LoadMixesAsync()
    {
        try
        {
            var mixes = await fileService.LoadFromXmlAsync<List<FertilizerMix>>(mixesDbPath);
            if (mixes != null)
            {
                savedMixes.Clear();
                foreach (var mix in mixes)
                {
                    savedMixes.Add(mix);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error loading mixes: {ex.Message}", "OK");
        }
    }

    private async void OnFertilizerSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Fertilizer selectedFertilizer)
        {
            // Check if already in mix
            if (currentMix.Any(i => i.FertilizerName == selectedFertilizer.Name))
            {
                await DisplayAlert("Already Added", "This fertilizer is already in the current mix.", "OK");
                FertilizerListView.SelectedItem = null;
                return;
            }

            // Add to mix
            var quantity = new FertilizerQuantity
            {
                FertilizerName = selectedFertilizer.Name,
                Quantity = 1.0 // Default quantity
            };

            currentMix.Add(quantity);
            UpdateNutrientTotals();
            FertilizerListView.SelectedItem = null;
        }
    }

    private async void OnAddFertilizerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddFertilizerPage(fileService));
    }

    private void OnRemoveFertilizerClicked(object sender, EventArgs e)
    {
        if (MixListView.SelectedItem is FertilizerQuantity selectedItem)
        {
            currentMix.Remove(selectedItem);
            UpdateNutrientTotals();
        }
        else
        {
            DisplayAlert("No Selection", "Please select a fertilizer to remove", "OK");
        }
    }

    private void OnQuantityChanged(object sender, EventArgs e)
    {
        UpdateNutrientTotals();
    }

    private void UpdateNutrientTotals()
    {
        double nitrogenTotal = 0;
        double phosphorusTotal = 0;
        double potassiumTotal = 0;
        
        foreach (var item in currentMix)
        {
            // Find the fertilizer in the available list
            var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == item.FertilizerName);
            if (fertilizer != null)
            {
                double quantity = item.Quantity;
                
                // If using gallons, adjust calculation
                if (!isMetricUnits)
                {
                    quantity *= GALLON_TO_LITER;
                }
                
                nitrogenTotal += fertilizer.NitrogenPercent * quantity / 100;
                phosphorusTotal += fertilizer.PhosphorusPercent * quantity / 100;
                potassiumTotal += fertilizer.PotassiumPercent * quantity / 100;
            }
        }
        
        // Update UI with totals
        NitrogenPercentLabel.Text = nitrogenTotal.ToString("F2");
        PhosphorusPercentLabel.Text = phosphorusTotal.ToString("F2");
        PotassiumPercentLabel.Text = potassiumTotal.ToString("F2");
        
        NitrogenPpmLabel.Text = (nitrogenTotal * 10).ToString("F1");
        PhosphorusPpmLabel.Text = (phosphorusTotal * 10).ToString("F1");
        PotassiumPpmLabel.Text = (potassiumTotal * 10).ToString("F1");
    }

    private void UpdateUnitDisplay()
    {
        // Update the unit text field (used for binding)
        unitLabelText = isMetricUnits ? "g/L" : "g/gal";
        
        // Force the collection view to refresh so it picks up the new unit text
        var temp = MixListView.ItemsSource;
        MixListView.ItemsSource = null;
        MixListView.ItemsSource = temp;
    }

    private void OnToggleUnitsClicked(object sender, EventArgs e)
    {
        isMetricUnits = !isMetricUnits;
        UpdateUnitDisplay();
        UpdateNutrientTotals();
    }

    private async void OnSaveMixClicked(object sender, EventArgs e)
    {
        if (currentMix.Count == 0)
        {
            await DisplayAlert("Error", "Cannot save an empty mix", "OK");
            return;
        }
        
        // Navigate to save mix page
        await Navigation.PushAsync(new SaveMixPage(fileService, currentMix.ToList()));
    }

    private async void OnLoadMixClicked(object sender, EventArgs e)
    {
        if (savedMixes.Count == 0)
        {
            await DisplayAlert("No Saved Mixes", "There are no saved mixes to load", "OK");
            return;
        }
        
        // Create a list of mix names for the user to choose from
        string[] mixNames = savedMixes.Select(m => m.Name).ToArray();
        string result = await DisplayActionSheet("Select a Mix", "Cancel", null, mixNames);
        
        if (result != "Cancel" && !string.IsNullOrEmpty(result))
        {
            // Find the selected mix
            var selectedMix = savedMixes.FirstOrDefault(m => m.Name == result);
            if (selectedMix != null)
            {
                // Ask if user wants to replace or add to current mix
                bool replace = await DisplayAlert("Load Mix", "Do you want to replace the current mix?", "Replace", "Add to Current");
                
                if (replace)
                {
                    currentMix.Clear();
                }
                
                // Add ingredients from selected mix
                foreach (var ingredient in selectedMix.Ingredients)
                {
                    // Check if already in mix when adding
                    if (!replace && currentMix.Any(i => i.FertilizerName == ingredient.FertilizerName))
                    {
                        // Skip existing items when adding to current mix
                        continue;
                    }
                    
                    currentMix.Add(new FertilizerQuantity
                    {
                        FertilizerName = ingredient.FertilizerName,
                        Quantity = ingredient.Quantity
                    });
                }
                
                UpdateNutrientTotals();
            }
        }
    }

    private async void OnClearMixClicked(object sender, EventArgs e)
    {
        if (currentMix.Count > 0)
        {
            bool confirm = await DisplayAlert("Confirm", "Are you sure you want to clear the current mix?", "Yes", "No");
            if (confirm)
            {
                currentMix.Clear();
                UpdateNutrientTotals();
            }
        }
    }
}
