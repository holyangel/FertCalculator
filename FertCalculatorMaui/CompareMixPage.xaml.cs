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

    // Single constructor for dependency injection
    public CompareMixPage(FileService fileService, IDialogService dialogService)
    {
        try
        {
            InitializeComponent();
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            
            Debug.WriteLine("CompareMixPage constructor completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in CompareMixPage constructor: {ex.Message}");
            throw;
        }
    }

    public async Task InitializeWithCurrentMix(List<FertilizerQuantity> currentMixIngredients, bool useImperialUnits)
    {
        try
        {
            Debug.WriteLine("InitializeWithCurrentMix called");
            await LoadAndInitializeAsync(currentMixIngredients, useImperialUnits);
            Debug.WriteLine("InitializeWithCurrentMix completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in InitializeWithCurrentMix: {ex.Message}");
            await _dialogService.DisplayAlertAsync("Error", $"Failed to initialize Compare Mix page: {ex.Message}", "OK");
            throw; // Rethrow to let the caller handle it
        }
    }

    public async Task InitializeWithMixes(FertilizerMix mix1, FertilizerMix mix2, List<Fertilizer> availableFertilizers, bool useImperialUnits)
    {
        try
        {
            Debug.WriteLine("InitializeWithMixes called");
            
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
            
            Debug.WriteLine("InitializeWithMixes completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in InitializeWithMixes: {ex.Message}");
            await _dialogService.DisplayAlertAsync("Error", $"Failed to initialize Compare Mix page: {ex.Message}", "OK");
            throw; // Rethrow to let the caller handle it
        }
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
                
                // Subscribe to property changed events
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                
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

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // When chart data or selections change, try to create the chart
        if (e.PropertyName == nameof(CompareMixViewModel.HasChartData) ||
            e.PropertyName == nameof(CompareMixViewModel.NutrientSeries) ||
            e.PropertyName == nameof(CompareMixViewModel.SelectedMix1) ||
            e.PropertyName == nameof(CompareMixViewModel.SelectedMix2))
        {
            try
            {
                Debug.WriteLine($"Property changed: {e.PropertyName}");
                
                // Attempt to create chart on a delay to ensure UI is ready
                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    try
                    {
                        await Task.Delay(300); // Give UI time to initialize
                        CreateChartSafely();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error creating chart on main thread: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in property changed handler: {ex.Message}");
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("CompareMixPage.OnAppearing called");
        
        // Try to create the chart when the page appears
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            try
            {
                await Task.Delay(500); // Give UI time to initialize
                CreateChartSafely();
                Debug.WriteLine("Chart creation attempted from OnAppearing");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating chart from OnAppearing: {ex.Message}");
            }
        });
    }

    private void CreateChartSafely()
    {
        try
        {
            // Clear any existing chart
            ChartContainer.Children.Clear();
            
            // Only create chart if we have data
            if (ViewModel.HasChartData && ViewModel.NutrientSeries != null)
            {
                try
                {
                    // Create the chart programmatically
                    var chart = new LiveChartsCore.SkiaSharpView.Maui.CartesianChart
                    {
                        Series = ViewModel.NutrientSeries,
                        XAxes = ViewModel.XAxes,
                        YAxes = ViewModel.YAxes,
                        LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom,
                        BackgroundColor = Colors.Black
                    };
                    
                    // Add the chart to the container
                    ChartContainer.Children.Add(chart);
                    Debug.WriteLine("Chart created and added to container");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating CartesianChart: {ex.Message}");
                    
                    // Show a placeholder label instead
                    var label = new Label
                    {
                        Text = "Chart data is available but chart rendering failed",
                        TextColor = Colors.LightGray,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    };
                    
                    ChartContainer.Children.Add(label);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating chart safely: {ex.Message}");
        }
    }
}
