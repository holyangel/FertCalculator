using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

public partial class SaveMixPage : ContentPage
{
    private readonly FileService fileService;
    private readonly List<FertilizerQuantity> currentMix;
    private List<FertilizerMix> savedMixes;
    private const string mixesDbPath = "Mixes.xml";

    public SaveMixPage(FileService fileService, List<FertilizerQuantity> currentMix)
    {
        InitializeComponent();
        this.fileService = fileService;
        this.currentMix = new List<FertilizerQuantity>(currentMix); // Create a copy
        // Use discard syntax to explicitly indicate we're ignoring the task
        _ = LoadMixesAsync();
    }

    private async Task LoadMixesAsync()
    {
        savedMixes = await fileService.LoadFromXmlAsync<List<FertilizerMix>>(mixesDbPath) ?? new List<FertilizerMix>();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MixNameEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a mix name", "OK");
            return;
        }

        // Check if mix name already exists
        if (savedMixes.Any(m => m.Name.Equals(MixNameEntry.Text, StringComparison.OrdinalIgnoreCase)))
        {
            bool overwrite = await DisplayAlert("Warning", "A mix with this name already exists. Do you want to overwrite it?", "Yes", "No");
            if (!overwrite)
                return;

            // Remove existing mix with the same name
            savedMixes.RemoveAll(m => m.Name.Equals(MixNameEntry.Text, StringComparison.OrdinalIgnoreCase));
        }

        // Create a new mix
        var mix = new FertilizerMix
        {
            Name = MixNameEntry.Text,
            Ingredients = new List<FertilizerQuantity>(currentMix) // Create a copy
        };

        // Add to saved mixes
        savedMixes.Add(mix);

        // Save to file
        await SaveMixesAsync();

        // Return to previous page
        await Navigation.PopAsync();
    }

    private async Task SaveMixesAsync()
    {
        try
        {
            await fileService.SaveToXmlAsync(savedMixes, mixesDbPath);
            await DisplayAlert("Success", "Mix saved successfully", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save mix: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
