using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using FertCalculatorMaui.Services;
using System.Diagnostics;

namespace FertCalculatorMaui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly FileService fileService;
    private readonly IDialogService dialogService;
    private const double GALLON_TO_LITER = 3.78541;

    [ObservableProperty]
    private ObservableCollection<Fertilizer> availableFertilizers = new();

    [ObservableProperty]
    private ObservableCollection<FertilizerMix> savedMixes = new();

    [ObservableProperty]
    private ObservableCollection<FertilizerQuantity> currentMix = new();

    [ObservableProperty]
    private bool useImperialUnits;

    [ObservableProperty]
    private string unitsTypeLabel = "g or ml (per liter)";

    [ObservableProperty]
    private string ppmHeaderLabel = "PPM (per liter)";

    // Current mix visibility
    [ObservableProperty]
    private bool isMixVisible = true;

    // Nutrient percentage properties
    [ObservableProperty]
    private double nitrogenPercent;

    [ObservableProperty]
    private double phosphorusPercent;

    [ObservableProperty]
    private double potassiumPercent;

    [ObservableProperty]
    private double calciumPercent;

    [ObservableProperty]
    private double magnesiumPercent;

    [ObservableProperty]
    private double sulfurPercent;

    [ObservableProperty]
    private double boronPercent;

    [ObservableProperty]
    private double copperPercent;

    [ObservableProperty]
    private double ironPercent;

    [ObservableProperty]
    private double manganesePercent;

    [ObservableProperty]
    private double molybdenumPercent;

    [ObservableProperty]
    private double zincPercent;

    [ObservableProperty]
    private double chlorinePercent;

    [ObservableProperty]
    private double silicaPercent;

    [ObservableProperty]
    private double humicAcidPercent;

    [ObservableProperty]
    private double fulvicAcidPercent;

    [ObservableProperty]
    private double totalNutrientPercent;

    // Nutrient PPM properties
    [ObservableProperty]
    private double nitrogenPpm;

    [ObservableProperty]
    private double phosphorusPpm;

    [ObservableProperty]
    private double potassiumPpm;

    [ObservableProperty]
    private double calciumPpm;

    [ObservableProperty]
    private double magnesiumPpm;

    [ObservableProperty]
    private double sulfurPpm;

    [ObservableProperty]
    private double boronPpm;

    [ObservableProperty]
    private double copperPpm;

    [ObservableProperty]
    private double ironPpm;

    [ObservableProperty]
    private double manganesePpm;

    [ObservableProperty]
    private double molybdenumPpm;

    [ObservableProperty]
    private double zincPpm;

    [ObservableProperty]
    private double chlorinePpm;

    [ObservableProperty]
    private double silicaPpm;

    [ObservableProperty]
    private double humicAcidPpm;

    [ObservableProperty]
    private double fulvicAcidPpm;

    [ObservableProperty]
    private double totalNutrientPpm;

    public MainViewModel(FileService fileService, IDialogService dialogService)
    {
        this.fileService = fileService;
        this.dialogService = dialogService;

        // Load settings
        var appSettings = new AppSettings();
        UseImperialUnits = appSettings.UseImperialUnits;
        
        // Initialize unit labels
        UpdateUnitDisplay();
        
        // Register for messages
        WeakReferenceMessenger.Default.Register<AddFertilizerToMixMessage>(this, (r, message) => {
            AddFertilizerToMix(message.Value);
        });
    }

    [RelayCommand]
    private async Task AddFertilizer()
    {
        try
        {
            await Shell.Current.Navigation.PushAsync(new ManageFertilizersPage(fileService, dialogService, AvailableFertilizers));
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", $"Failed to open Manage Fertilizers page: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task LoadMix()
    {
        try
        {
            var mix = await dialogService.DisplayActionSheetAsync("Load Mix", "Cancel", null, SavedMixes.Select(m => m.Name).ToArray());
            if (mix != null && mix != "Cancel")
            {
                var selectedMix = SavedMixes.FirstOrDefault(m => m.Name == mix);
                if (selectedMix != null)
                {
                    CurrentMix.Clear();
                    foreach (var item in selectedMix.Ingredients)
                    {
                        CurrentMix.Add(item);
                    }
                    UpdateNutrientTotals();
                }
            }
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", $"Failed to load mix: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveMix()
    {
        try
        {
            await Shell.Current.Navigation.PushAsync(new SaveMixPage(fileService, CurrentMix.ToList()));
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", $"Failed to open Save Mix page: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ClearMix()
    {
        try
        {
            bool confirm = await dialogService.DisplayConfirmationAsync("Confirm Clear", 
                "Are you sure you want to clear the current mix?", "Yes", "No");

            if (confirm)
            {
                CurrentMix.Clear();
                UpdateNutrientTotals();
            }
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", $"Failed to clear mix: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CompareMix()
    {
        try
        {
            Debug.WriteLine("CompareMix command executed");
            
            // Get the current navigation
            var navigation = GetCurrentNavigation();
            if (navigation == null)
            {
                Debug.WriteLine("Navigation service is not available");
                await dialogService.DisplayAlertAsync("Error", "Navigation service is not available", "OK");
                return;
            }

            // Get the service provider from the current application
            var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;
            if (serviceProvider == null)
            {
                Debug.WriteLine("Service provider is not available");
                await dialogService.DisplayAlertAsync("Error", "Service provider is not available", "OK");
                return;
            }

            try
            {
                // Get CompareMixPage from DI
                var compareMixPage = serviceProvider.GetService<CompareMixPage>();
                if (compareMixPage == null)
                {
                    Debug.WriteLine("Failed to create CompareMixPage from DI");
                    await dialogService.DisplayAlertAsync("Error", "Failed to create Compare Mix page", "OK");
                    return;
                }

                Debug.WriteLine("Successfully created CompareMixPage from DI");
                
                // Initialize the page with current mix data
                await compareMixPage.InitializeWithCurrentMix(CurrentMix.ToList(), UseImperialUnits);
                
                // Navigate to the page
                await navigation.PushAsync(compareMixPage);
                Debug.WriteLine("Successfully navigated to CompareMixPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing or navigating to CompareMixPage: {ex.Message}");
                await dialogService.DisplayAlertAsync("Error", $"Failed to initialize Compare Mix page: {ex.Message}", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in CompareMix command: {ex.Message}");
            await dialogService.DisplayAlertAsync("Error", $"Failed to open Compare Mix page: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RemoveFertilizer(string fertilizerName)
    {
        try
        {
            var item = CurrentMix.FirstOrDefault(x => x.FertilizerName == fertilizerName);
            if (item != null)
            {
                CurrentMix.Remove(item);
                UpdateNutrientTotals();
            }
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", $"Failed to remove fertilizer: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void ToggleUnits()
    {
        UseImperialUnits = !UseImperialUnits;
        UpdateUnitDisplay();
        UpdateNutrientTotals();

        // Save the setting
        var appSettings = new AppSettings();
        appSettings.UseImperialUnits = UseImperialUnits;
    }

    [RelayCommand]
    private async Task ManageFertilizers()
    {
        try
        {
            await Shell.Current.Navigation.PushAsync(new ManageFertilizersPage(
                fileService, 
                dialogService, 
                AvailableFertilizers));
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", $"Failed to open Manage Fertilizers page: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void ToggleMixVisibility()
    {
        try
        {
            IsMixVisible = !IsMixVisible;
            OnPropertyChanged(nameof(IsMixVisible));
        }
        catch (Exception ex)
        {
            dialogService.DisplayAlert("Error", $"Failed to toggle mix visibility: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        try
        {
            await Shell.Current.Navigation.PushAsync(new ImportOptionsPage(
                fileService,
                AvailableFertilizers,
                SavedMixes));
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", $"Failed to open Import page: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        try
        {
            await Shell.Current.Navigation.PushAsync(new ExportOptionsPage(
                fileService,
                AvailableFertilizers.ToList(),
                SavedMixes));
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", $"Failed to open Export page: {ex.Message}", "OK");
        }
    }

    public void UpdateUnitDisplay()
    {
        UnitsTypeLabel = UseImperialUnits ? "g or ml (per gallon)" : "g or ml (per liter)";
        PpmHeaderLabel = UseImperialUnits ? "PPM (per gallon)" : "PPM (per liter)";
    }

    public void UpdateNutrientTotals()
    {
        try
        {
            // Reset all totals
            ResetNutrientTotals();

            double totalQuantity = 0;
            
            // Calculate the contribution of each fertilizer to the total
            foreach (var item in CurrentMix)
            {
                var fertilizer = AvailableFertilizers.FirstOrDefault(f => f.Name == item.FertilizerName);
                if (fertilizer != null)
                {
                    double quantity = item.Quantity;
                    totalQuantity += quantity;
                    
                    // Calculate weighted contribution for percentage display
                    NitrogenPercent += (fertilizer.NitrogenPercent / 100) * quantity;
                    PhosphorusPercent += (fertilizer.PhosphorusPercent / 100) * quantity;
                    PotassiumPercent += (fertilizer.PotassiumPercent / 100) * quantity;
                    CalciumPercent += (fertilizer.CalciumPercent / 100) * quantity;
                    MagnesiumPercent += (fertilizer.MagnesiumPercent / 100) * quantity;
                    SulfurPercent += (fertilizer.SulfurPercent / 100) * quantity;
                    BoronPercent += (fertilizer.BoronPercent / 100) * quantity;
                    CopperPercent += (fertilizer.CopperPercent / 100) * quantity;
                    IronPercent += (fertilizer.IronPercent / 100) * quantity;
                    ManganesePercent += (fertilizer.ManganesePercent / 100) * quantity;
                    MolybdenumPercent += (fertilizer.MolybdenumPercent / 100) * quantity;
                    ZincPercent += (fertilizer.ZincPercent / 100) * quantity;
                    ChlorinePercent += (fertilizer.ChlorinePercent / 100) * quantity;
                    SilicaPercent += (fertilizer.SilicaPercent / 100) * quantity;
                    HumicAcidPercent += (fertilizer.HumicAcidPercent / 100) * quantity;
                    FulvicAcidPercent += (fertilizer.FulvicAcidPercent / 100) * quantity;
                    
                    // Calculate PPM directly from fertilizer PPM values
                    NitrogenPpm += fertilizer.NitrogenPpm(UseImperialUnits) * quantity;
                    PhosphorusPpm += fertilizer.PhosphorusPpm(UseImperialUnits) * quantity;
                    PotassiumPpm += fertilizer.PotassiumPpm(UseImperialUnits) * quantity;
                    CalciumPpm += fertilizer.CalciumPpm(UseImperialUnits) * quantity;
                    MagnesiumPpm += fertilizer.MagnesiumPpm(UseImperialUnits) * quantity;
                    SulfurPpm += fertilizer.SulfurPpm(UseImperialUnits) * quantity;
                    BoronPpm += fertilizer.BoronPpm(UseImperialUnits) * quantity;
                    CopperPpm += fertilizer.CopperPpm(UseImperialUnits) * quantity;
                    IronPpm += fertilizer.IronPpm(UseImperialUnits) * quantity;
                    ManganesePpm += fertilizer.ManganesePpm(UseImperialUnits) * quantity;
                    MolybdenumPpm += fertilizer.MolybdenumPpm(UseImperialUnits) * quantity;
                    ZincPpm += fertilizer.ZincPpm(UseImperialUnits) * quantity;
                    ChlorinePpm += fertilizer.ChlorinePpm(UseImperialUnits) * quantity;
                    SilicaPpm += fertilizer.SilicaPpm(UseImperialUnits) * quantity;
                    HumicAcidPpm += fertilizer.HumicAcidPpm(UseImperialUnits) * quantity;
                    FulvicAcidPpm += fertilizer.FulvicAcidPpm(UseImperialUnits) * quantity;
                }
            }
            
            // Normalize by total quantity
            if (totalQuantity > 0)
            {
                NitrogenPercent = (NitrogenPercent / totalQuantity) * 100;
                PhosphorusPercent = (PhosphorusPercent / totalQuantity) * 100;
                PotassiumPercent = (PotassiumPercent / totalQuantity) * 100;
                CalciumPercent = (CalciumPercent / totalQuantity) * 100;
                MagnesiumPercent = (MagnesiumPercent / totalQuantity) * 100;
                SulfurPercent = (SulfurPercent / totalQuantity) * 100;
                BoronPercent = (BoronPercent / totalQuantity) * 100;
                CopperPercent = (CopperPercent / totalQuantity) * 100;
                IronPercent = (IronPercent / totalQuantity) * 100;
                ManganesePercent = (ManganesePercent / totalQuantity) * 100;
                MolybdenumPercent = (MolybdenumPercent / totalQuantity) * 100;
                ZincPercent = (ZincPercent / totalQuantity) * 100;
                ChlorinePercent = (ChlorinePercent / totalQuantity) * 100;
                SilicaPercent = (SilicaPercent / totalQuantity) * 100;
                HumicAcidPercent = (HumicAcidPercent / totalQuantity) * 100;
                FulvicAcidPercent = (FulvicAcidPercent / totalQuantity) * 100;
            }
            
            // Calculate total nutrient percentage
            TotalNutrientPercent = NitrogenPercent + PhosphorusPercent + PotassiumPercent +
                                  CalciumPercent + MagnesiumPercent + SulfurPercent +
                                  BoronPercent + CopperPercent + IronPercent +
                                  ManganesePercent + MolybdenumPercent + ZincPercent +
                                  ChlorinePercent + SilicaPercent + HumicAcidPercent + FulvicAcidPercent;
            
            // Calculate total PPM
            TotalNutrientPpm = NitrogenPpm + PhosphorusPpm + PotassiumPpm +
                              CalciumPpm + MagnesiumPpm + SulfurPpm +
                              BoronPpm + CopperPpm + IronPpm +
                              ManganesePpm + MolybdenumPpm + ZincPpm +
                              ChlorinePpm + SilicaPpm + HumicAcidPpm + FulvicAcidPpm;
        }
        catch (Exception ex)
        {
            dialogService.DisplayAlertAsync("Error", $"Failed to update nutrient totals: {ex.Message}", "OK");
        }
    }

    private void ResetNutrientTotals()
    {
        // Reset percentages
        NitrogenPercent = 0;
        PhosphorusPercent = 0;
        PotassiumPercent = 0;
        CalciumPercent = 0;
        MagnesiumPercent = 0;
        SulfurPercent = 0;
        BoronPercent = 0;
        CopperPercent = 0;
        IronPercent = 0;
        ManganesePercent = 0;
        MolybdenumPercent = 0;
        ZincPercent = 0;
        ChlorinePercent = 0;
        SilicaPercent = 0;
        HumicAcidPercent = 0;
        FulvicAcidPercent = 0;
        TotalNutrientPercent = 0;

        // Reset PPM values
        NitrogenPpm = 0;
        PhosphorusPpm = 0;
        PotassiumPpm = 0;
        CalciumPpm = 0;
        MagnesiumPpm = 0;
        SulfurPpm = 0;
        BoronPpm = 0;
        CopperPpm = 0;
        IronPpm = 0;
        ManganesePpm = 0;
        MolybdenumPpm = 0;
        ZincPpm = 0;
        ChlorinePpm = 0;
        SilicaPpm = 0;
        HumicAcidPpm = 0;
        FulvicAcidPpm = 0;
        TotalNutrientPpm = 0;
    }

    private void AddFertilizerToMix(Fertilizer fertilizer)
    {
        if (fertilizer == null) return;

        try
        {
            var quantity = new FertilizerQuantity
            {
                FertilizerName = fertilizer.Name,
                Quantity = 1.0 // Default quantity
            };

            CurrentMix.Add(quantity);
            UpdateNutrientTotals();
        }
        catch (Exception ex)
        {
            dialogService.DisplayAlertAsync("Error", $"Failed to add fertilizer to mix: {ex.Message}", "OK");
        }
    }

    // Helper method to get the current navigation
    private INavigation GetCurrentNavigation()
    {
        // Try to get navigation from Shell first
        if (Shell.Current?.Navigation != null)
            return Shell.Current.Navigation;
            
        // Fallback to Application.Current.MainPage.Navigation
        return Application.Current?.MainPage?.Navigation;
    }
}
