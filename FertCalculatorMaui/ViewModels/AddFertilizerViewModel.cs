using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui.ViewModels;

public partial class AddFertilizerViewModel : ObservableObject
{
    private readonly FileService fileService;
    private readonly IDialogService dialogService;
    private Fertilizer? editingFertilizer;
    private static readonly Regex _decimalRegex = new(@"^-?\d*\.?\d*$", RegexOptions.Compiled);

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private double nitrogenPercent;

    [ObservableProperty]
    private double phosphorusPercent;

    [ObservableProperty]
    private bool isPhosphorusInOxideForm;

    [ObservableProperty]
    private double originalPhosphorusValue;

    [ObservableProperty]
    private double potassiumPercent;

    [ObservableProperty]
    private bool isPotassiumInOxideForm;

    [ObservableProperty]
    private double originalPotassiumValue;

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
    private bool isLiquid;

    [ObservableProperty]
    private double density = 1.0;

    public AddFertilizerViewModel(FileService fileService, IDialogService dialogService)
    {
        this.fileService = fileService;
        this.dialogService = dialogService;

        // Default new fertilizers to oxide form for P and K, matching the desktop
        // app and typical US fertilizer labels.
        IsPhosphorusInOxideForm = true;
        IsPotassiumInOxideForm = true;
    }

    public void LoadExistingFertilizer(Fertilizer fertilizer)
    {
        if (fertilizer == null) return;

        editingFertilizer = fertilizer;
        Name = fertilizer.Name;
        NitrogenPercent = fertilizer.NitrogenPercent;
        
        // Handle phosphorus with oxide form
        IsPhosphorusInOxideForm = fertilizer.IsPhosphorusInOxideForm;
        OriginalPhosphorusValue = fertilizer.OriginalPhosphorusValue;
        PhosphorusPercent = fertilizer.PhosphorusPercent;

        // Backfill the original label value for legacy entries that predate it,
        // so re-saving doesn't zero out the nutrient
        if (IsPhosphorusInOxideForm && OriginalPhosphorusValue == 0 && PhosphorusPercent > 0)
        {
            OriginalPhosphorusValue = PhosphorusPercent / Fertilizer.P2O5ToP(1);
        }
        
        // Handle potassium with oxide form
        IsPotassiumInOxideForm = fertilizer.IsPotassiumInOxideForm;
        OriginalPotassiumValue = fertilizer.OriginalPotassiumValue;
        PotassiumPercent = fertilizer.PotassiumPercent;

        if (IsPotassiumInOxideForm && OriginalPotassiumValue == 0 && PotassiumPercent > 0)
        {
            OriginalPotassiumValue = PotassiumPercent / Fertilizer.K2OToK(1);
        }
        
        CalciumPercent = fertilizer.CalciumPercent;
        MagnesiumPercent = fertilizer.MagnesiumPercent;
        SulfurPercent = fertilizer.SulfurPercent;
        BoronPercent = fertilizer.BoronPercent;
        CopperPercent = fertilizer.CopperPercent;
        IronPercent = fertilizer.IronPercent;
        ManganesePercent = fertilizer.ManganesePercent;
        MolybdenumPercent = fertilizer.MolybdenumPercent;
        ZincPercent = fertilizer.ZincPercent;
        ChlorinePercent = fertilizer.ChlorinePercent;
        SilicaPercent = fertilizer.SilicaPercent;
        HumicAcidPercent = fertilizer.HumicAcidPercent;
        FulvicAcidPercent = fertilizer.FulvicAcidPercent;
        KelpPercent = fertilizer.KelpPercent;
        IsLiquid = fertilizer.IsLiquid;
        Density = fertilizer.Density > 0 ? fertilizer.Density : 1.0;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await dialogService.DisplayAlertAsync("Error", "Please enter a fertilizer name", "OK");
            return;
        }

        if (!ValidateNumericEntries())
        {
            await dialogService.DisplayAlertAsync("Error", "Please enter valid numeric values", "OK");
            return;
        }

        var fertilizers = await fileService.LoadFertilizersAsync();

        var existingByName = fertilizers.FirstOrDefault(f =>
            string.Equals(f.Name, Name, StringComparison.OrdinalIgnoreCase));
        bool isRename = editingFertilizer != null &&
            !string.Equals(editingFertilizer.Name, Name, StringComparison.OrdinalIgnoreCase);

        // Reject renames that would collide with a different existing fertilizer
        if (isRename && existingByName != null)
        {
            await dialogService.DisplayAlertAsync("Error",
                "A fertilizer with this name already exists.", "OK");
            return;
        }

        // For new fertilizers, confirm before overwriting an existing entry
        if (editingFertilizer == null && existingByName != null)
        {
            bool overwrite = await dialogService.DisplayConfirmationAsync("Warning", 
                "A fertilizer with this name already exists. Do you want to overwrite it?", 
                "Yes", "No");
            if (!overwrite)
                return;
        }

