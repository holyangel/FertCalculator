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
    private bool useImperialUnits = false; // false = per liter (metric), true = per gallon (imperial)
    private readonly FileService fileService;
    private AppSettings appSettings;

    public bool UseImperialUnits
    {
        get => useImperialUnits;
        set
        {
            useImperialUnits = value;
            UpdateUnitDisplay();
            UpdateNutrientTotals();
        }
    }

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
            
            // Initialize settings
            appSettings = new AppSettings();
            UseImperialUnits = appSettings.UseImperialUnits;
            
            // Set the binding context for the switch
            BindingContext = this;
            
            // Load saved data
            _ = LoadFertilizersAsync();
            _ = LoadMixesAsync();
            
            // Set up data bindings
            FertilizerListView.ItemsSource = availableFertilizers;
            MixListView.ItemsSource = currentMix;
            Debug.WriteLine("Data bindings set up");
            
            // Set up unit display
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
            var fertilizers = await fileService.LoadFertilizersAsync();
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
                savedMixes.Clear();
                foreach (var mix in mixes)
                {
                    savedMixes.Add(mix);
                }
                
                // Update the picker
                MixPicker.ItemsSource = savedMixes.Select(m => m.Name).ToList();
                MixPicker.IsEnabled = savedMixes.Count > 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load mixes: {ex.Message}", "OK");
        }
    }

    private void OnFertilizerSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var selectedFertilizer = e.CurrentSelection[0] as Fertilizer;
            if (selectedFertilizer != null)
            {
                // If the fertilizer is already in the mix, increment its quantity
                var existingItem = currentMix.FirstOrDefault(item => item.FertilizerName == selectedFertilizer.Name);
                if (existingItem != null)
                {
                    existingItem.Quantity += 1;
                }
                else
                {
                    // Add 1 gram of the selected fertilizer to the mix
                    currentMix.Add(new FertilizerQuantity { FertilizerName = selectedFertilizer.Name, Quantity = 1 });
                }
                
                // Update the UI
                UpdateNutrientTotals();
            }
            
            // Clear selection
            FertilizerListView.SelectedItem = null;
        }
    }
    
    private void OnQuantityChanged(object sender, EventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is FertilizerQuantity item)
        {
            // Ensure the quantity is a valid number
            if (double.TryParse(entry.Text, out double quantity))
            {
                // Update the quantity in the model
                item.Quantity = quantity;
                
                // Update nutrient calculations
                UpdateNutrientTotals();
            }
            else
            {
                // If not a valid number, revert to previous value
                entry.Text = item.Quantity.ToString();
            }
        }
    }

    private async void OnAddFertilizerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddFertilizerPage(fileService));
    }

    private void OnRemoveFertilizerClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is FertilizerQuantity item)
        {
            currentMix.Remove(item);
            
            // Update nutrient totals
            UpdateNutrientTotals();
        }
    }

    private void UpdateNutrientTotals()
    {
        // Calculate nutrient totals
        double nitrogenTotal = 0;
        double phosphorusTotal = 0;
        double potassiumTotal = 0;
        double calciumTotal = 0;
        double magnesiumTotal = 0;
        double sulfurTotal = 0;
        double boronTotal = 0;
        double copperTotal = 0;
        double ironTotal = 0;
        double manganeseTotal = 0;
        double molybdenumTotal = 0;
        double zincTotal = 0;
        double chlorineTotal = 0;
        double silicaTotal = 0;
        double humicAcidTotal = 0;
        double fulvicAcidTotal = 0;
        
        double totalGrams = 0;
        
        // Calculate for each fertilizer in the mix
        foreach (var item in currentMix)
        {
            var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == item.FertilizerName);
            if (fertilizer == null) continue;
            
            double grams = item.Quantity;
            totalGrams += grams;
            
            // Calculate weighted contribution of each nutrient
            nitrogenTotal += (fertilizer.NitrogenPercent / 100) * grams;
            phosphorusTotal += (fertilizer.PhosphorusPercent / 100) * grams;
            potassiumTotal += (fertilizer.PotassiumPercent / 100) * grams;
            calciumTotal += (fertilizer.CalciumPercent / 100) * grams;
            magnesiumTotal += (fertilizer.MagnesiumPercent / 100) * grams;
            sulfurTotal += (fertilizer.SulfurPercent / 100) * grams;
            boronTotal += (fertilizer.BoronPercent / 100) * grams;
            copperTotal += (fertilizer.CopperPercent / 100) * grams;
            ironTotal += (fertilizer.IronPercent / 100) * grams;
            manganeseTotal += (fertilizer.ManganesePercent / 100) * grams;
            molybdenumTotal += (fertilizer.MolybdenumPercent / 100) * grams;
            zincTotal += (fertilizer.ZincPercent / 100) * grams;
            chlorineTotal += (fertilizer.ChlorinePercent / 100) * grams;
            silicaTotal += (fertilizer.SilicaPercent / 100) * grams;
            humicAcidTotal += (fertilizer.HumicAcidPercent / 100) * grams;
            fulvicAcidTotal += (fertilizer.FulvicAcidPercent / 100) * grams;
        }
        
        // Convert to PPM (mg/L or mg/gal depending on units)
        if (totalGrams > 0)
        {
            // Calculate PPM in water-based solution
            // If imperial units are used, we need to adjust for gallons vs liters
            double conversionFactor = 1.0;
            if (useImperialUnits)
            {
                conversionFactor = 1.0 / 3.78541;
            }
            
            nitrogenTotal = (nitrogenTotal / totalGrams) * conversionFactor;
            phosphorusTotal = (phosphorusTotal / totalGrams) * conversionFactor;
            potassiumTotal = (potassiumTotal / totalGrams) * conversionFactor;
            calciumTotal = (calciumTotal / totalGrams) * conversionFactor;
            magnesiumTotal = (magnesiumTotal / totalGrams) * conversionFactor;
            sulfurTotal = (sulfurTotal / totalGrams) * conversionFactor;
            boronTotal = (boronTotal / totalGrams) * conversionFactor;
            copperTotal = (copperTotal / totalGrams) * conversionFactor;
            ironTotal = (ironTotal / totalGrams) * conversionFactor;
            manganeseTotal = (manganeseTotal / totalGrams) * conversionFactor;
            molybdenumTotal = (molybdenumTotal / totalGrams) * conversionFactor;
            zincTotal = (zincTotal / totalGrams) * conversionFactor;
            chlorineTotal = (chlorineTotal / totalGrams) * conversionFactor;
            silicaTotal = (silicaTotal / totalGrams) * conversionFactor;
            humicAcidTotal = (humicAcidTotal / totalGrams) * conversionFactor;
            fulvicAcidTotal = (fulvicAcidTotal / totalGrams) * conversionFactor;
        }
        
        // Update the UI
        NitrogenPpmLabel.Text = (nitrogenTotal * 1000).ToString("F1");
        PhosphorusPpmLabel.Text = (phosphorusTotal * 1000).ToString("F1");
        PotassiumPpmLabel.Text = (potassiumTotal * 1000).ToString("F1");
        CalciumPpmLabel.Text = (calciumTotal * 1000).ToString("F1");
        MagnesiumPpmLabel.Text = (magnesiumTotal * 1000).ToString("F1");
        SulfurPpmLabel.Text = (sulfurTotal * 1000).ToString("F1");
        BoronPpmLabel.Text = (boronTotal * 1000).ToString("F1");
        CopperPpmLabel.Text = (copperTotal * 1000).ToString("F1");
        IronPpmLabel.Text = (ironTotal * 1000).ToString("F1");
        ManganesePpmLabel.Text = (manganeseTotal * 1000).ToString("F1");
        MolybdenumPpmLabel.Text = (molybdenumTotal * 1000).ToString("F1");
        ZincPpmLabel.Text = (zincTotal * 1000).ToString("F1");
        ChlorinePpmLabel.Text = (chlorineTotal * 1000).ToString("F1");
        SilicaPpmLabel.Text = (silicaTotal * 1000).ToString("F1");
        HumicAcidPpmLabel.Text = (humicAcidTotal * 1000).ToString("F1");
        FulvicAcidPpmLabel.Text = (fulvicAcidTotal * 1000).ToString("F1");
    }

    private async void UpdateUnitDisplay()
    {
        // Update the units type label
        UnitsTypeLabel.Text = useImperialUnits ? "Imperial Units (per gallon)" : "Metric Units (per liter)";
        
        // Update PPM header label
        PpmHeaderLabel.Text = useImperialUnits ? "PPM (per gallon)" : "PPM (per liter)";
        
        // Since UnitLabel is in a DataTemplate, we need to refresh the CollectionView
        // to update all instances
        MixListView.ItemsSource = null;
        MixListView.ItemsSource = currentMix;
        
        // Save the setting
        appSettings.UseImperialUnits = useImperialUnits;
        bool success = await fileService.SaveToXmlAsync(appSettings, "AppSettings.xml");
        if (!success)
        {
            await DisplayAlert("Error", "Failed to save settings", "OK");
        }
        
        // Force the collection view to refresh
        var temp = MixListView.ItemsSource;
        MixListView.ItemsSource = null;
        MixListView.ItemsSource = temp;
    }

    private void OnUnitsToggled(object sender, ToggledEventArgs e)
    {
        useImperialUnits = e.Value;
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
        if (MixPicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please select a mix to load", "OK");
            return;
        }
        
        // Convert the selected item to a string safely
        var selectedItem = MixPicker.SelectedItem;
        string selectedMixName = selectedItem?.ToString() ?? string.Empty;
        
        if (string.IsNullOrEmpty(selectedMixName))
        {
            await DisplayAlert("Error", "Invalid mix selection", "OK");
            return;
        }
        
        var mix = savedMixes.FirstOrDefault(m => m.Name == selectedMixName);
        
        if (mix != null)
        {
            // Ask if user wants to replace or add to current mix
            bool replaceCurrentMix = await DisplayAlert("Load Mix", 
                "Do you want to replace the current mix?", 
                "Replace", "Add to Current");
            
            if (replaceCurrentMix)
            {
                currentMix.Clear();
            }
            
            // Add each ingredient from the saved mix
            foreach (var ingredient in mix.Ingredients)
            {
                // Check if fertilizer exists
                if (availableFertilizers.Any(f => f.Name == ingredient.FertilizerName))
                {
                    // Check if it's already in the current mix when adding
                    var existingItem = currentMix.FirstOrDefault(i => i.FertilizerName == ingredient.FertilizerName);
                    
                    if (existingItem != null && !replaceCurrentMix)
                    {
                        // Add to existing quantity
                        existingItem.Quantity += ingredient.Quantity;
                    }
                    else
                    {
                        // Add as new ingredient
                        currentMix.Add(new FertilizerQuantity
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
            UpdateNutrientTotals();
        }
    }

    private async void OnClearMixClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm", 
            "Are you sure you want to clear the current mix?", 
            "Yes", "No");
        
        if (confirm)
        {
            currentMix.Clear();
            UpdateNutrientTotals();
        }
    }

    private async void OnCompareMixesClicked(object sender, EventArgs e)
    {
        if (currentMix.Count == 0)
        {
            await DisplayAlert("Error", "Current mix is empty. Add some fertilizers before comparing.", "OK");
            return;
        }
        
        await Navigation.PushAsync(new CompareMixPage(fileService, currentMix.ToList(), useImperialUnits));
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ImportOptionsPage(fileService, availableFertilizers, savedMixes));
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ExportOptionsPage(fileService, availableFertilizers.ToList(), savedMixes));
    }
}
