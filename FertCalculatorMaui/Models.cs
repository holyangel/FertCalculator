using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;

namespace FertCalculatorMaui
{
    [Serializable]
    public class Fertilizer : INotifyPropertyChanged
    {
        private string name = string.Empty;
        private double nitrogenPercent;
        private double phosphorusPercent;
        private double potassiumPercent;
        private double calciumPercent;
        private double magnesiumPercent;
        private double sulfurPercent;
        private double boronPercent;
        private double copperPercent;
        private double ironPercent;
        private double manganesePercent;
        private double molybdenumPercent;
        private double zincPercent;
        private double chlorinePercent;
        private double silicaPercent;
        private double humicAcidPercent;
        private double fulvicAcidPercent;
        
        // Constants for unit conversion
        private const double GALLON_TO_LITER = 3.78541;

        // Properties to store original input format and values
        public bool IsPhosphorusInOxideForm { get; set; }
        public bool IsPotassiumInOxideForm { get; set; }
        public double OriginalPhosphorusValue { get; set; }
        public double OriginalPotassiumValue { get; set; }

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        // Store as percentages (0-100)
        public double NitrogenPercent
        {
            get => nitrogenPercent;
            set { nitrogenPercent = value; OnPropertyChanged(nameof(NitrogenPercent)); }
        }

        public double PhosphorusPercent
        {
            get => phosphorusPercent;
            set { phosphorusPercent = value; OnPropertyChanged(nameof(PhosphorusPercent)); }
        }

        public double PotassiumPercent
        {
            get => potassiumPercent;
            set { potassiumPercent = value; OnPropertyChanged(nameof(PotassiumPercent)); }
        }

        public double CalciumPercent
        {
            get => calciumPercent;
            set { calciumPercent = value; OnPropertyChanged(nameof(CalciumPercent)); }
        }

        public double MagnesiumPercent
        {
            get => magnesiumPercent;
            set { magnesiumPercent = value; OnPropertyChanged(nameof(MagnesiumPercent)); }
        }

        public double SulfurPercent
        {
            get => sulfurPercent;
            set { sulfurPercent = value; OnPropertyChanged(nameof(SulfurPercent)); }
        }

        public double BoronPercent
        {
            get => boronPercent;
            set { boronPercent = value; OnPropertyChanged(nameof(BoronPercent)); }
        }

        public double CopperPercent
        {
            get => copperPercent;
            set { copperPercent = value; OnPropertyChanged(nameof(CopperPercent)); }
        }

        public double IronPercent
        {
            get => ironPercent;
            set { ironPercent = value; OnPropertyChanged(nameof(IronPercent)); }
        }

        public double ManganesePercent
        {
            get => manganesePercent;
            set { manganesePercent = value; OnPropertyChanged(nameof(ManganesePercent)); }
        }

        public double MolybdenumPercent
        {
            get => molybdenumPercent;
            set { molybdenumPercent = value; OnPropertyChanged(nameof(MolybdenumPercent)); }
        }

        public double ZincPercent
        {
            get => zincPercent;
            set { zincPercent = value; OnPropertyChanged(nameof(ZincPercent)); }
        }

        public double ChlorinePercent
        {
            get => chlorinePercent;
            set { chlorinePercent = value; OnPropertyChanged(nameof(ChlorinePercent)); }
        }

        public double SilicaPercent
        {
            get => silicaPercent;
            set { silicaPercent = value; OnPropertyChanged(nameof(SilicaPercent)); }
        }

        public double HumicAcidPercent
        {
            get => humicAcidPercent;
            set { humicAcidPercent = value; OnPropertyChanged(nameof(HumicAcidPercent)); }
        }

        public double FulvicAcidPercent
        {
            get => fulvicAcidPercent;
            set { fulvicAcidPercent = value; OnPropertyChanged(nameof(FulvicAcidPercent)); }
        }

        // PPM calculation with support for metric/imperial units
        private double GetPpmValue(double percentValue, bool useImperial = false)
        {
            double basePpm = (percentValue / 100) * 1000;
            return useImperial ? basePpm / GALLON_TO_LITER : basePpm;
        }