        Fertilizer fertilizer;
        if (editingFertilizer != null)
        {
            // Drop the persisted copy stored under the old name (handles renames)
            fertilizers.RemoveAll(f => string.Equals(f.Name, editingFertilizer.Name, StringComparison.OrdinalIgnoreCase));
            fertilizer = editingFertilizer;
            fertilizer.Name = Name;
        }
        else
        {
            fertilizer = existingByName ?? new Fertilizer { Name = Name };
        }

        // Set nutrient values
        fertilizer.NitrogenPercent = NitrogenPercent;
        
        // Handle phosphorus with oxide form. The entry binds the raw label value
        // (OriginalPhosphorusValue); store elemental P.
        fertilizer.IsPhosphorusInOxideForm = IsPhosphorusInOxideForm;
        fertilizer.OriginalPhosphorusValue = OriginalPhosphorusValue;
        fertilizer.PhosphorusPercent = IsPhosphorusInOxideForm ? 
            Fertilizer.P2O5ToP(OriginalPhosphorusValue) : 
            OriginalPhosphorusValue;
        
        // Handle potassium with oxide form
        fertilizer.IsPotassiumInOxideForm = IsPotassiumInOxideForm;
        fertilizer.OriginalPotassiumValue = OriginalPotassiumValue;
        fertilizer.PotassiumPercent = IsPotassiumInOxideForm ? 
            Fertilizer.K2OToK(OriginalPotassiumValue) : 
            OriginalPotassiumValue;
        
        fertilizer.CalciumPercent = CalciumPercent;
        fertilizer.MagnesiumPercent = MagnesiumPercent;
        fertilizer.SulfurPercent = SulfurPercent;
        fertilizer.BoronPercent = BoronPercent;
        fertilizer.CopperPercent = CopperPercent;
        fertilizer.IronPercent = IronPercent;
        fertilizer.ManganesePercent = ManganesePercent;
        fertilizer.MolybdenumPercent = MolybdenumPercent;
        fertilizer.ZincPercent = ZincPercent;
        fertilizer.ChlorinePercent = ChlorinePercent;
        fertilizer.SilicaPercent = SilicaPercent;
        fertilizer.HumicAcidPercent = HumicAcidPercent;
        fertilizer.FulvicAcidPercent = FulvicAcidPercent;
        fertilizer.KelpPercent = KelpPercent;
        fertilizer.IsLiquid = IsLiquid;
        fertilizer.Density = Density > 0 ? Density : 1.0;

        // Add to list if new
        if (!fertilizers.Contains(fertilizer))
        {
            fertilizers.Add(fertilizer);
        }

        // Save to file
        await fileService.SaveFertilizersAsync(fertilizers);

        // Notify other pages that fertilizers have been updated
        WeakReferenceMessenger.Default.Send(new FertilizersUpdatedMessage(fertilizer));

        // Navigate back
        await Shell.Current.Navigation.PopAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.Navigation.PopAsync();
    }

    public async Task LoadFertilizersAsync()
    {
        try
        {
            var fertilizers = await fileService.LoadFertilizersAsync();
            if (fertilizers == null || !fertilizers.Any())
            {
                await dialogService.DisplayAlertAsync("No Fertilizers", 
                    "No fertilizers found. Please add some fertilizers.", "OK");
            }
        }
        catch (Exception ex)
        {
            await dialogService.DisplayAlertAsync("Error", 
                $"Failed to load fertilizers: {ex.Message}", "OK");
        }
    }

    private bool ValidateNumericEntries()
    {
        // Validate all numeric entries — P and K are validated on the raw label
        // values the user actually typed (Original values), not the stored
        // elemental conversions
        var numericProperties = new[]
        {
            NitrogenPercent,
            OriginalPhosphorusValue,
            OriginalPotassiumValue,
            CalciumPercent,
            MagnesiumPercent,
            SulfurPercent,
            BoronPercent,
            CopperPercent,
            IronPercent,
            ManganesePercent,
            MolybdenumPercent,
            ZincPercent,
            ChlorinePercent,
            SilicaPercent,
            HumicAcidPercent,
            FulvicAcidPercent,
            KelpPercent
        };

        return numericProperties.All(value => value >= 0 && value <= 100);
    }

    partial void OnIsPhosphorusInOxideFormChanged(bool value)
    {
        if (value)
        {
            PhosphorusPercent = Fertilizer.P2O5ToP(OriginalPhosphorusValue);
        }
        else
        {
            PhosphorusPercent = OriginalPhosphorusValue;
        }
    }

    partial void OnIsPotassiumInOxideFormChanged(bool value)
    {
        if (value)
        {
            PotassiumPercent = Fertilizer.K2OToK(OriginalPotassiumValue);
        }
        else
        {
            PotassiumPercent = OriginalPotassiumValue;
        }
    }
}
