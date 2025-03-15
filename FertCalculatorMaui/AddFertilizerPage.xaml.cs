using System.Text.RegularExpressions;
using FertCalculatorMaui.Services;
using Microsoft.Maui.Controls;

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
        NameEntry.Text = existingFertilizer.Name;
        NitrogenEntry.Text = existingFertilizer.NitrogenPercent.ToString();
        
        // For phosphorus and potassium, use the original values and set checkboxes
        if (existingFertilizer.IsPhosphorusInOxideForm)
        {
            PhosphorusEntry.Text = existingFertilizer.OriginalPhosphorusValue.ToString();
            PhosphorusOxideCheckbox.IsChecked = true;
        }
        else
        {
            PhosphorusEntry.Text = existingFertilizer.PhosphorusPercent.ToString();
            PhosphorusOxideCheckbox.IsChecked = false;
        }
        
        if (existingFertilizer.IsPotassiumInOxideForm)
        {
            PotassiumEntry.Text = existingFertilizer.OriginalPotassiumValue.ToString();
            PotassiumOxideCheckbox.IsChecked = true;
        }
        else
        {
            PotassiumEntry.Text = existingFertilizer.PotassiumPercent.ToString();
            PotassiumOxideCheckbox.IsChecked = false;
        }
        
        CalciumEntry.Text = existingFertilizer.CalciumPercent.ToString();
        MagnesiumEntry.Text = existingFertilizer.MagnesiumPercent.ToString();
        SulfurEntry.Text = existingFertilizer.SulfurPercent.ToString();
        BoronEntry.Text = existingFertilizer.BoronPercent.ToString();
        CopperEntry.Text = existingFertilizer.CopperPercent.ToString();
        IronEntry.Text = existingFertilizer.IronPercent.ToString();
        ManganeseEntry.Text = existingFertilizer.ManganesePercent.ToString();
        MolybdenumEntry.Text = existingFertilizer.MolybdenumPercent.ToString();
        ZincEntry.Text = existingFertilizer.ZincPercent.ToString();
    }

    private async Task LoadFertilizersAsync()
    {
        availableFertilizers = await fileService.LoadFromXmlAsync<List<Fertilizer>>(fertilizerDbPath) ?? new List<Fertilizer>();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a fertilizer name", "OK");
            return;
        }

        // Check if fertilizer name already exists
        if (availableFertilizers.Any(f => f.Name.Equals(NameEntry.Text, StringComparison.OrdinalIgnoreCase)))
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
        var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name.Equals(NameEntry.Text, StringComparison.OrdinalIgnoreCase)) 
                         ?? new Fertilizer { Name = NameEntry.Text };

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

        // Add to list if new
        if (!availableFertilizers.Contains(fertilizer))
        {
            availableFertilizers.Add(fertilizer);
        }

        // Save to file
        await SaveFertilizersAsync();

        // Notify other pages that fertilizers have been updated
        MessagingCenter.Send(this, "FertilizersUpdated", fertilizer);

        // Return to previous page
        await Navigation.PopAsync();
    }

    private void TryParseEntry(Entry entry, Action<double> setter)
    {
        if (!string.IsNullOrWhiteSpace(entry.Text) && double.TryParse(entry.Text, out double value))
        {
            setter(value);
        }
    }

    private bool TryParseEntry(Entry entry, out double value)
    {
        if (!string.IsNullOrWhiteSpace(entry.Text) && double.TryParse(entry.Text, out value))
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
            ManganeseEntry, MolybdenumEntry, ZincEntry
        };
        
        var regex = new Regex(@"^$|^\d*\.?\d*$"); // Empty or valid decimal number
        return entries.All(e => regex.IsMatch(e.Text ?? string.Empty));
    }

    private async Task SaveFertilizersAsync()
    {
        try
        {
            await fileService.SaveToXmlAsync(availableFertilizers, fertilizerDbPath);
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