        // Properties that calculate PPM values with unit conversion
        public double NitrogenPpm(bool useImperial = false) => GetPpmValue(NitrogenPercent, useImperial);
        public double PhosphorusPpm(bool useImperial = false) => GetPpmValue(PhosphorusPercent, useImperial);
        public double PotassiumPpm(bool useImperial = false) => GetPpmValue(PotassiumPercent, useImperial);
        public double CalciumPpm(bool useImperial = false) => GetPpmValue(CalciumPercent, useImperial);
        public double MagnesiumPpm(bool useImperial = false) => GetPpmValue(MagnesiumPercent, useImperial);
        public double SulfurPpm(bool useImperial = false) => GetPpmValue(SulfurPercent, useImperial);
        public double BoronPpm(bool useImperial = false) => GetPpmValue(BoronPercent, useImperial);
        public double CopperPpm(bool useImperial = false) => GetPpmValue(CopperPercent, useImperial);
        public double IronPpm(bool useImperial = false) => GetPpmValue(IronPercent, useImperial);
        public double ManganesePpm(bool useImperial = false) => GetPpmValue(ManganesePercent, useImperial);
        public double MolybdenumPpm(bool useImperial = false) => GetPpmValue(MolybdenumPercent, useImperial);
        public double ZincPpm(bool useImperial = false) => GetPpmValue(ZincPercent, useImperial);
        public double ChlorinePpm(bool useImperial = false) => GetPpmValue(ChlorinePercent, useImperial);
        public double SilicaPpm(bool useImperial = false) => GetPpmValue(SilicaPercent, useImperial);
        public double HumicAcidPpm(bool useImperial = false) => GetPpmValue(HumicAcidPercent, useImperial);
        public double FulvicAcidPpm(bool useImperial = false) => GetPpmValue(FulvicAcidPercent, useImperial);

        // Total nutrient calculations
        public double TotalNutrientPercent => NitrogenPercent + PhosphorusPercent + PotassiumPercent + CalciumPercent + MagnesiumPercent + SulfurPercent + BoronPercent + CopperPercent + IronPercent + ManganesePercent + MolybdenumPercent + ZincPercent + ChlorinePercent + SilicaPercent + HumicAcidPercent + FulvicAcidPercent;
        public double TotalNutrientPpm(bool useImperial = false) => GetPpmValue(TotalNutrientPercent, useImperial);

        // Conversion helpers for P2O5 and K2O
        public static double P2O5ToP(double p2o5Value) => p2o5Value * 0.4364;
        public static double K2OToK(double k2oValue) => k2oValue * 0.8301;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable]
    public class FertilizerQuantity : INotifyPropertyChanged
    {
        private string fertilizerName = string.Empty;
        private double quantity;

        public string FertilizerName
        {
            get => fertilizerName;
            set { fertilizerName = value; OnPropertyChanged(nameof(FertilizerName)); }
        }

        public double Quantity
        {
            get => quantity;
            set { quantity = value; OnPropertyChanged(nameof(Quantity)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable]
    public class FertilizerMix
    {
        public string Name { get; set; } = string.Empty;
        public List<FertilizerQuantity> Ingredients { get; set; } = new List<FertilizerQuantity>();
    }

    // Helper class for application settings
    public class AppSettings : INotifyPropertyChanged
    {
        private const string UNIT_PREFERENCE_KEY = "UseImperialUnits";
        private bool useImperialUnits;
        
        public AppSettings()
        {
            // Load settings from preferences
            try
            {
                UseImperialUnits = Preferences.Default.Get(UNIT_PREFERENCE_KEY, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                // Use default value if there's an error
                UseImperialUnits = false;
            }
        }
        
        public bool UseImperialUnits
        {
            get => useImperialUnits;
            set 
            { 
                if (useImperialUnits != value)
                {
                    useImperialUnits = value; 
                    OnPropertyChanged(nameof(UseImperialUnits));
                    OnPropertyChanged(nameof(UnitLabel));
                    
                    // Save the setting when it changes
                    try
                    {
                        Preferences.Default.Set(UNIT_PREFERENCE_KEY, value);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                    }
                }
            }
        }
        
        public string UnitLabel => UseImperialUnits ? "PPM per gallon" : "PPM per liter";
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
