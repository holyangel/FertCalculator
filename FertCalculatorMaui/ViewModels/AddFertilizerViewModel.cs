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

    public AddFertilizerViewModel(FileService fileService, IDialogService dialogService)
    {
        this.fileService = fileService;
        this.dialogService = dialogService;
    }

    public void LoadExistingFertilizer(Fertilizer fertilizer)
    {
        if (fertilizer == null) return;

        Name = fertilizer.Name;
        NitrogenPercent = fertilizer.NitrogenPercent;
        
        // Handle phosphorus with oxide form
        IsPhosphorusInOxideForm = fertilizer.IsPhosphorusInOxideForm;
        OriginalPhosphorusValue = fertilizer.OriginalPhosphorusValue;
        PhosphorusPercent = fertilizer.PhosphorusPercent;
        
        // Handle potassium with oxide form
        IsPotassiumInOxideForm = fertilizer.IsPotassiumInOxideForm;
        OriginalPotassiumValue = fertilizer.OriginalPotassiumValue;
        PotassiumPercent = fertilizer.PotassiumPercent;
        
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
        
        // Check if fertilizer name already exists
        if (fertilizers.Any(f => f.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)))
        {
            bool overwrite = await dialogService.DisplayConfirmationAsync("Warning", 
                "A fertilizer with this name already exists. Do you want to overwrite it?", 
                "Yes", "No");
            if (!overwrite)
                return;
        }

        // Create new fertilizer or update existing one
        var fertilizer = fertilizers.FirstOrDefault(f => f.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)) 
                        ?? new Fertilizer { Name = Name };

        // Set nutrient values
        fertilizer.NitrogenPercent = NitrogenPercent;
        
        // Handle phosphorus with oxide form
        fertilizer.IsPhosphorusInOxideForm = IsPhosphorusInOxideForm;
        fertilizer.OriginalPhosphorusValue = OriginalPhosphorusValue;
        fertilizer.PhosphorusPercent = IsPhosphorusInOxideForm ? 
            Fertilizer.P2O5ToP(OriginalPhosphorusValue) : 
            PhosphorusPercent;
        
        // Handle potassium with oxide form
        fertilizer.IsPotassiumInOxideForm = IsPotassiumInOxideForm;
        fertilizer.OriginalPotassiumValue = OriginalPotassiumValue;
        fertilizer.PotassiumPercent = IsPotassiumInOxideForm ? 
            Fertilizer.K2OToK(OriginalPotassiumValue) : 
            PotassiumPercent;
        
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
        // Validate all numeric entries
        var numericProperties = new[]
        {
            NitrogenPercent,
            PhosphorusPercent,
            PotassiumPercent,
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
            FulvicAcidPercent
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
