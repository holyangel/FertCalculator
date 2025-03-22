using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FertCalculatorMaui.ViewModels;
using FertCalculatorMaui.Models;
using FertCalculatorMaui.Services;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

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
                ViewModel = new CompareMixViewModel(_cachedFertilizers, availableMixes);
                
                // Set binding context
                BindingContext = ViewModel;

                // Subscribe to the CloseRequested event
                ViewModel.CloseRequested += OnCloseRequested;
                
                // Subscribe to property changed events
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                
                // Subscribe to application-wide settings changes
                MessagingCenter.Subscribe<MainViewModel>(this, "UnitSettingChanged", (sender) => 
                {
                    // Update the unit setting in our ViewModel
                    var appSettings = new AppSettings();
                    ViewModel.UseImperialUnits = appSettings.UseImperialUnits;
                    
                    // Recalculate and update chart
                    ViewModel.CalculateMix1Nutrients();
                    ViewModel.CalculateMix2Nutrients();
                    ViewModel.UpdateChartData();
                    
                    // Update the chart
                    CreateChartSafely();
                });
                
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
            e.PropertyName == nameof(CompareMixViewModel.SelectedMix2) ||
            e.PropertyName == nameof(CompareMixViewModel.UseImperialUnits))
        {
            try
            {
                Debug.WriteLine($"Property changed: {e.PropertyName}");
                
                // Only attempt to create chart if we have a valid ViewModel
                if (ViewModel != null)
                {
                    // Attempt to create chart on a delay to ensure UI is ready
                    MainThread.BeginInvokeOnMainThread(async () => 
                    {
                        try
                        {
                            await Task.Delay(300); // Give UI more time to initialize
                            
                            // Ensure data is calculated before creating the chart
                            if (e.PropertyName == nameof(CompareMixViewModel.SelectedMix1) ||
                                e.PropertyName == nameof(CompareMixViewModel.SelectedMix2) ||
                                e.PropertyName == nameof(CompareMixViewModel.UseImperialUnits))
                            {
                                ViewModel.CalculateMix1Nutrients();
                                ViewModel.CalculateMix2Nutrients();
                                ViewModel.UpdateChartData();
                            }
                            
                            CreateChartSafely();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error creating chart on main thread: {ex.Message}");
                        }
                    });
                }
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
        
        // Try to create the chart when the page appears, but with a longer delay
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            try
            {
                // Wait longer to ensure everything is fully loaded
                await Task.Delay(500);
                
                // Only attempt to create chart if we have chart data
                if (ViewModel != null)
                {
                    // Ensure data is calculated
                    ViewModel.CalculateMix1Nutrients();
                    ViewModel.CalculateMix2Nutrients();
                    ViewModel.UpdateChartData();
                    
                    // Create the chart
                    CreateChartSafely();
                    Debug.WriteLine("Chart creation attempted from OnAppearing");
                }
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
            
            if (ViewModel == null || ViewModel.SelectedMix1 == null || ViewModel.SelectedMix2 == null)
            {
                Debug.WriteLine("ViewModel or selected mixes are null, cannot create chart");
                return;
            }
            
            // Prepare data for the chart
            var data = new List<(string Name, double Mix1Value, double Mix2Value)>();
            
            // Add all nutrients to the data
            data.Add(("N", ViewModel.Mix1NitrogenPpm, ViewModel.Mix2NitrogenPpm));
            data.Add(("P", ViewModel.Mix1PhosphorusPpm, ViewModel.Mix2PhosphorusPpm));
            data.Add(("K", ViewModel.Mix1PotassiumPpm, ViewModel.Mix2PotassiumPpm));
            data.Add(("Ca", ViewModel.Mix1CalciumPpm, ViewModel.Mix2CalciumPpm));
            data.Add(("Mg", ViewModel.Mix1MagnesiumPpm, ViewModel.Mix2MagnesiumPpm));
            data.Add(("S", ViewModel.Mix1SulfurPpm, ViewModel.Mix2SulfurPpm));
            data.Add(("B", ViewModel.Mix1BoronPpm, ViewModel.Mix2BoronPpm));
            data.Add(("Cu", ViewModel.Mix1CopperPpm, ViewModel.Mix2CopperPpm));
            data.Add(("Fe", ViewModel.Mix1IronPpm, ViewModel.Mix2IronPpm));
            data.Add(("Mn", ViewModel.Mix1ManganesePpm, ViewModel.Mix2ManganesePpm));
            data.Add(("Mo", ViewModel.Mix1MolybdenumPpm, ViewModel.Mix2MolybdenumPpm));
            data.Add(("Zn", ViewModel.Mix1ZincPpm, ViewModel.Mix2ZincPpm));
            data.Add(("Cl", ViewModel.Mix1ChlorinePpm, ViewModel.Mix2ChlorinePpm));
            data.Add(("Si", ViewModel.Mix1SilicaPpm, ViewModel.Mix2SilicaPpm));
            data.Add(("HuA", ViewModel.Mix1HumicAcidPpm, ViewModel.Mix2HumicAcidPpm));
            data.Add(("FuA", ViewModel.Mix1FulvicAcidPpm, ViewModel.Mix2FulvicAcidPpm));
            
            // Filter out nutrients with zero values in both mixes
            data = data.Where(n => n.Mix1Value > 0 || n.Mix2Value > 0).ToList();
            
            // Find the maximum value for scaling
            double maxValue = data.Max(d => Math.Max(d.Mix1Value, d.Mix2Value));
            
            // Add a 10% margin to the max value for better visualization
            maxValue *= 1.1;
            
            // Create the chart view
            var chartView = new GraphicsView
            {
                Drawable = new LineChartDrawable(
                    data,
                    maxValue,
                    ViewModel.SelectedMix1.Name,
                    ViewModel.SelectedMix2.Name,
                    ViewModel.UseImperialUnits),
                HeightRequest = 300,
                WidthRequest = ChartContainer.Width,
                BackgroundColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            
            // Add the chart to the container
            ChartContainer.Children.Add(chartView);
            
            // Add unit label
            var unitLabel = new Label
            {
                Text = $"Values shown in PPM ({(ViewModel.UseImperialUnits ? "Per Gallon" : "Per Litre")})",
                TextColor = Colors.LightGray,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };
            ChartContainer.Children.Add(unitLabel);
            
            Debug.WriteLine("Line chart created successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating chart: {ex.Message}");
            
            // Show a placeholder label instead
            var label = new Label
            {
                Text = $"Unable to create chart: {ex.Message}",
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            ChartContainer.Children.Add(label);
        }
    }

    private class LineChartDrawable : IDrawable
    {
        private readonly List<(string Name, double Mix1Value, double Mix2Value)> _data;
        private readonly double _maxValue;
        private readonly string _mix1Name;
        private readonly string _mix2Name;
        private readonly bool _useImperialUnits;

        public LineChartDrawable(List<(string Name, double Mix1Value, double Mix2Value)> data, double maxValue, string mix1Name, string mix2Name, bool useImperialUnits)
        {
            _data = data;
            _maxValue = maxValue;
            _mix1Name = mix1Name;
            _mix2Name = mix2Name;
            _useImperialUnits = useImperialUnits;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            try
            {
                // Chart dimensions
                float chartWidth = dirtyRect.Width;
                float chartHeight = dirtyRect.Height * 0.75f; // Leave space for labels
                float chartTop = dirtyRect.Height * 0.1f;
                float chartBottom = chartTop + chartHeight;
                float chartLeft = dirtyRect.Width * 0.08f;
                float chartRight = chartLeft + chartWidth * 0.9f; // Leave space for legend
                
                // Draw chart background
                canvas.FillColor = Colors.Black;
                canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);
                
                // Draw chart border
                canvas.StrokeColor = Colors.Gray;
                canvas.StrokeSize = 1;
                canvas.DrawRectangle(chartLeft, chartTop, chartRight - chartLeft, chartBottom - chartTop);
                
                // Draw horizontal grid lines
                int numGridLines = 5;
                canvas.StrokeColor = Colors.DimGray;
                canvas.StrokeSize = 0.5f;
                canvas.FontSize = 10;
                canvas.FontColor = Colors.LightGray;
                
                for (int i = 0; i <= numGridLines; i++)
                {
                    float y = chartBottom - (i * chartHeight / numGridLines);
                    canvas.DrawLine(chartLeft, y, chartRight, y);
                    
                    // Draw y-axis labels
                    double value = (_maxValue * i) / numGridLines;
                    canvas.DrawString(value.ToString("F1"), chartLeft - 5, y, HorizontalAlignment.Right);
                }
                
                // Calculate x positions for each nutrient
                if (_data.Count == 0) return;
                
                float barWidth = (chartRight - chartLeft) / _data.Count;
                List<PointF> mix1Points = new List<PointF>();
                List<PointF> mix2Points = new List<PointF>();
                
                // Draw x-axis labels and collect points for lines
                for (int i = 0; i < _data.Count; i++)
                {
                    float x = chartLeft + (i * barWidth) + (barWidth / 2);
                    
                    // Calculate y positions based on values
                    float y1 = (float)(chartBottom - (chartHeight * _data[i].Mix1Value / _maxValue));
                    float y2 = (float)(chartBottom - (chartHeight * _data[i].Mix2Value / _maxValue));
                    
                    mix1Points.Add(new PointF(x, y1));
                    mix2Points.Add(new PointF(x, y2));
                    
                    // Draw vertical grid line
                    canvas.StrokeColor = Colors.DimGray;
                    canvas.StrokeSize = 0.5f;
                    canvas.DrawLine(x, chartTop, x, chartBottom);
                    
                    // Draw vertical tick mark for x-axis
                    canvas.StrokeColor = Colors.LightGray;
                    canvas.StrokeSize = 1;
                    canvas.DrawLine(x, chartBottom, x, chartBottom + 5);
                    
                    // Draw x-axis label below the chart
                    canvas.FontColor = Colors.White;
                    canvas.DrawString(_data[i].Name, x, chartBottom + 15, HorizontalAlignment.Center);
                }
                
                // Draw lines connecting points
                canvas.StrokeSize = 2;
                
                // Draw Mix 1 line
                canvas.StrokeColor = Colors.DodgerBlue;
                for (int i = 0; i < mix1Points.Count - 1; i++)
                {
                    canvas.DrawLine(mix1Points[i], mix1Points[i + 1]);
                }
                
                // Draw Mix 2 line
                canvas.StrokeColor = Colors.OrangeRed;
                for (int i = 0; i < mix2Points.Count - 1; i++)
                {
                    canvas.DrawLine(mix2Points[i], mix2Points[i + 1]);
                }
                
                // Draw data points
                float pointRadius = 5;
                
                // Draw Mix 1 points
                canvas.FillColor = Colors.DodgerBlue;
                foreach (var point in mix1Points)
                {
                    canvas.FillCircle(point.X, point.Y, pointRadius);
                }
                
                // Draw Mix 2 points
                canvas.FillColor = Colors.OrangeRed;
                foreach (var point in mix2Points)
                {
                    canvas.FillCircle(point.X, point.Y, pointRadius);
                }
                
                // Draw legend in the upper right corner
                float legendX = chartRight - 100;
                float legendY = chartTop + 20;
                float legendSpacing = 20;
                
                // Mix 1 legend
                canvas.FillColor = Colors.DodgerBlue;
                canvas.FillCircle(legendX, legendY, pointRadius);
                canvas.FontColor = Colors.White;
                canvas.DrawString(_mix1Name, legendX + 10, legendY, HorizontalAlignment.Left);
                
                // Mix 2 legend
                canvas.FillColor = Colors.OrangeRed;
                canvas.FillCircle(legendX, legendY + legendSpacing, pointRadius);
                canvas.FontColor = Colors.White;
                canvas.DrawString(_mix2Name, legendX + 10, legendY + legendSpacing, HorizontalAlignment.Left);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error drawing chart: {ex.Message}");
            }
        }
    }
}
