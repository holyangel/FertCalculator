using FertCalculatorMaui.Services;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using System.Diagnostics;

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
        WeakReferenceMessenger.Default.Register<FertilizersUpdatedMessage>(this, async (r, message) => 
        {
            // Reload fertilizers from file
            await ReloadFertilizersAsync();
        });
    }
    
    private async Task ReloadFertilizersAsync()
    {
        // Load the latest fertilizers from the file
        var updatedFertilizers = await fileService.LoadFertilizersAsync();
        
        // Sort fertilizers alphabetically by name
        var sortedFertilizers = updatedFertilizers.OrderBy(f => f.Name).ToList();
        
        // Clear and repopulate the collection
        availableFertilizers.Clear();
        foreach (var fertilizer in sortedFertilizers)
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
        if (sender is Button button)
        {
            var fertilizerName = button.GetValue(Button.CommandParameterProperty) as string;
            if (!string.IsNullOrEmpty(fertilizerName))
            {
                var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
                if (fertilizer != null)
                {
                    await Navigation.PushAsync(new AddFertilizerPage(fileService, fertilizer));
                }
            }
        }
    }
    
    private async void OnDeleteFertilizerClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var fertilizerName = button.GetValue(Button.CommandParameterProperty) as string;
            if (!string.IsNullOrEmpty(fertilizerName))
            {
                bool confirm = await DisplayAlert("Confirm Delete", 
                    $"Are you sure you want to delete {fertilizerName}?", 
                    "Yes", "No");
                
                if (confirm)
                {
                    var fertilizerToRemove = availableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
                    if (fertilizerToRemove != null)
                    {
                        availableFertilizers.Remove(fertilizerToRemove);
                        await fileService.SaveFertilizersAsync(availableFertilizers.ToList());
                    }
                }
            }
        }
    }
    
    private void OnFertilizerSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // This method prevents the app from crashing when a fertilizer name is clicked
        // We could implement additional functionality here if needed
    }
    
    private async void OnAddToMixClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var fertilizerName = button.GetValue(Button.CommandParameterProperty) as string;
            if (!string.IsNullOrEmpty(fertilizerName))
            {
                var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
                if (fertilizer != null)
                {
                    try
                    {
                        // Send a message back to the MainPage to add this fertilizer to the mix
                        WeakReferenceMessenger.Default.Send(new AddFertilizerToMixMessage(fertilizer));
                        
                        // Go back to the MainPage
                        await Navigation.PopAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error sending message: {ex.Message}");
                        await DisplayAlert("Error", "Could not add fertilizer to mix", "OK");
                    }
                }
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
        
        // Force refresh of the CollectionView
        FertilizerListView.ItemsSource = null;
        FertilizerListView.ItemsSource = availableFertilizers;
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe from WeakReferenceMessenger to prevent memory leaks
        WeakReferenceMessenger.Default.Unregister<FertilizersUpdatedMessage>(this);
    }
}
