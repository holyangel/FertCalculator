using System.Collections.Generic;
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
            MixListView.ItemsSource = currentMix;
            Debug.WriteLine("Data bindings set up");
            
            // Set up unit display
            UpdateUnitDisplay();
            Debug.WriteLine("MainPage constructor completed");
            
            // Subscribe to messages from ManageFertilizersPage
            MessagingCenter.Subscribe<ManageFertilizersPage, Fertilizer>(this, "AddFertilizerToMix", (sender, fertilizer) => {
                AddFertilizerToMix(fertilizer);
            });
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
    
    private async void OnEditFertilizerClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string fertilizerName)
        {
            var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
            if (fertilizer != null)
            {
                await Navigation.PushAsync(new AddFertilizerPage(fileService, fertilizer));
            }
        }
    }
    
    private void OnRemoveFromMixClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string fertilizerName)
        {
            var itemToRemove = currentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
            if (itemToRemove != null)
            {
                currentMix.Remove(itemToRemove);
                UpdateNutrientTotals();
            }
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
        double totalNutrientTotal = 0;
        
        double totalGrams = 0;
        
        // Calculate percentage contributions (for display)
        double nitrogenPercent = 0;
        double phosphorusPercent = 0;
        double potassiumPercent = 0;
        double calciumPercent = 0;
        double magnesiumPercent = 0;
        double sulfurPercent = 0;
        double boronPercent = 0;
        double copperPercent = 0;
        double ironPercent = 0;
        double manganesePercent = 0;
        double molybdenumPercent = 0;
        double zincPercent = 0;
        double chlorinePercent = 0;
        double silicaPercent = 0;
        double humicAcidPercent = 0;
        double fulvicAcidPercent = 0;
        double totalNutrientPercent = 0;
        
        // Calculate for each fertilizer in the mix
        foreach (var item in currentMix)
        {
            var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == item.FertilizerName);
            if (fertilizer == null) continue;
            
            double quantity = item.Quantity;
            totalGrams += quantity;
            
            // Calculate weighted contribution for percentage display
            nitrogenPercent += (fertilizer.NitrogenPercent / 100) * quantity;
            phosphorusPercent += (fertilizer.PhosphorusPercent / 100) * quantity;
            potassiumPercent += (fertilizer.PotassiumPercent / 100) * quantity;
            calciumPercent += (fertilizer.CalciumPercent / 100) * quantity;
            magnesiumPercent += (fertilizer.MagnesiumPercent / 100) * quantity;
            sulfurPercent += (fertilizer.SulfurPercent / 100) * quantity;
            boronPercent += (fertilizer.BoronPercent / 100) * quantity;
            copperPercent += (fertilizer.CopperPercent / 100) * quantity;
            ironPercent += (fertilizer.IronPercent / 100) * quantity;
            manganesePercent += (fertilizer.ManganesePercent / 100) * quantity;
            molybdenumPercent += (fertilizer.MolybdenumPercent / 100) * quantity;
            zincPercent += (fertilizer.ZincPercent / 100) * quantity;
            chlorinePercent += (fertilizer.ChlorinePercent / 100) * quantity;
            silicaPercent += (fertilizer.SilicaPercent / 100) * quantity;
            humicAcidPercent += (fertilizer.HumicAcidPercent / 100) * quantity;
            fulvicAcidPercent += (fertilizer.FulvicAcidPercent / 100) * quantity;
            totalNutrientPercent += (fertilizer.TotalNutrientPercent / 100) * quantity;
            
            // Calculate PPM directly from fertilizer PPM values
            nitrogenTotal += fertilizer.NitrogenPpm(useImperialUnits) * quantity;
            phosphorusTotal += fertilizer.PhosphorusPpm(useImperialUnits) * quantity;
            potassiumTotal += fertilizer.PotassiumPpm(useImperialUnits) * quantity;
            calciumTotal += fertilizer.CalciumPpm(useImperialUnits) * quantity;
            magnesiumTotal += fertilizer.MagnesiumPpm(useImperialUnits) * quantity;
            sulfurTotal += fertilizer.SulfurPpm(useImperialUnits) * quantity;
            boronTotal += fertilizer.BoronPpm(useImperialUnits) * quantity;
            copperTotal += fertilizer.CopperPpm(useImperialUnits) * quantity;
            ironTotal += fertilizer.IronPpm(useImperialUnits) * quantity;
            manganeseTotal += fertilizer.ManganesePpm(useImperialUnits) * quantity;
            molybdenumTotal += fertilizer.MolybdenumPpm(useImperialUnits) * quantity;
            zincTotal += fertilizer.ZincPpm(useImperialUnits) * quantity;
            chlorineTotal += fertilizer.ChlorinePpm(useImperialUnits) * quantity;
            silicaTotal += fertilizer.SilicaPpm(useImperialUnits) * quantity;
            humicAcidTotal += fertilizer.HumicAcidPpm(useImperialUnits) * quantity;
            fulvicAcidTotal += fertilizer.FulvicAcidPpm(useImperialUnits) * quantity;
            totalNutrientTotal += fertilizer.TotalNutrientPpm(useImperialUnits) * quantity;
        }
        
        // Convert percentages to actual percentages if there are any fertilizers in the mix
        if (totalGrams > 0)
        {
            nitrogenPercent = (nitrogenPercent / totalGrams) * 100;
            phosphorusPercent = (phosphorusPercent / totalGrams) * 100;
            potassiumPercent = (potassiumPercent / totalGrams) * 100;
            calciumPercent = (calciumPercent / totalGrams) * 100;
            magnesiumPercent = (magnesiumPercent / totalGrams) * 100;
            sulfurPercent = (sulfurPercent / totalGrams) * 100;
            boronPercent = (boronPercent / totalGrams) * 100;
            copperPercent = (copperPercent / totalGrams) * 100;
            ironPercent = (ironPercent / totalGrams) * 100;
            manganesePercent = (manganesePercent / totalGrams) * 100;
            molybdenumPercent = (molybdenumPercent / totalGrams) * 100;
            zincPercent = (zincPercent / totalGrams) * 100;
            chlorinePercent = (chlorinePercent / totalGrams) * 100;
            silicaPercent = (silicaPercent / totalGrams) * 100;
            humicAcidPercent = (humicAcidPercent / totalGrams) * 100;
            fulvicAcidPercent = (fulvicAcidPercent / totalGrams) * 100;
            totalNutrientPercent = (totalNutrientPercent / totalGrams) * 100;
        }
        
        // No need for a conversion factor here - already applied in the PPM methods
        
        // Update the UI for percentages
        NitrogenPercentLabel.Text = nitrogenPercent.ToString("F2");
        PhosphorusPercentLabel.Text = phosphorusPercent.ToString("F2");
        PotassiumPercentLabel.Text = potassiumPercent.ToString("F2");
        CalciumPercentLabel.Text = calciumPercent.ToString("F2");
        MagnesiumPercentLabel.Text = magnesiumPercent.ToString("F2");
        SulfurPercentLabel.Text = sulfurPercent.ToString("F2");
        BoronPercentLabel.Text = boronPercent.ToString("F2");
        CopperPercentLabel.Text = copperPercent.ToString("F2");
        IronPercentLabel.Text = ironPercent.ToString("F2");
        ManganesePercentLabel.Text = manganesePercent.ToString("F2");
        MolybdenumPercentLabel.Text = molybdenumPercent.ToString("F2");
        ZincPercentLabel.Text = zincPercent.ToString("F2");
        ChlorinePercentLabel.Text = chlorinePercent.ToString("F2");
        SilicaPercentLabel.Text = silicaPercent.ToString("F2");
        HumicAcidPercentLabel.Text = humicAcidPercent.ToString("F2");
        FulvicAcidPercentLabel.Text = fulvicAcidPercent.ToString("F2");
        TotalNutrientPercentLabel.Text = (nitrogenPercent + phosphorusPercent + potassiumPercent + calciumPercent + magnesiumPercent + sulfurPercent + boronPercent + copperPercent + ironPercent + manganesePercent + molybdenumPercent + zincPercent + chlorinePercent + silicaPercent + humicAcidPercent + fulvicAcidPercent).ToString("F2");
        
        // Update the UI for PPM - no need to apply conversionFactor since it's already done in the PPM methods
        NitrogenPpmLabel.Text = nitrogenTotal.ToString("F2");
        PhosphorusPpmLabel.Text = phosphorusTotal.ToString("F2");
        PotassiumPpmLabel.Text = potassiumTotal.ToString("F2");
        CalciumPpmLabel.Text = calciumTotal.ToString("F2");
        MagnesiumPpmLabel.Text = magnesiumTotal.ToString("F2");
        SulfurPpmLabel.Text = sulfurTotal.ToString("F2");
        BoronPpmLabel.Text = boronTotal.ToString("F2");
        CopperPpmLabel.Text = copperTotal.ToString("F2");
        IronPpmLabel.Text = ironTotal.ToString("F2");
        ManganesePpmLabel.Text = manganeseTotal.ToString("F2");
        MolybdenumPpmLabel.Text = molybdenumTotal.ToString("F2");
        ZincPpmLabel.Text = zincTotal.ToString("F2");
        ChlorinePpmLabel.Text = chlorineTotal.ToString("F2");
        SilicaPpmLabel.Text = silicaTotal.ToString("F2");
        HumicAcidPpmLabel.Text = humicAcidTotal.ToString("F2");
        FulvicAcidPpmLabel.Text = fulvicAcidTotal.ToString("F2");
        TotalNutrientPpmLabel.Text = (nitrogenTotal + phosphorusTotal + potassiumTotal + calciumTotal + magnesiumTotal + sulfurTotal + boronTotal + copperTotal + ironTotal + manganeseTotal + molybdenumTotal + zincTotal + chlorineTotal + silicaTotal + humicAcidTotal + fulvicAcidTotal).ToString("F2");
    }

    private async void UpdateUnitDisplay()
    {
        // Update the units type label
        UnitsTypeLabel.Text = useImperialUnits ? "Imperial (per gallon)" : "Metric (per liter)";
        
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

    private async void SaveFertilizers()
    {
        await fileService.SaveFertilizersAsync(availableFertilizers.ToList());
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

    private async void OnManageFertilizersClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ManageFertilizersPage(fileService, availableFertilizers));
    }
    
    private void OnIncrementSmallClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string fertilizerName)
        {
            var mixItem = currentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
            
            if (mixItem != null)
            {
                mixItem.Quantity += 0.1; // Increment by 0.1 gram
                mixItem.Quantity = Math.Round(mixItem.Quantity, 1); // Round to 1 decimal place for precision
                UpdateNutrientTotals();
            }
        }
    }
    
    private void OnDecrementSmallClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string fertilizerName)
        {
            var mixItem = currentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
            
            if (mixItem != null && mixItem.Quantity >= 0.1)
            {
                mixItem.Quantity -= 0.1; // Decrement by 0.1 gram
                mixItem.Quantity = Math.Round(mixItem.Quantity, 1); // Round to 1 decimal place for precision
                UpdateNutrientTotals();
            }
        }
    }
    
    private void OnIncrementGramClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string fertilizerName)
        {
            var mixItem = currentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
            
            if (mixItem != null)
            {
                mixItem.Quantity += 1.0; // Increment by 1 gram
                UpdateNutrientTotals();
            }
        }
    }
    
    private void OnDecrementGramClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string fertilizerName)
        {
            var mixItem = currentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
            
            if (mixItem != null && mixItem.Quantity >= 1.0)
            {
                mixItem.Quantity -= 1.0; // Decrement by 1 gram, but don't go below 0
                UpdateNutrientTotals();
            }
        }
    }

    private async void OnFertilizerTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string fertilizerName)
        {
            var mixItem = currentMix.FirstOrDefault(item => item.FertilizerName == fertilizerName);
            
            if (mixItem != null)
            {
                var editPage = new EditQuantityPage(fertilizerName, mixItem.Quantity, useImperialUnits);
                
                // Subscribe to the QuantityChanged event
                editPage.QuantityChanged += (s, args) =>
                {
                    // Update the quantity in the current mix
                    mixItem.Quantity = args.Quantity;
                    UpdateNutrientTotals();
                };
                
                await Navigation.PushAsync(editPage);
            }
        }
    }

    private void OnToggleMixVisibilityClicked(object sender, EventArgs e)
    {
        // Toggle visibility of mix content
        bool isVisible = MixButtonsLayout.IsVisible;
        
        // Toggle visibility
        MixButtonsLayout.IsVisible = !isVisible;
        MixListView.IsVisible = !isVisible;
        
        // Update button text
        ToggleMixVisibilityButton.Text = isVisible ? "▲" : "▼";
    }

    private void AddFertilizerToMix(Fertilizer fertilizer)
    {
        // Check if the fertilizer is already in the mix
        var existingItem = currentMix.FirstOrDefault(item => item.FertilizerName == fertilizer.Name);
        
        if (existingItem == null)
        {
            // Add new fertilizer to the mix with default quantity of 1.0
            currentMix.Add(new FertilizerQuantity
            {
                FertilizerName = fertilizer.Name,
                Quantity = 1.0
            });
            
            // Update nutrient totals
            UpdateNutrientTotals();
        }
        else
        {
            // Increment the quantity if it already exists
            existingItem.Quantity += 1.0;
            UpdateNutrientTotals();
        }
    }
}
