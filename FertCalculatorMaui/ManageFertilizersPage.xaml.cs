using FertCalculatorMaui.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using System.Diagnostics;
using FertCalculatorMaui.ViewModels;

namespace FertCalculatorMaui;

public partial class ManageFertilizersPage : ContentPage
{
    private readonly ManageFertilizersViewModel viewModel;
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
