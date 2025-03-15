using FertCalculatorMaui.Services;
using System.Collections.ObjectModel;

namespace FertCalculatorMaui;

public partial class ManageFertilizersPage : ContentPage
{
    private ObservableCollection<Fertilizer> availableFertilizers;
    private readonly FileService fileService;
    
    public ManageFertilizersPage(FileService fileService, ObservableCollection<Fertilizer> fertilizers)
    {
        InitializeComponent();
        this.fileService = fileService;
        this.availableFertilizers = fertilizers;
        
        // Set the ItemsSource for the CollectionView
        FertilizerListView.ItemsSource = availableFertilizers;
        
        // Subscribe to the FertilizersUpdated message from any instance of AddFertilizerPage
        MessagingCenter.Subscribe<object, Fertilizer>(this, "FertilizersUpdated", async (sender, fertilizer) => 
        {
            // Reload fertilizers from file
            await ReloadFertilizersAsync();
        });
    }
    
    private async Task ReloadFertilizersAsync()
    {
        // Load the latest fertilizers from the file
        var updatedFertilizers = await fileService.LoadFertilizersAsync();
        
        // Clear and repopulate the collection
        availableFertilizers.Clear();
        foreach (var fertilizer in updatedFertilizers)
        {
            availableFertilizers.Add(fertilizer);
        }
        
        // Refresh the list view
        FertilizerListView.ItemsSource = null;
        FertilizerListView.ItemsSource = availableFertilizers;
    }
    
    private async void OnAddFertilizerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddFertilizerPage(fileService));
    }
    
    private async void OnEditFertilizerClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string fertilizerName)
        {
            var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
            if (fertilizer != null)
            {
                await Navigation.PushAsync(new AddFertilizerPage(fileService, fertilizer));
            }
        }
    }
    
    private void OnRemoveFertilizerClicked(object sender, EventArgs e)
    {
        if (FertilizerListView.SelectedItem is Fertilizer selectedFertilizer)
        {
            availableFertilizers.Remove(selectedFertilizer);
            SaveFertilizers();
        }
    }
    
    private void OnFertilizerSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // This method prevents the app from crashing when a fertilizer name is clicked
        // We could implement additional functionality here if needed
    }
    
    private async void OnAddToMixClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string fertilizerName)
        {
            var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
            if (fertilizer != null)
            {
                // Send a message back to the MainPage to add this fertilizer to the mix
                MessagingCenter.Send(this, "AddFertilizerToMix", fertilizer);
                
                // Go back to the MainPage
                await Navigation.PopAsync();
            }
        }
    }
    
    private async void OnBackToMixClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    
    private async void SaveFertilizers()
    {
        await fileService.SaveFertilizersAsync(availableFertilizers.ToList());
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Reload fertilizers from file when the page appears
        await ReloadFertilizersAsync();
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe from MessagingCenter to prevent memory leaks
        MessagingCenter.Unsubscribe<object, Fertilizer>(this, "FertilizersUpdated");
    }
}
