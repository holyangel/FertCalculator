using FertCalculatorMaui.Services;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace FertCalculatorMaui;

public partial class ManageFertilizersPage : ContentPage
{
    private ManageFertilizersViewModel viewModel;
    private readonly FileService fileService;
    private readonly IDialogService dialogService;
    
    public ManageFertilizersPage(FileService fileService, IDialogService dialogService, ObservableCollection<Fertilizer> fertilizers)
    {
        InitializeComponent();
        this.fileService = fileService;
        this.dialogService = dialogService;
        
        // Create and set the ViewModel
        viewModel = new ManageFertilizersViewModel(fileService, dialogService, fertilizers);
        BindingContext = viewModel;
        
        // Subscribe to the FertilizersUpdated message from any instance of AddFertilizerPage
        WeakReferenceMessenger.Default.Register<FertilizersUpdatedMessage>(this, async (r, message) => 
        {
            // Reload fertilizers from file
            await viewModel.ReloadFertilizersAsync();
        });
    }
    
    // Event handlers for the buttons that still use events instead of commands
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
                var fertilizer = viewModel.AvailableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
                if (fertilizer != null)
                {
                    await Navigation.PushAsync(new AddFertilizerPage(fileService, fertilizer));
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
                var fertilizer = viewModel.AvailableFertilizers.FirstOrDefault(f => f.Name == fertilizerName);
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
                        await dialogService.DisplayAlert("Error", "Could not add fertilizer to mix", "OK");
                    }
                }
            }
        }
    }
    
    private async void OnDeleteFertilizerClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var fertilizer = button.CommandParameter as Fertilizer;
            if (fertilizer != null)
            {
                // Call the ViewModel's DeleteFertilizer method
                await viewModel.DeleteFertilizerAsync(fertilizer);
            }
        }
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Reload fertilizers from file when the page appears
        await viewModel.ReloadFertilizersAsync();
        
        // Force refresh of the CollectionView
        FertilizerListView.ItemsSource = null;
        FertilizerListView.ItemsSource = viewModel.AvailableFertilizers;
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe from WeakReferenceMessenger to prevent memory leaks
        WeakReferenceMessenger.Default.Unregister<FertilizersUpdatedMessage>(this);
    }
}
