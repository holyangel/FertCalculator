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
    private double kelpPercent;

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
    private double kelpPpm;

    [ObservableProperty]
    private double totalNutrientPpm;

    public MainViewModel(FileService fileService, IDialogService dialogService)
    {
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        Debug.WriteLine("MainViewModel constructor called with injected services");

        // Load settings directly from Preferences
        try
        {
            UseImperialUnits = Preferences.Default.Get("UseImperialUnits", false);
            Debug.WriteLine($"Loaded unit preference: UseImperialUnits = {UseImperialUnits}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading unit preference: {ex.Message}");
            UseImperialUnits = false; // Default to metric if there's an error
        }
        
        // Initialize unit labels
        UpdateUnitDisplay();
        
        // Register for messages
        WeakReferenceMessenger.Default.Register<AddFertilizerToMixMessage>(this, (r, message) => {
            AddFertilizerToMix(message.Value);
        });
        
        WeakReferenceMessenger.Default.Register<FertilizerDeletedMessage>(this, (r, message) => {
            // Remove the deleted fertilizer from the current mix if present
            for (int i = CurrentMix.Count - 1; i >= 0; i--)
            {
                if (string.Equals(CurrentMix[i].FertilizerName, message.Value, StringComparison.OrdinalIgnoreCase))
                    CurrentMix.RemoveAt(i);
            }
            UpdateNutrientTotals();
        });
        
        Debug.WriteLine("MainViewModel initialization completed");
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
                var selectedMix = SavedMixes.FirstOrDefault(m => string.Equals(m.Name, mix, StringComparison.OrdinalIgnoreCase));
                if (selectedMix != null)
                {
                    if (CurrentMix.Count > 0)
                    {
                        bool confirm = await dialogService.DisplayConfirmationAsync("Confirm Load",
                            "Loading a mix will replace your current mix. Continue?", "Yes", "No");
                        if (!confirm)
                            return;
                    }

                    CurrentMix.Clear();
                    int skipped = 0;
                    foreach (var item in selectedMix.Ingredients)
                    {
                        // Verify the fertilizer still exists, and copy the quantity so
                        // the saved mix isn't mutated by later edits
                        if (AvailableFertilizers.Any(f => string.Equals(f.Name, item.FertilizerName, StringComparison.OrdinalIgnoreCase)))
                        {
                            CurrentMix.Add(new FertilizerQuantity
                            {
                                FertilizerName = item.FertilizerName,
                                Quantity = item.Quantity
                            });
                        }
                        else
                        {
                            skipped++;
                        }
                    }
                    UpdateNutrientTotals();

                    if (skipped > 0)
                    {
                        await dialogService.DisplayAlertAsync("Mix Loaded",
                            $"Skipped {skipped} ingredient(s) whose fertilizer no longer exists.", "OK");
                    }
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
            var item = CurrentMix.FirstOrDefault(x => string.Equals(x.FertilizerName, fertilizerName, StringComparison.OrdinalIgnoreCase));
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
    public void ToggleUnits()
    {
        UseImperialUnits = !UseImperialUnits;
        UpdateUnitDisplay();
        UpdateNutrientTotals();

        // Save the setting
        try
        {
            Preferences.Default.Set("UseImperialUnits", UseImperialUnits);
            Debug.WriteLine($"Units toggled. UseImperialUnits is now: {UseImperialUnits}");
            
            // Send a message to notify other components about the unit change
            WeakReferenceMessenger.Default.Send(new UnitChangedMessage(UseImperialUnits));
            Debug.WriteLine("Sent UnitChangedMessage");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving unit preference: {ex.Message}");
        }
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
        
        // Explicitly notify property changes to ensure UI updates
        OnPropertyChanged(nameof(UnitsTypeLabel));
        OnPropertyChanged(nameof(PpmHeaderLabel));
    }

    public void UpdateNutrientTotals()
    {
        try
        {
            // Reset all totals
            ResetNutrientTotals();

            double totalQuantity = 0;

            // Calculate the weighted average nutrient percentage of the mix
            foreach (var item in CurrentMix)
            {
                var fertilizer = AvailableFertilizers.FirstOrDefault(f => string.Equals(f.Name, item.FertilizerName, StringComparison.OrdinalIgnoreCase));
                if (fertilizer != null)
                {
                    // For liquids the quantity is mL — weight by density to get grams.
                    double quantity = item.Quantity * fertilizer.EffectiveDensity;
                    totalQuantity += quantity;

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
                    KelpPercent += (fertilizer.KelpPercent / 100) * quantity;
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
                KelpPercent = (KelpPercent / totalQuantity) * 100;
            }

            // Calculate total nutrient percentage
            TotalNutrientPercent = NitrogenPercent + PhosphorusPercent + PotassiumPercent +
                                  CalciumPercent + MagnesiumPercent + SulfurPercent +
                                  BoronPercent + CopperPercent + IronPercent +
                                  ManganesePercent + MolybdenumPercent + ZincPercent +
                                  ChlorinePercent + SilicaPercent + HumicAcidPercent + FulvicAcidPercent + KelpPercent;

            // Sum PPM contributions via the shared calculator
            var totals = MixCalculator.Calculate(CurrentMix, AvailableFertilizers, UseImperialUnits);
            NitrogenPpm = totals.Nitrogen;
            PhosphorusPpm = totals.Phosphorus;
            PotassiumPpm = totals.Potassium;
            CalciumPpm = totals.Calcium;
            MagnesiumPpm = totals.Magnesium;
            SulfurPpm = totals.Sulfur;
            BoronPpm = totals.Boron;
            CopperPpm = totals.Copper;
            IronPpm = totals.Iron;
            ManganesePpm = totals.Manganese;
            MolybdenumPpm = totals.Molybdenum;
            ZincPpm = totals.Zinc;
            ChlorinePpm = totals.Chlorine;
            SilicaPpm = totals.Silica;
            HumicAcidPpm = totals.HumicAcid;
            FulvicAcidPpm = totals.FulvicAcid;
            KelpPpm = totals.Kelp;
            TotalNutrientPpm = totals.Total;
            
            // Explicitly notify property changes for all PPM values
            NotifyAllPropertyChanges();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating nutrient totals: {ex.Message}");
        }
    }
    
    private void NotifyAllPropertyChanges()
    {
        // Notify all percent properties
        OnPropertyChanged(nameof(NitrogenPercent));
        OnPropertyChanged(nameof(PhosphorusPercent));
        OnPropertyChanged(nameof(PotassiumPercent));
        OnPropertyChanged(nameof(CalciumPercent));
        OnPropertyChanged(nameof(MagnesiumPercent));
        OnPropertyChanged(nameof(SulfurPercent));
        OnPropertyChanged(nameof(BoronPercent));
        OnPropertyChanged(nameof(CopperPercent));
        OnPropertyChanged(nameof(IronPercent));
        OnPropertyChanged(nameof(ManganesePercent));
        OnPropertyChanged(nameof(MolybdenumPercent));
        OnPropertyChanged(nameof(ZincPercent));
        OnPropertyChanged(nameof(ChlorinePercent));
        OnPropertyChanged(nameof(SilicaPercent));
        OnPropertyChanged(nameof(HumicAcidPercent));
        OnPropertyChanged(nameof(FulvicAcidPercent));
        OnPropertyChanged(nameof(KelpPercent));
        
        // Notify all PPM properties
        OnPropertyChanged(nameof(NitrogenPpm));
        OnPropertyChanged(nameof(PhosphorusPpm));
        OnPropertyChanged(nameof(PotassiumPpm));
        OnPropertyChanged(nameof(CalciumPpm));
        OnPropertyChanged(nameof(MagnesiumPpm));
        OnPropertyChanged(nameof(SulfurPpm));
        OnPropertyChanged(nameof(BoronPpm));
        OnPropertyChanged(nameof(CopperPpm));
        OnPropertyChanged(nameof(IronPpm));
        OnPropertyChanged(nameof(ManganesePpm));
        OnPropertyChanged(nameof(MolybdenumPpm));
        OnPropertyChanged(nameof(ZincPpm));
        OnPropertyChanged(nameof(ChlorinePpm));
        OnPropertyChanged(nameof(SilicaPpm));
        OnPropertyChanged(nameof(HumicAcidPpm));
        OnPropertyChanged(nameof(FulvicAcidPpm));
        OnPropertyChanged(nameof(KelpPpm));
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
        KelpPercent = 0;
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
        KelpPpm = 0;
        TotalNutrientPpm = 0;
    }

    private void AddFertilizerToMix(Fertilizer fertilizer)
    {
        if (fertilizer == null) return;

        try
        {
            // Prevent duplicate entries in the mix
            if (CurrentMix.Any(x => string.Equals(x.FertilizerName, fertilizer.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _ = dialogService.DisplayAlertAsync("Already Added",
                    "This fertilizer is already in the current mix.", "OK");
                return;
            }

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
