using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FertCalculatorMaui.Services;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;

namespace FertCalculatorMaui;

public partial class AddFertilizerPage : ContentPage
{
    private readonly FileService fileService;
    private List<Fertilizer> availableFertilizers = new List<Fertilizer>();
    private const string fertilizerDbPath = "Fertilizers.xml";

    public AddFertilizerPage(FileService fileService)
    {
        InitializeComponent();
        this.fileService = fileService;
        // Use discard syntax to explicitly indicate we're ignoring the task
        _ = LoadFertilizersAsync();
    }
    
    public AddFertilizerPage(FileService fileService, Fertilizer existingFertilizer)
    {
        InitializeComponent();
        this.fileService = fileService;
        _ = LoadFertilizersAsync();
        
        // Populate fields with existing fertilizer data
        NameEntry.SetValue(Entry.TextProperty, existingFertilizer.Name);
        NitrogenEntry.SetValue(Entry.TextProperty, existingFertilizer.NitrogenPercent.ToString());
        
        // For phosphorus and potassium, use the original values and set checkboxes
        if (existingFertilizer.IsPhosphorusInOxideForm)
        {
            PhosphorusEntry.SetValue(Entry.TextProperty, existingFertilizer.OriginalPhosphorusValue.ToString());
            PhosphorusOxideCheckbox.IsChecked = true;
        }
        else
        {
            PhosphorusEntry.SetValue(Entry.TextProperty, existingFertilizer.PhosphorusPercent.ToString());
            PhosphorusOxideCheckbox.IsChecked = false;
        }
        
        if (existingFertilizer.IsPotassiumInOxideForm)
        {
            PotassiumEntry.SetValue(Entry.TextProperty, existingFertilizer.OriginalPotassiumValue.ToString());
            PotassiumOxideCheckbox.IsChecked = true;
        }
        else
        {
            PotassiumEntry.SetValue(Entry.TextProperty, existingFertilizer.PotassiumPercent.ToString());
            PotassiumOxideCheckbox.IsChecked = false;
        }
        
        CalciumEntry.SetValue(Entry.TextProperty, existingFertilizer.CalciumPercent.ToString());
        MagnesiumEntry.SetValue(Entry.TextProperty, existingFertilizer.MagnesiumPercent.ToString());
        SulfurEntry.SetValue(Entry.TextProperty, existingFertilizer.SulfurPercent.ToString());
        BoronEntry.SetValue(Entry.TextProperty, existingFertilizer.BoronPercent.ToString());
        CopperEntry.SetValue(Entry.TextProperty, existingFertilizer.CopperPercent.ToString());
        IronEntry.SetValue(Entry.TextProperty, existingFertilizer.IronPercent.ToString());
        ManganeseEntry.SetValue(Entry.TextProperty, existingFertilizer.ManganesePercent.ToString());
        MolybdenumEntry.SetValue(Entry.TextProperty, existingFertilizer.MolybdenumPercent.ToString());
        ZincEntry.SetValue(Entry.TextProperty, existingFertilizer.ZincPercent.ToString());
        ChlorineEntry.SetValue(Entry.TextProperty, existingFertilizer.ChlorinePercent.ToString());
        SilicaEntry.SetValue(Entry.TextProperty, existingFertilizer.SilicaPercent.ToString());
        HumicAcidEntry.SetValue(Entry.TextProperty, existingFertilizer.HumicAcidPercent.ToString());
        FulvicAcidEntry.SetValue(Entry.TextProperty, existingFertilizer.FulvicAcidPercent.ToString());
    }

    private async Task LoadFertilizersAsync()
    {
        availableFertilizers = await fileService.LoadFertilizersAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var nameText = NameEntry.GetValue(Entry.TextProperty) as string;
        if (string.IsNullOrWhiteSpace(nameText))
        {
            await DisplayAlert("Error", "Please enter a fertilizer name", "OK");
            return;
        }

        // Check if fertilizer name already exists
        if (availableFertilizers.Any(f => f.Name.Equals(nameText, StringComparison.OrdinalIgnoreCase)))
        {
            bool overwrite = await DisplayAlert("Warning", "A fertilizer with this name already exists. Do you want to overwrite it?", "Yes", "No");
            if (!overwrite)
                return;
        }

        // Validate all entries are valid numbers or empty
        if (!ValidateNumericEntries())
        {
            await DisplayAlert("Error", "All nutrient values must be valid numbers", "OK");
            return;
        }

        // Create new fertilizer or update existing one
        var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name.Equals(nameText, StringComparison.OrdinalIgnoreCase)) 
                         ?? new Fertilizer { Name = nameText };

