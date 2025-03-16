using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FertCalculatorMaui.Services;
using System.Diagnostics;

namespace FertCalculatorMaui;

public partial class CompareMixPage : ContentPage
{
    private readonly FileService _fileService;
    private ObservableCollection<Fertilizer> _cachedFertilizers;
    public CompareMixViewModel ViewModel { get; private set; }

    public CompareMixPage(FileService fileService, List<FertilizerQuantity> currentMixIngredients, bool useImperialUnits)
    {
        InitializeComponent();
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        
        // Create temporary mixes for comparison
        var currentMix = new FertilizerMix { 
            Name = "Current Mix", 
            Ingredients = currentMixIngredients 
        };
        
        // Initialize ViewModel
        ViewModel = new CompareMixViewModel
        {
            UseImperialUnits = useImperialUnits
        };
        
        // Set binding context
        BindingContext = ViewModel;
        
        // Subscribe to the CloseRequested event
        ViewModel.CloseRequested += OnCloseRequested;
        
        // Load fertilizers and mixes to populate available lists
        _ = LoadAndInitializeAsync(currentMix, useImperialUnits);
    }
    
    private async Task LoadAndInitializeAsync(FertilizerMix currentMix, bool useImperialUnits)
    {
        try
        {
            // Load available fertilizers
            var fertilizers = await _fileService.LoadFertilizersAsync();
            _cachedFertilizers = new ObservableCollection<Fertilizer>(fertilizers);
            
            // Load available mixes
            var availableMixes = await _fileService.LoadMixesAsync();
            
            if (_cachedFertilizers != null)
            {
                // Add the current mix to the available mixes for selection
                if (availableMixes == null)
                {
                    availableMixes = new ObservableCollection<FertilizerMix>();
                }
                
                // Check if current mix has ingredients before adding
                if (currentMix.Ingredients != null && currentMix.Ingredients.Count > 0)
                {
                    // Add current mix at the beginning of the list
                    availableMixes.Insert(0, currentMix);
                }
                
                // Set the available mixes in the view model
                ViewModel.AvailableMixes = availableMixes;
                
                // Set initial selections
                if (availableMixes.Count > 0)
                {
                    ViewModel.SelectedMix1 = availableMixes[0];
                    
                    // Set Mix2 to the second mix if available, otherwise use the first one
                    ViewModel.SelectedMix2 = availableMixes.Count > 1 ? availableMixes[1] : availableMixes[0];
                }
                
                // Update the comparison table
                UpdateComparisonTable();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in LoadAndInitializeAsync: {ex.Message}");
            await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
    }

    public CompareMixPage(FertilizerMix mix1, FertilizerMix mix2, List<Fertilizer> availableFertilizers, bool useImperialUnits)
    {
        InitializeComponent();
        
        // Cache the fertilizers
        _cachedFertilizers = new ObservableCollection<Fertilizer>(availableFertilizers);
        
        // Create a collection with just these two mixes
        var mixes = new ObservableCollection<FertilizerMix> { mix1, mix2 };
        
        // Initialize ViewModel
        ViewModel = new CompareMixViewModel
        {
            AvailableMixes = mixes,
            SelectedMix1 = mix1,
            SelectedMix2 = mix2,
            UseImperialUnits = useImperialUnits
        };
        
        // Set binding context
        BindingContext = ViewModel;

        // Subscribe to the CloseRequested event
        ViewModel.CloseRequested += OnCloseRequested;
    }

    // Add the UpdateComparisonTable method
    private void UpdateComparisonTable()
    {
        if (ViewModel.SelectedMix1 == null || ViewModel.SelectedMix2 == null)
            return;

        // Calculate nutrient totals for both mixes
        CalculateNutrientTotals(ViewModel.SelectedMix1, ViewModel.SelectedMix2);
    }

    // Calculate nutrient totals for both mixes
    private void CalculateNutrientTotals(FertilizerMix mix1, FertilizerMix mix2)
    {
        // Calculate totals for each mix
        if (mix1?.Ingredients != null)
        {
            // Calculate for mix1
            CalculateMixNutrients(mix1, true);
        }
        
        if (mix2?.Ingredients != null)
        {
            // Calculate for mix2
            CalculateMixNutrients(mix2, false);
        }
    }

    // Calculate nutrients for a specific mix
    private void CalculateMixNutrients(FertilizerMix mix, bool isFirstMix)
    {
        // Initialize nutrient totals
        Dictionary<string, double> nutrientTotals = new Dictionary<string, double>();
        
        // Process each ingredient in the mix
        foreach (var ingredient in mix.Ingredients)
        {
            // Find the fertilizer in the available fertilizers
            var fertilizer = FindFertilizer(ingredient.FertilizerName);
            if (fertilizer == null) continue;
            
            // Calculate contribution of each nutrient using the Fertilizer model's PPM methods
            Dictionary<string, double> nutrientValues = new Dictionary<string, double>
            {
                { "N", fertilizer.NitrogenPpm(ViewModel.UseImperialUnits) },
                { "P", fertilizer.PhosphorusPpm(ViewModel.UseImperialUnits) },
                { "K", fertilizer.PotassiumPpm(ViewModel.UseImperialUnits) },
                { "Ca", fertilizer.CalciumPpm(ViewModel.UseImperialUnits) },
                { "Mg", fertilizer.MagnesiumPpm(ViewModel.UseImperialUnits) },
                { "S", fertilizer.SulfurPpm(ViewModel.UseImperialUnits) },
                { "B", fertilizer.BoronPpm(ViewModel.UseImperialUnits) },
                { "Cu", fertilizer.CopperPpm(ViewModel.UseImperialUnits) },
                { "Fe", fertilizer.IronPpm(ViewModel.UseImperialUnits) },
                { "Mn", fertilizer.ManganesePpm(ViewModel.UseImperialUnits) },
                { "Mo", fertilizer.MolybdenumPpm(ViewModel.UseImperialUnits) },
                { "Zn", fertilizer.ZincPpm(ViewModel.UseImperialUnits) },
                { "Cl", fertilizer.ChlorinePpm(ViewModel.UseImperialUnits) },
                { "Si", fertilizer.SilicaPpm(ViewModel.UseImperialUnits) },
                { "Humic Acid", fertilizer.HumicAcidPpm(ViewModel.UseImperialUnits) },
                { "Fulvic Acid", fertilizer.FulvicAcidPpm(ViewModel.UseImperialUnits) }
            };
            
            foreach (var nutrient in nutrientValues)
            {
                double ppm = nutrient.Value * ingredient.Quantity;
                
                if (nutrientTotals.ContainsKey(nutrient.Key))
                    nutrientTotals[nutrient.Key] += ppm;
                else
                    nutrientTotals[nutrient.Key] = ppm;
            }
        }
        
        // Update the ViewModel with calculated values
        UpdateViewModelWithNutrients(nutrientTotals, isFirstMix);
    }
    
    // Handler for the CloseRequested event
    private async void OnCloseRequested(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // Find a fertilizer by name
    private Fertilizer FindFertilizer(string name)
    {
        // Look up the fertilizer by name from the cached list
        // This avoids the deadlock from using .Result on an async method
        return _cachedFertilizers?.FirstOrDefault(f => f.Name == name);
    }

    // Update the ViewModel with calculated nutrient values
    private void UpdateViewModelWithNutrients(Dictionary<string, double> nutrients, bool isFirstMix)
    {
        // This method would update the appropriate properties in the ViewModel
        // based on whether we're updating Mix1 or Mix2
        
        // For example:
        if (isFirstMix)
        {
            // Update Mix1 properties
            if (nutrients.TryGetValue("N", out double nitrogen))
                ViewModel.Mix1NitrogenPpm = nitrogen;
            
            if (nutrients.TryGetValue("P", out double phosphorus))
                ViewModel.Mix1PhosphorusPpm = phosphorus;
            
            if (nutrients.TryGetValue("K", out double potassium))
                ViewModel.Mix1PotassiumPpm = potassium;
            
            if (nutrients.TryGetValue("Ca", out double calcium))
                ViewModel.Mix1CalciumPpm = calcium;
            
            if (nutrients.TryGetValue("Mg", out double magnesium))
                ViewModel.Mix1MagnesiumPpm = magnesium;
            
            if (nutrients.TryGetValue("S", out double sulfur))
                ViewModel.Mix1SulfurPpm = sulfur;
            
            if (nutrients.TryGetValue("B", out double boron))
                ViewModel.Mix1BoronPpm = boron;
            
            if (nutrients.TryGetValue("Cu", out double copper))
                ViewModel.Mix1CopperPpm = copper;
            
            if (nutrients.TryGetValue("Fe", out double iron))
                ViewModel.Mix1IronPpm = iron;
            
            if (nutrients.TryGetValue("Mn", out double manganese))
                ViewModel.Mix1ManganesePpm = manganese;
            
            if (nutrients.TryGetValue("Mo", out double molybdenum))
                ViewModel.Mix1MolybdenumPpm = molybdenum;
            
            if (nutrients.TryGetValue("Zn", out double zinc))
                ViewModel.Mix1ZincPpm = zinc;
            
            if (nutrients.TryGetValue("Cl", out double chlorine))
                ViewModel.Mix1ChlorinePpm = chlorine;
            
            if (nutrients.TryGetValue("Si", out double silica))
                ViewModel.Mix1SilicaPpm = silica;
            
            if (nutrients.TryGetValue("Humic Acid", out double humicAcid))
                ViewModel.Mix1HumicAcidPpm = humicAcid;
            
            if (nutrients.TryGetValue("Fulvic Acid", out double fulvicAcid))
                ViewModel.Mix1FulvicAcidPpm = fulvicAcid;
            
            // Calculate the total nutrients PPM for Mix1
            ViewModel.Mix1TotalNutrientPpm = ViewModel.Mix1NitrogenPpm + ViewModel.Mix1PhosphorusPpm + 
                                           ViewModel.Mix1PotassiumPpm + ViewModel.Mix1CalciumPpm + 
                                           ViewModel.Mix1MagnesiumPpm + ViewModel.Mix1SulfurPpm + 
                                           ViewModel.Mix1BoronPpm + ViewModel.Mix1CopperPpm + 
                                           ViewModel.Mix1IronPpm + ViewModel.Mix1ManganesePpm + 
                                           ViewModel.Mix1MolybdenumPpm + ViewModel.Mix1ZincPpm + 
                                           ViewModel.Mix1ChlorinePpm + ViewModel.Mix1SilicaPpm + 
                                           ViewModel.Mix1HumicAcidPpm + ViewModel.Mix1FulvicAcidPpm;
        }
        else
        {
            // Update Mix2 properties
            if (nutrients.TryGetValue("N", out double nitrogen))
                ViewModel.Mix2NitrogenPpm = nitrogen;
            
            if (nutrients.TryGetValue("P", out double phosphorus))
                ViewModel.Mix2PhosphorusPpm = phosphorus;
            
            if (nutrients.TryGetValue("K", out double potassium))
                ViewModel.Mix2PotassiumPpm = potassium;
            
            if (nutrients.TryGetValue("Ca", out double calcium))
                ViewModel.Mix2CalciumPpm = calcium;
            
            if (nutrients.TryGetValue("Mg", out double magnesium))
                ViewModel.Mix2MagnesiumPpm = magnesium;
            
            if (nutrients.TryGetValue("S", out double sulfur))
                ViewModel.Mix2SulfurPpm = sulfur;
            
            if (nutrients.TryGetValue("B", out double boron))
                ViewModel.Mix2BoronPpm = boron;
            
            if (nutrients.TryGetValue("Cu", out double copper))
                ViewModel.Mix2CopperPpm = copper;
            
            if (nutrients.TryGetValue("Fe", out double iron))
                ViewModel.Mix2IronPpm = iron;
            
            if (nutrients.TryGetValue("Mn", out double manganese))
                ViewModel.Mix2ManganesePpm = manganese;
            
            if (nutrients.TryGetValue("Mo", out double molybdenum))
                ViewModel.Mix2MolybdenumPpm = molybdenum;
            
            if (nutrients.TryGetValue("Zn", out double zinc))
                ViewModel.Mix2ZincPpm = zinc;
            
            if (nutrients.TryGetValue("Cl", out double chlorine))
                ViewModel.Mix2ChlorinePpm = chlorine;
            
            if (nutrients.TryGetValue("Si", out double silica))
                ViewModel.Mix2SilicaPpm = silica;
            
            if (nutrients.TryGetValue("Humic Acid", out double humicAcid))
                ViewModel.Mix2HumicAcidPpm = humicAcid;
            
            if (nutrients.TryGetValue("Fulvic Acid", out double fulvicAcid))
                ViewModel.Mix2FulvicAcidPpm = fulvicAcid;
            
            // Calculate the total nutrients PPM for Mix2
            ViewModel.Mix2TotalNutrientPpm = ViewModel.Mix2NitrogenPpm + ViewModel.Mix2PhosphorusPpm + 
                                           ViewModel.Mix2PotassiumPpm + ViewModel.Mix2CalciumPpm + 
                                           ViewModel.Mix2MagnesiumPpm + ViewModel.Mix2SulfurPpm + 
                                           ViewModel.Mix2BoronPpm + ViewModel.Mix2CopperPpm + 
                                           ViewModel.Mix2IronPpm + ViewModel.Mix2ManganesePpm + 
                                           ViewModel.Mix2MolybdenumPpm + ViewModel.Mix2ZincPpm + 
                                           ViewModel.Mix2ChlorinePpm + ViewModel.Mix2SilicaPpm + 
                                           ViewModel.Mix2HumicAcidPpm + ViewModel.Mix2FulvicAcidPpm;
        }
    }
}
