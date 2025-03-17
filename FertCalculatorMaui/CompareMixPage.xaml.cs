using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FertCalculatorMaui.Services;
using System.Diagnostics;
using FertCalculatorMaui.ViewModels;
using FertCalculatorMaui.Models;
using LiveChartsCore.SkiaSharpView;

namespace FertCalculatorMaui;

public partial class CompareMixPage : ContentPage
{
    private readonly FileService _fileService;
    private readonly IDialogService _dialogService;
    private ObservableCollection<Fertilizer> _cachedFertilizers;
    public CompareMixViewModel ViewModel { get; private set; }

    public CompareMixPage(FileService fileService, IDialogService dialogService)
    {
        InitializeComponent();
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        // Load data first - will use current mix from MainViewModel
        _ = LoadAndInitializeAsync(null, false);
    }

    public CompareMixPage(FileService fileService, List<FertilizerQuantity> currentMixIngredients, bool useImperialUnits)
    {
        InitializeComponent();
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _dialogService = new DialogService(); // Fallback for backward compatibility
        
        // Load data first
        _ = LoadAndInitializeAsync(currentMixIngredients, useImperialUnits);
    }

    public CompareMixPage(FileService fileService, FertilizerMix mix1, FertilizerMix mix2, List<Fertilizer> availableFertilizers, bool useImperialUnits)
    {
        InitializeComponent();
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _dialogService = new DialogService(); // Fallback for backward compatibility
        
        // Cache the fertilizers
        _cachedFertilizers = new ObservableCollection<Fertilizer>(availableFertilizers);
        
        // Create a collection with just these two mixes
        var mixes = new ObservableCollection<FertilizerMix> { mix1, mix2 };
        
        // Initialize ViewModel
        ViewModel = new CompareMixViewModel(_cachedFertilizers, mixes)
        {
            UseImperialUnits = useImperialUnits,
            SelectedMix1 = mix1,
            SelectedMix2 = mix2
        };
        
        // Set binding context
        BindingContext = ViewModel;

        // Subscribe to the CloseRequested event
        ViewModel.CloseRequested += OnCloseRequested;
    }
    
    private async Task LoadAndInitializeAsync(List<FertilizerQuantity> currentMixIngredients, bool useImperialUnits)
    {
        try
        {
            // Load available fertilizers
            var fertilizers = await _fileService.LoadFertilizersAsync();
            _cachedFertilizers = new ObservableCollection<Fertilizer>(fertilizers);
            
            // Load available mixes
            var availableMixes = await _fileService.LoadMixesAsync() ?? new ObservableCollection<FertilizerMix>();
            
            if (_cachedFertilizers != null)
            {
                // Check if current mix has ingredients before adding
                if (currentMixIngredients != null && currentMixIngredients.Count > 0)
                {
                    // Create temporary mix for comparison
                    var currentMix = new FertilizerMix { 
                        Name = "Current Mix", 
                        Ingredients = currentMixIngredients 
                    };
                    
                    // Add current mix at the beginning of the list
                    availableMixes.Insert(0, currentMix);
                }
                
                // Initialize ViewModel with loaded data
                ViewModel = new CompareMixViewModel(_cachedFertilizers, availableMixes)
                {
                    UseImperialUnits = useImperialUnits
                };
                
                // Set binding context
                BindingContext = ViewModel;

                // Subscribe to the CloseRequested event
                ViewModel.CloseRequested += OnCloseRequested;
                
                // Set initial selections after everything is initialized
                if (availableMixes.Count > 0)
                {
                    // Set Mix2 to the second mix if available, otherwise use the first one
                    var mix2 = availableMixes.Count > 1 ? availableMixes[1] : availableMixes[0];
                    
                    // Set selections in the correct order to trigger proper updates
                    ViewModel.SelectedMix2 = mix2;
                    ViewModel.SelectedMix1 = availableMixes[0];
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in LoadAndInitializeAsync: {ex.Message}");
            await _dialogService.DisplayAlertAsync("Error", $"Failed to load data: {ex.Message}", "OK");
        }
    }

    private async void OnCloseRequested(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private Fertilizer FindFertilizer(string name)
    {
        return _cachedFertilizers?.FirstOrDefault(f => f.Name == name);
    }
}
