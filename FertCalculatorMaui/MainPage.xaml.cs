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
    private const double GALLON_TO_LITER = 3.78541;
    private string fertilizerDbPath = "Fertilizers.xml";
    private string mixesDbPath = "Mixes.xml";
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
        // Initialize all nutrient totals to 0
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
        
        foreach (var item in currentMix)
        {
            // Find the fertilizer in the available list
            var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == item.FertilizerName);
            if (fertilizer != null)
            {
                double quantity = item.Quantity;
                
                // Calculate PPM based on nutrient percentages
                nitrogenTotal += fertilizer.CalculateNitrogenPpm(useImperialUnits) * quantity / 1000;
                phosphorusTotal += fertilizer.CalculatePhosphorusPpm(useImperialUnits) * quantity / 1000;
                potassiumTotal += fertilizer.CalculatePotassiumPpm(useImperialUnits) * quantity / 1000;
                calciumTotal += fertilizer.CalculateCalciumPpm(useImperialUnits) * quantity / 1000;
                magnesiumTotal += fertilizer.CalculateMagnesiumPpm(useImperialUnits) * quantity / 1000;
                sulfurTotal += fertilizer.CalculateSulfurPpm(useImperialUnits) * quantity / 1000;
                boronTotal += fertilizer.CalculateBoronPpm(useImperialUnits) * quantity / 1000;
                copperTotal += fertilizer.CalculateCopperPpm(useImperialUnits) * quantity / 1000;
                ironTotal += fertilizer.CalculateIronPpm(useImperialUnits) * quantity / 1000;
                manganeseTotal += fertilizer.CalculateManganesePpm(useImperialUnits) * quantity / 1000;
                molybdenumTotal += fertilizer.CalculateMolybdenumPpm(useImperialUnits) * quantity / 1000;
                zincTotal += fertilizer.CalculateZincPpm(useImperialUnits) * quantity / 1000;
                chlorineTotal += fertilizer.CalculateChlorinePpm(useImperialUnits) * quantity / 1000;
                silicaTotal += fertilizer.CalculateSilicaPpm(useImperialUnits) * quantity / 1000;
                humicAcidTotal += fertilizer.CalculateHumicAcidPpm(useImperialUnits) * quantity / 1000;
                fulvicAcidTotal += fertilizer.CalculateFulvicAcidPpm(useImperialUnits) * quantity / 1000;
            }
        }
        
        // Update PPM header based on units
        PpmHeaderLabel.Text = useImperialUnits ? "PPM (per gal)" : "PPM (per L)";
        
        // Update UI with totals (percentages)
        NitrogenPercentLabel.Text = (nitrogenTotal * 100).ToString("F2");
        PhosphorusPercentLabel.Text = (phosphorusTotal * 100).ToString("F2");
        PotassiumPercentLabel.Text = (potassiumTotal * 100).ToString("F2");
        CalciumPercentLabel.Text = (calciumTotal * 100).ToString("F2");
        MagnesiumPercentLabel.Text = (magnesiumTotal * 100).ToString("F2");
        SulfurPercentLabel.Text = (sulfurTotal * 100).ToString("F2");
        BoronPercentLabel.Text = (boronTotal * 100).ToString("F2");
        CopperPercentLabel.Text = (copperTotal * 100).ToString("F2");
        IronPercentLabel.Text = (ironTotal * 100).ToString("F2");
        ManganesePercentLabel.Text = (manganeseTotal * 100).ToString("F2");
        MolybdenumPercentLabel.Text = (molybdenumTotal * 100).ToString("F2");
        ZincPercentLabel.Text = (zincTotal * 100).ToString("F2");
        ChlorinePercentLabel.Text = (chlorineTotal * 100).ToString("F2");
        SilicaPercentLabel.Text = (silicaTotal * 100).ToString("F2");
        HumicAcidPercentLabel.Text = (humicAcidTotal * 100).ToString("F2");
        FulvicAcidPercentLabel.Text = (fulvicAcidTotal * 100).ToString("F2");
        
        // Update UI with totals (PPM)
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

    private void UpdateUnitDisplay()
    {
        // Update the unit label for the mix entries
        UnitLabel.Text = useImperialUnits ? "g/gal" : "g/L";
        
        // Update the units type label
        UnitsTypeLabel.Text = useImperialUnits ? "Imperial Units (per gallon)" : "Metric Units (per liter)";
        
        // Save the setting
        appSettings.UseImperialUnits = useImperialUnits;
        _ = fileService.SaveToXmlAsync(appSettings, "AppSettings.xml");
        
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
    
    private async void OnCompareMixesClicked(object sender, EventArgs e)
    {
        if (currentMix.Count == 0)
        {
            await DisplayAlert("No Mix", "Create a mix first before comparing", "OK");
            return;
        }
        
        // Navigate to compare mix page with the current mix
        await Navigation.PushAsync(new CompareMixPage(fileService, currentMix.ToList(), useImperialUnits));
    }
    
    private async void OnImportClicked(object sender, EventArgs e)
    {
        // Navigate to import options page
        await Navigation.PushAsync(new ImportOptionsPage(fileService));
    }
    
    private async void OnExportClicked(object sender, EventArgs e)
    {
        // Check if there are fertilizers or mixes to export
        if (availableFertilizers.Count == 0 && savedMixes.Count == 0)
        {
            await DisplayAlert("Nothing to Export", "There are no fertilizers or mixes to export", "OK");
            return;
        }
        
        // Navigate to export options page
        await Navigation.PushAsync(new ExportOptionsPage(fileService, availableFertilizers.ToList(), savedMixes.ToList()));
    }
}