        // Parse nutrient values
        TryParseEntry(NitrogenEntry, value => fertilizer.NitrogenPercent = value);
        
        // Handle phosphorus with oxide form option
        if (TryParseEntry(PhosphorusEntry, out double phosphorusValue))
        {
            fertilizer.IsPhosphorusInOxideForm = PhosphorusOxideCheckbox.IsChecked;
            fertilizer.OriginalPhosphorusValue = phosphorusValue;
            
            // Convert from P₂O₅ to P if in oxide form
            if (fertilizer.IsPhosphorusInOxideForm)
            {
                fertilizer.PhosphorusPercent = Fertilizer.P2O5ToP(phosphorusValue);
            }
            else
            {
                fertilizer.PhosphorusPercent = phosphorusValue;
            }
        }
        
        // Handle potassium with oxide form option
        if (TryParseEntry(PotassiumEntry, out double potassiumValue))
        {
            fertilizer.IsPotassiumInOxideForm = PotassiumOxideCheckbox.IsChecked;
            fertilizer.OriginalPotassiumValue = potassiumValue;
            
            // Convert from K₂O to K if in oxide form
            if (fertilizer.IsPotassiumInOxideForm)
            {
                fertilizer.PotassiumPercent = Fertilizer.K2OToK(potassiumValue);
            }
            else
            {
                fertilizer.PotassiumPercent = potassiumValue;
            }
        }
        
        TryParseEntry(CalciumEntry, value => fertilizer.CalciumPercent = value);
        TryParseEntry(MagnesiumEntry, value => fertilizer.MagnesiumPercent = value);
        TryParseEntry(SulfurEntry, value => fertilizer.SulfurPercent = value);
        TryParseEntry(BoronEntry, value => fertilizer.BoronPercent = value);
        TryParseEntry(CopperEntry, value => fertilizer.CopperPercent = value);
        TryParseEntry(IronEntry, value => fertilizer.IronPercent = value);
        TryParseEntry(ManganeseEntry, value => fertilizer.ManganesePercent = value);
        TryParseEntry(MolybdenumEntry, value => fertilizer.MolybdenumPercent = value);
        TryParseEntry(ZincEntry, value => fertilizer.ZincPercent = value);
        TryParseEntry(ChlorineEntry, value => fertilizer.ChlorinePercent = value);
        TryParseEntry(SilicaEntry, value => fertilizer.SilicaPercent = value);
        TryParseEntry(HumicAcidEntry, value => fertilizer.HumicAcidPercent = value);
        TryParseEntry(FulvicAcidEntry, value => fertilizer.FulvicAcidPercent = value);

        // Add to list if new
        if (!availableFertilizers.Contains(fertilizer))
        {
            availableFertilizers.Add(fertilizer);
        }

        // Save to file
        await SaveFertilizersAsync();

        // Notify other pages that fertilizers have been updated
        WeakReferenceMessenger.Default.Send(new FertilizersUpdatedMessage(fertilizer));

        // Return to previous page
        await Navigation.PopAsync();
    }

    private static void TryParseEntry(Entry entry, Action<double> setter)
    {
        if (!string.IsNullOrWhiteSpace(entry.GetValue(Entry.TextProperty) as string) && double.TryParse(entry.GetValue(Entry.TextProperty) as string, out double value))
        {
            setter(value);
        }
    }

    private static bool TryParseEntry(Entry entry, out double value)
    {
        if (!string.IsNullOrWhiteSpace(entry.GetValue(Entry.TextProperty) as string) && double.TryParse(entry.GetValue(Entry.TextProperty) as string, out value))
        {
            return true;
        }
        value = 0;
        return false;
    }

    private bool ValidateNumericEntries()
    {
        var entries = new[] { 
            NitrogenEntry, PhosphorusEntry, PotassiumEntry, 
            CalciumEntry, MagnesiumEntry, SulfurEntry,
            BoronEntry, CopperEntry, IronEntry,
            ManganeseEntry, MolybdenumEntry, ZincEntry,
            ChlorineEntry, SilicaEntry, HumicAcidEntry, FulvicAcidEntry
        };
        
        // Use a static regex instance with compiled option to address SYSLIB1045
        return entries.All(e => _decimalRegex.IsMatch(e.GetValue(Entry.TextProperty) as string ?? string.Empty));
    }

    // Static regex instance with RegexOptions.Compiled for better performance
    private static readonly Regex _decimalRegex = new Regex(@"^$|^\d*\.?\d*$", RegexOptions.Compiled);

    private async Task SaveFertilizersAsync()
    {
        try
        {
            await fileService.SaveFertilizersAsync(availableFertilizers);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save fertilizers: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
