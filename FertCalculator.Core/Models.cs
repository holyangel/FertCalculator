using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FertCalculator.Core
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

        // Properties to store original input format and values
        public bool IsPhosphorusInOxideForm { get; set; }
        public bool IsPotassiumInOxideForm { get; set; }
        public double OriginalPhosphorusValue { get; set; }
        public double OriginalPotassiumValue { get; set; }

        // Liquid products are dosed in mL, but label percentages are by weight,
        // so the density converts mL -> grams. Defaults keep pre-existing files
        // (which lack these elements) behaving exactly as before.
        public bool IsLiquid { get; set; }
        public double Density { get; set; } = 1.0;

        // Grams of product per mL for liquids; 1.0 for solids / unset density.
        // Read-only, so XmlSerializer ignores it.
        public double EffectiveDensity => IsLiquid && Density > 0 ? Density : 1.0;

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

        // Convert percentage to decimal (divide by 100) and multiply by 1000 to get
        // mg of nutrient per gram of fertilizer (i.e. PPM for a 1 g/L dose).
        // With useImperial, the value is scaled for a dose expressed in g/gal.
        private double GetPpmValue(double percentValue, bool useImperial = false)
        {
            double basePpm = (percentValue / 100) * 1000;
            return useImperial ? basePpm / MixCalculator.GallonToLiter : basePpm;
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
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

        public event PropertyChangedEventHandler PropertyChanged;
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

    [Serializable]
    public class ExportData
    {
        // Increment when the export schema changes. Older readers ignore unknown
        // elements, and older files deserialize with this default.
        public const int CurrentVersion = 1;

        public int Version { get; set; } = CurrentVersion;
        public List<Fertilizer> Fertilizers { get; set; } = new List<Fertilizer>();
        public List<FertilizerMix> Mixes { get; set; } = new List<FertilizerMix>();
    }
}
