using System.Text.RegularExpressions;
using FertCalculatorMaui.Services;

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
        TryParseEntry(PhosphorusEntry, value => fertilizer.PhosphorusPercent = value);
        TryParseEntry(PotassiumEntry, value => fertilizer.PotassiumPercent = value);
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
