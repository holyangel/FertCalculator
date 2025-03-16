using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FertCalculatorMaui
{
    public partial class CompareMixViewModel : ObservableObject
    {
        private const double GALLON_TO_LITER = 3.78541;

        // Event that the page can subscribe to for handling navigation
        public event EventHandler CloseRequested;

        [ObservableProperty]
        private ObservableCollection<FertilizerMix> availableMixes;

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

        // Command for toggling between metric and imperial units
        [RelayCommand]
        private void ToggleUnit()
        {
            UseImperialUnits = !UseImperialUnits;
        }

        public Color UnitButtonColor => UseImperialUnits ? Colors.DarkOrange : Colors.DarkGreen;

        public CompareMixViewModel()
        {
            AvailableMixes = new ObservableCollection<FertilizerMix>();
            Name = "Compare Mixes";
            UpdateUnitDisplay();
        }

        public void UpdateUnitDisplay()
        {
            // Update the units type label
            UnitsTypeLabel = UseImperialUnits ? "Imperial (per gallon)" : "Metric (per liter)";
            
            // Update PPM header label
            PpmHeaderLabel = UseImperialUnits ? "PPM (per gallon)" : "PPM (per liter)";
        }

        partial void OnSelectedMix1Changed(FertilizerMix value)
        {
            if (value != null)
            {
                Mix1IngredientCount = value.Ingredients?.Count ?? 0;
                Mix1Name = value.Name;
                CalculateMix1Nutrients();
            }
            else
            {
                Mix1IngredientCount = 0;
                Mix1Name = string.Empty;
                ClearMix1Nutrients();
            }
        }

        partial void OnSelectedMix2Changed(FertilizerMix value)
        {
            if (value != null)
            {
                Mix2IngredientCount = value.Ingredients?.Count ?? 0;
                Mix2Name = value.Name;
                CalculateMix2Nutrients();
            }
            else
            {
                Mix2IngredientCount = 0;
                Mix2Name = string.Empty;
                ClearMix2Nutrients();
            }
        }

        partial void OnUseImperialUnitsChanged(bool value)
        {
            UpdateUnitDisplay();
            CalculateMix1Nutrients();
            CalculateMix2Nutrients();
        }

        private void CalculateMix1Nutrients()
        {
            // This would be implemented to calculate nutrient values for Mix1
            // based on the selected mix and unit settings
        }

        private void CalculateMix2Nutrients()
        {
            // This would be implemented to calculate nutrient values for Mix2
            // based on the selected mix and unit settings
        }

        private void ClearMix1Nutrients()
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
        }

        private void ClearMix2Nutrients()
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
