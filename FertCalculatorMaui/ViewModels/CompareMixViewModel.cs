using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FertCalculatorMaui.ViewModels
{
    public partial class CompareMixViewModel : ObservableObject
    {
        private const double GALLON_TO_LITER = 3.78541;

        // Event that the page can subscribe to for handling navigation
        public event EventHandler CloseRequested;

        [ObservableProperty]
        private ObservableCollection<FertilizerMix> availableMixes = new();

        [ObservableProperty]
        private FertilizerMix selectedMix1;

        [ObservableProperty]
        private FertilizerMix selectedMix2;

        [ObservableProperty]
        private int mix1IngredientCount;

        [ObservableProperty]
        private int mix2IngredientCount;

        [ObservableProperty]
        private string mix1Name;

        [ObservableProperty]
        private string mix2Name;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private bool useImperialUnits;

        [ObservableProperty]
        private string unitsTypeLabel;

        [ObservableProperty]
        private string ppmHeaderLabel;

        [ObservableProperty]
        private Color unitButtonColor = Colors.DodgerBlue;

        // Mix 1 (Left side) nutrients
        [ObservableProperty]
        private double mix1NitrogenPpm;

        [ObservableProperty]
        private double mix1PhosphorusPpm;

        [ObservableProperty]
        private double mix1PotassiumPpm;

        [ObservableProperty]
        private double mix1CalciumPpm;

        [ObservableProperty]
        private double mix1MagnesiumPpm;

        [ObservableProperty]
        private double mix1SulfurPpm;

        [ObservableProperty]
        private double mix1BoronPpm;

        [ObservableProperty]
        private double mix1CopperPpm;

        [ObservableProperty]
        private double mix1IronPpm;

        [ObservableProperty]
        private double mix1ManganesePpm;

        [ObservableProperty]
        private double mix1MolybdenumPpm;

        [ObservableProperty]
        private double mix1ZincPpm;

        [ObservableProperty]
        private double mix1ChlorinePpm;

        [ObservableProperty]
        private double mix1SilicaPpm;

        [ObservableProperty]
        private double mix1HumicAcidPpm;

        [ObservableProperty]
        private double mix1FulvicAcidPpm;

        [ObservableProperty]
        private double mix1TotalNutrientPpm;

        // Mix 2 (Right side) nutrients
        [ObservableProperty]
        private double mix2NitrogenPpm;

        [ObservableProperty]
        private double mix2PhosphorusPpm;

        [ObservableProperty]
        private double mix2PotassiumPpm;

        [ObservableProperty]
        private double mix2CalciumPpm;

        [ObservableProperty]
        private double mix2MagnesiumPpm;

        [ObservableProperty]
        private double mix2SulfurPpm;

        [ObservableProperty]
        private double mix2BoronPpm;

        [ObservableProperty]
        private double mix2CopperPpm;

        [ObservableProperty]
        private double mix2IronPpm;

        [ObservableProperty]
        private double mix2ManganesePpm;

        [ObservableProperty]
        private double mix2MolybdenumPpm;

        [ObservableProperty]
        private double mix2ZincPpm;

        [ObservableProperty]
        private double mix2ChlorinePpm;

        [ObservableProperty]
        private double mix2SilicaPpm;

        [ObservableProperty]
        private double mix2HumicAcidPpm;

        [ObservableProperty]
        private double mix2FulvicAcidPpm;

        [ObservableProperty]
        private double mix2TotalNutrientPpm;

        [ObservableProperty]
        private ISeries[] nutrientSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] xAxes = new[] { new Axis { Labels = Array.Empty<string>() } };

        [ObservableProperty]
        private Axis[] yAxes = new[] { new Axis { Name = "PPM" } };

        [ObservableProperty]
        private bool hasChartData;

        [ObservableProperty]
        private SolidColorPaint mix1Color = new(SKColors.DodgerBlue);

        [ObservableProperty]
        private SolidColorPaint mix2Color = new(SKColors.Orange);

        [ObservableProperty]
        private SolidColorPaint separatorColor = new(SKColors.LightGray) { StrokeThickness = 1 };

        [ObservableProperty]
        private ObservableCollection<Fertilizer> availableFertilizers = new();

        // Command for toggling between metric and imperial units
        [RelayCommand]
        private void ToggleUnit()
        {
            UseImperialUnits = !UseImperialUnits;
            UpdateUnitLabels();
            CalculateMix1Nutrients();
            CalculateMix2Nutrients();
            UpdateChartData();
        }

        private void UpdateUnitLabels()
        {
            UnitsTypeLabel = UseImperialUnits ? "Imperial Units" : "Metric Units";
            PpmHeaderLabel = UseImperialUnits ? "(PPM per Gallon)" : "(PPM per Liter)";
            UnitButtonColor = UseImperialUnits ? Colors.Orange : Colors.DodgerBlue;
        }

        public CompareMixViewModel(IEnumerable<Fertilizer> fertilizers, IEnumerable<FertilizerMix> mixes)
        {
            AvailableFertilizers = new ObservableCollection<Fertilizer>(fertilizers);
            AvailableMixes = new ObservableCollection<FertilizerMix>(mixes);
            Name = "Compare Mixes";
            UpdateUnitLabels();
        }

        public void UpdateUnitDisplay()
        {
            UpdateUnitLabels();
            CalculateMix1Nutrients();
            CalculateMix2Nutrients();
            UpdateChartData();
        }

        partial void OnSelectedMix1Changed(FertilizerMix value)
        {
            Mix1Name = value?.Name ?? "Mix 1";
            Mix1IngredientCount = value?.Ingredients?.Count ?? 0;
            CalculateMix1Nutrients();
            UpdateChartData();
        }

        partial void OnSelectedMix2Changed(FertilizerMix value)
        {
            Mix2Name = value?.Name ?? "Mix 2";
            Mix2IngredientCount = value?.Ingredients?.Count ?? 0;
            CalculateMix2Nutrients();
            UpdateChartData();
        }

        partial void OnUseImperialUnitsChanged(bool value)
        {
            UpdateUnitLabels();
            CalculateMix1Nutrients();
            CalculateMix2Nutrients();
            UpdateChartData();
        }

        partial void OnAvailableMixesChanged(ObservableCollection<FertilizerMix> value)
        {
            // When mixes are loaded, recalculate nutrients and update chart if we have selections
            if (SelectedMix1 != null)
            {
                CalculateMix1Nutrients();
            }
            
            if (SelectedMix2 != null)
            {
                CalculateMix2Nutrients();
            }
            
            if (SelectedMix1 != null && SelectedMix2 != null)
            {
                UpdateChartData();
            }
        }

        private void CalculateMix1Nutrients()
        {
            if (SelectedMix1?.Ingredients == null)
            {
                ResetMix1Nutrients();
                return;
            }

            // Reset all nutrient values
            ResetMix1Nutrients();

            foreach (var ingredient in SelectedMix1.Ingredients)
            {
                // Find the fertilizer by name
                var fertilizer = AvailableFertilizers.FirstOrDefault(f => f.Name == ingredient.FertilizerName);
                if (fertilizer == null) continue;

                // Get the base PPM values (per liter)
                double nitrogenPpm = fertilizer.NitrogenPpm() * ingredient.Quantity;
                double phosphorusPpm = fertilizer.PhosphorusPpm() * ingredient.Quantity;
                double potassiumPpm = fertilizer.PotassiumPpm() * ingredient.Quantity;
                double calciumPpm = fertilizer.CalciumPpm() * ingredient.Quantity;
                double magnesiumPpm = fertilizer.MagnesiumPpm() * ingredient.Quantity;
                double sulfurPpm = fertilizer.SulfurPpm() * ingredient.Quantity;
                double boronPpm = fertilizer.BoronPpm() * ingredient.Quantity;
                double copperPpm = fertilizer.CopperPpm() * ingredient.Quantity;
                double ironPpm = fertilizer.IronPpm() * ingredient.Quantity;
                double manganesePpm = fertilizer.ManganesePpm() * ingredient.Quantity;
                double molybdenumPpm = fertilizer.MolybdenumPpm() * ingredient.Quantity;
                double zincPpm = fertilizer.ZincPpm() * ingredient.Quantity;
                double chlorinePpm = fertilizer.ChlorinePpm() * ingredient.Quantity;
                double silicaPpm = fertilizer.SilicaPpm() * ingredient.Quantity;
                double humicAcidPpm = fertilizer.HumicAcidPpm() * ingredient.Quantity;
                double fulvicAcidPpm = fertilizer.FulvicAcidPpm() * ingredient.Quantity;

                // Convert to imperial if needed (divide by GALLON_TO_LITER to convert from per liter to per gallon)
                double conversionFactor = UseImperialUnits ? 1.0 / GALLON_TO_LITER : 1.0;
                Mix1NitrogenPpm += nitrogenPpm * conversionFactor;
                Mix1PhosphorusPpm += phosphorusPpm * conversionFactor;
                Mix1PotassiumPpm += potassiumPpm * conversionFactor;
                Mix1CalciumPpm += calciumPpm * conversionFactor;
                Mix1MagnesiumPpm += magnesiumPpm * conversionFactor;
                Mix1SulfurPpm += sulfurPpm * conversionFactor;
                Mix1BoronPpm += boronPpm * conversionFactor;
                Mix1CopperPpm += copperPpm * conversionFactor;
                Mix1IronPpm += ironPpm * conversionFactor;
                Mix1ManganesePpm += manganesePpm * conversionFactor;
                Mix1MolybdenumPpm += molybdenumPpm * conversionFactor;
                Mix1ZincPpm += zincPpm * conversionFactor;
                Mix1ChlorinePpm += chlorinePpm * conversionFactor;
                Mix1SilicaPpm += silicaPpm * conversionFactor;
                Mix1HumicAcidPpm += humicAcidPpm * conversionFactor;
                Mix1FulvicAcidPpm += fulvicAcidPpm * conversionFactor;
            }

            // Calculate total nutrients
            Mix1TotalNutrientPpm = Mix1NitrogenPpm + Mix1PhosphorusPpm + Mix1PotassiumPpm + Mix1CalciumPpm + 
                Mix1MagnesiumPpm + Mix1SulfurPpm + Mix1BoronPpm + Mix1CopperPpm + Mix1IronPpm + 
                Mix1ManganesePpm + Mix1MolybdenumPpm + Mix1ZincPpm + Mix1ChlorinePpm + Mix1SilicaPpm + 
                Mix1HumicAcidPpm + Mix1FulvicAcidPpm;
        }

        private void CalculateMix2Nutrients()
        {
            if (SelectedMix2?.Ingredients == null)
            {
                ResetMix2Nutrients();
                return;
            }

            // Reset all nutrient values
            ResetMix2Nutrients();

            foreach (var ingredient in SelectedMix2.Ingredients)
            {
                // Find the fertilizer by name
                var fertilizer = AvailableFertilizers.FirstOrDefault(f => f.Name == ingredient.FertilizerName);
                if (fertilizer == null) continue;

                // Get the base PPM values (per liter)
                double nitrogenPpm = fertilizer.NitrogenPpm() * ingredient.Quantity;
                double phosphorusPpm = fertilizer.PhosphorusPpm() * ingredient.Quantity;
                double potassiumPpm = fertilizer.PotassiumPpm() * ingredient.Quantity;
                double calciumPpm = fertilizer.CalciumPpm() * ingredient.Quantity;
                double magnesiumPpm = fertilizer.MagnesiumPpm() * ingredient.Quantity;
                double sulfurPpm = fertilizer.SulfurPpm() * ingredient.Quantity;
                double boronPpm = fertilizer.BoronPpm() * ingredient.Quantity;
                double copperPpm = fertilizer.CopperPpm() * ingredient.Quantity;
                double ironPpm = fertilizer.IronPpm() * ingredient.Quantity;
                double manganesePpm = fertilizer.ManganesePpm() * ingredient.Quantity;
                double molybdenumPpm = fertilizer.MolybdenumPpm() * ingredient.Quantity;
                double zincPpm = fertilizer.ZincPpm() * ingredient.Quantity;
                double chlorinePpm = fertilizer.ChlorinePpm() * ingredient.Quantity;
                double silicaPpm = fertilizer.SilicaPpm() * ingredient.Quantity;
                double humicAcidPpm = fertilizer.HumicAcidPpm() * ingredient.Quantity;
                double fulvicAcidPpm = fertilizer.FulvicAcidPpm() * ingredient.Quantity;

                // Convert to imperial if needed (divide by GALLON_TO_LITER to convert from per liter to per gallon)
                double conversionFactor = UseImperialUnits ? 1.0 / GALLON_TO_LITER : 1.0;
                Mix2NitrogenPpm += nitrogenPpm * conversionFactor;
                Mix2PhosphorusPpm += phosphorusPpm * conversionFactor;
                Mix2PotassiumPpm += potassiumPpm * conversionFactor;
                Mix2CalciumPpm += calciumPpm * conversionFactor;
                Mix2MagnesiumPpm += magnesiumPpm * conversionFactor;
                Mix2SulfurPpm += sulfurPpm * conversionFactor;
                Mix2BoronPpm += boronPpm * conversionFactor;
                Mix2CopperPpm += copperPpm * conversionFactor;
                Mix2IronPpm += ironPpm * conversionFactor;
                Mix2ManganesePpm += manganesePpm * conversionFactor;
                Mix2MolybdenumPpm += molybdenumPpm * conversionFactor;
                Mix2ZincPpm += zincPpm * conversionFactor;
                Mix2ChlorinePpm += chlorinePpm * conversionFactor;
                Mix2SilicaPpm += silicaPpm * conversionFactor;
                Mix2HumicAcidPpm += humicAcidPpm * conversionFactor;
                Mix2FulvicAcidPpm += fulvicAcidPpm * conversionFactor;
            }

            // Calculate total nutrients
            Mix2TotalNutrientPpm = Mix2NitrogenPpm + Mix2PhosphorusPpm + Mix2PotassiumPpm + Mix2CalciumPpm + 
                Mix2MagnesiumPpm + Mix2SulfurPpm + Mix2BoronPpm + Mix2CopperPpm + Mix2IronPpm + 
                Mix2ManganesePpm + Mix2MolybdenumPpm + Mix2ZincPpm + Mix2ChlorinePpm + Mix2SilicaPpm + 
                Mix2HumicAcidPpm + Mix2FulvicAcidPpm;
        }

        private void ResetMix1Nutrients()
        {
            Mix1NitrogenPpm = 0;
            Mix1PhosphorusPpm = 0;
            Mix1PotassiumPpm = 0;
            Mix1CalciumPpm = 0;
            Mix1MagnesiumPpm = 0;
            Mix1SulfurPpm = 0;
            Mix1BoronPpm = 0;
            Mix1CopperPpm = 0;
            Mix1IronPpm = 0;
            Mix1ManganesePpm = 0;
            Mix1MolybdenumPpm = 0;
            Mix1ZincPpm = 0;
            Mix1ChlorinePpm = 0;
            Mix1SilicaPpm = 0;
            Mix1HumicAcidPpm = 0;
            Mix1FulvicAcidPpm = 0;
            Mix1TotalNutrientPpm = 0;
        }

        private void ResetMix2Nutrients()
        {
            Mix2NitrogenPpm = 0;
            Mix2PhosphorusPpm = 0;
            Mix2PotassiumPpm = 0;
            Mix2CalciumPpm = 0;
            Mix2MagnesiumPpm = 0;
            Mix2SulfurPpm = 0;
            Mix2BoronPpm = 0;
            Mix2CopperPpm = 0;
            Mix2IronPpm = 0;
            Mix2ManganesePpm = 0;
            Mix2MolybdenumPpm = 0;
            Mix2ZincPpm = 0;
            Mix2ChlorinePpm = 0;
            Mix2SilicaPpm = 0;
            Mix2HumicAcidPpm = 0;
            Mix2FulvicAcidPpm = 0;
            Mix2TotalNutrientPpm = 0;
        }

        private void UpdateChartData()
        {
            if (SelectedMix1 == null || SelectedMix2 == null)
            {
                HasChartData = false;
                NutrientSeries = Array.Empty<ISeries>();
                XAxes = new[] { new Axis { Labels = Array.Empty<string>() } };
                YAxes = new[] { new Axis { Name = "" } };
                return;
            }

            var mix1Values = new List<ObservableValue>();
            var mix2Values = new List<ObservableValue>();
            var labels = new List<string>();

            // Add values for each nutrient
            AddNutrientData("N", Mix1NitrogenPpm, Mix2NitrogenPpm, mix1Values, mix2Values, labels);
            AddNutrientData("P", Mix1PhosphorusPpm, Mix2PhosphorusPpm, mix1Values, mix2Values, labels);
            AddNutrientData("K", Mix1PotassiumPpm, Mix2PotassiumPpm, mix1Values, mix2Values, labels);
            AddNutrientData("Ca", Mix1CalciumPpm, Mix2CalciumPpm, mix1Values, mix2Values, labels);
            AddNutrientData("Mg", Mix1MagnesiumPpm, Mix2MagnesiumPpm, mix1Values, mix2Values, labels);
            AddNutrientData("S", Mix1SulfurPpm, Mix2SulfurPpm, mix1Values, mix2Values, labels);
            AddNutrientData("B", Mix1BoronPpm, Mix2BoronPpm, mix1Values, mix2Values, labels);
            AddNutrientData("Cu", Mix1CopperPpm, Mix2CopperPpm, mix1Values, mix2Values, labels);
            AddNutrientData("Fe", Mix1IronPpm, Mix2IronPpm, mix1Values, mix2Values, labels);
            AddNutrientData("Mn", Mix1ManganesePpm, Mix2ManganesePpm, mix1Values, mix2Values, labels);
            AddNutrientData("Mo", Mix1MolybdenumPpm, Mix2MolybdenumPpm, mix1Values, mix2Values, labels);
            AddNutrientData("Zn", Mix1ZincPpm, Mix2ZincPpm, mix1Values, mix2Values, labels);
            AddNutrientData("Cl", Mix1ChlorinePpm, Mix2ChlorinePpm, mix1Values, mix2Values, labels);
            AddNutrientData("Si", Mix1SilicaPpm, Mix2SilicaPpm, mix1Values, mix2Values, labels);
            AddNutrientData("Humic", Mix1HumicAcidPpm, Mix2HumicAcidPpm, mix1Values, mix2Values, labels);
            AddNutrientData("Fulvic", Mix1FulvicAcidPpm, Mix2FulvicAcidPpm, mix1Values, mix2Values, labels);

            if (mix1Values.Count == 0)
            {
                HasChartData = false;
                NutrientSeries = Array.Empty<ISeries>();
                XAxes = new[] { new Axis { Labels = Array.Empty<string>() } };
                YAxes = new[] { new Axis { Name = "" } };
                return;
            }

            // Create series for each mix
            NutrientSeries = new ISeries[]
            {
                new LineSeries<ObservableValue>
                {
                    Name = Mix1Name,
                    Values = mix1Values,
                    Fill = null,
                    Stroke = new SolidColorPaint(new SKColor(
                        (byte)(Colors.DodgerBlue.Red * 255), 
                        (byte)(Colors.DodgerBlue.Green * 255), 
                        (byte)(Colors.DodgerBlue.Blue * 255))) 
                    { 
                        StrokeThickness = 4 
                    },
                    GeometrySize = 10,
                    GeometryStroke = new SolidColorPaint(new SKColor(
                        (byte)(Colors.DodgerBlue.Red * 255), 
                        (byte)(Colors.DodgerBlue.Green * 255), 
                        (byte)(Colors.DodgerBlue.Blue * 255))) 
                    { 
                        StrokeThickness = 2 
                    },
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    LineSmoothness = 0.5
                },
                new LineSeries<ObservableValue>
                {
                    Name = Mix2Name,
                    Values = mix2Values,
                    Fill = null,
                    Stroke = new SolidColorPaint(new SKColor(
                        (byte)(Colors.OrangeRed.Red * 255), 
                        (byte)(Colors.OrangeRed.Green * 255), 
                        (byte)(Colors.OrangeRed.Blue * 255))) 
                    { 
                        StrokeThickness = 4 
                    },
                    GeometrySize = 10,
                    GeometryStroke = new SolidColorPaint(new SKColor(
                        (byte)(Colors.OrangeRed.Red * 255), 
                        (byte)(Colors.OrangeRed.Green * 255), 
                        (byte)(Colors.OrangeRed.Blue * 255))) 
                    { 
                        StrokeThickness = 2 
                    },
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    LineSmoothness = 0.5
                }
            };

            // Update axes with proper styling
            XAxes = new[]
            {
                new Axis
                {
                    Labels = labels.ToArray(),
                    LabelsRotation = 45,
                    TextSize = 12,
                    NameTextSize = 14,
                    MinStep = 1,
                    ForceStepToMin = true
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Name = "", 
                    TextSize = 12,
                    NameTextSize = 14,
                    MinStep = 1,
                    Labeler = (value) => value.ToString("0.##")
                }
            };

            HasChartData = true;
        }

        private void AddNutrientData(string label, double mix1Value, double mix2Value,
            List<ObservableValue> mix1Values, List<ObservableValue> mix2Values, List<string> labels)
        {
            // Only add nutrient data if at least one mix has a non-zero value
            if (mix1Value > 0 || mix2Value > 0)
            {
                mix1Values.Add(new ObservableValue(mix1Value));
                mix2Values.Add(new ObservableValue(mix2Value));
                labels.Add(label);
            }
        }

        // Close command implementation
        [RelayCommand]
        private void Close()
        {
            // Raise the event to notify the page that close was requested
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
