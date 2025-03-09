using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FertilizerCalculator
{
    [Serializable]
    public class Fertilizer : INotifyPropertyChanged
    {
        private string name;
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

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        // Store as percentages (0-100)
        public double NitrogenPercent
        {
            get => nitrogenPercent;
            set { nitrogenPercent = value; OnPropertyChanged(nameof(NitrogenPercent)); OnPropertyChanged(nameof(NitrogenPpm)); }
        }

        public double PhosphorusPercent
        {
            get => phosphorusPercent;
            set { phosphorusPercent = value; OnPropertyChanged(nameof(PhosphorusPercent)); OnPropertyChanged(nameof(PhosphorusPpm)); }
        }

        public double PotassiumPercent
        {
            get => potassiumPercent;
            set { potassiumPercent = value; OnPropertyChanged(nameof(PotassiumPercent)); OnPropertyChanged(nameof(PotassiumPpm)); }
        }

        public double CalciumPercent
        {
            get => calciumPercent;
            set { calciumPercent = value; OnPropertyChanged(nameof(CalciumPercent)); OnPropertyChanged(nameof(CalciumPpm)); }
        }

        public double MagnesiumPercent
        {
            get => magnesiumPercent;
            set { magnesiumPercent = value; OnPropertyChanged(nameof(MagnesiumPercent)); OnPropertyChanged(nameof(MagnesiumPpm)); }
        }

        public double SulfurPercent
        {
            get => sulfurPercent;
            set { sulfurPercent = value; OnPropertyChanged(nameof(SulfurPercent)); OnPropertyChanged(nameof(SulfurPpm)); }
        }

        public double BoronPercent
        {
            get => boronPercent;
            set { boronPercent = value; OnPropertyChanged(nameof(BoronPercent)); OnPropertyChanged(nameof(BoronPpm)); }
        }

        public double CopperPercent
        {
            get => copperPercent;
            set { copperPercent = value; OnPropertyChanged(nameof(CopperPercent)); OnPropertyChanged(nameof(CopperPpm)); }
        }

        public double IronPercent
        {
            get => ironPercent;
            set { ironPercent = value; OnPropertyChanged(nameof(IronPercent)); OnPropertyChanged(nameof(IronPpm)); }
        }

        public double ManganesePercent
        {
            get => manganesePercent;
            set { manganesePercent = value; OnPropertyChanged(nameof(ManganesePercent)); OnPropertyChanged(nameof(ManganesePpm)); }
        }

        public double MolybdenumPercent
        {
            get => molybdenumPercent;
            set { molybdenumPercent = value; OnPropertyChanged(nameof(MolybdenumPercent)); OnPropertyChanged(nameof(MolybdenumPpm)); }
        }

        public double ZincPercent
        {
            get => zincPercent;
            set { zincPercent = value; OnPropertyChanged(nameof(ZincPercent)); OnPropertyChanged(nameof(ZincPpm)); }
        }

        public double ChlorinePercent
        {
            get => chlorinePercent;
            set { chlorinePercent = value; OnPropertyChanged(nameof(ChlorinePercent)); OnPropertyChanged(nameof(ChlorinePpm)); }
        }

        public double SilicaPercent
        {
            get => silicaPercent;
            set { silicaPercent = value; OnPropertyChanged(nameof(SilicaPercent)); OnPropertyChanged(nameof(SilicaPpm)); }
        }

        public double HumicAcidPercent
        {
            get => humicAcidPercent;
            set { humicAcidPercent = value; OnPropertyChanged(nameof(HumicAcidPercent)); OnPropertyChanged(nameof(HumicAcidPpm)); }
        }

        public double FulvicAcidPercent
        {
            get => fulvicAcidPercent;
            set { fulvicAcidPercent = value; OnPropertyChanged(nameof(FulvicAcidPercent)); OnPropertyChanged(nameof(FulvicAcidPpm)); }
        }

        // Convert percentage to decimal (divide by 100) and multiply by 1000 to get mg/L (PPM)
        public double NitrogenPpm => (NitrogenPercent / 100) * 1000;
        public double PhosphorusPpm => (PhosphorusPercent / 100) * 1000;
        public double PotassiumPpm => (PotassiumPercent / 100) * 1000;
        public double CalciumPpm => (CalciumPercent / 100) * 1000;
        public double MagnesiumPpm => (MagnesiumPercent / 100) * 1000;
        public double SulfurPpm => (SulfurPercent / 100) * 1000;
        public double BoronPpm => (BoronPercent / 100) * 1000;
        public double CopperPpm => (CopperPercent / 100) * 1000;
        public double IronPpm => (IronPercent / 100) * 1000;
        public double ManganesePpm => (ManganesePercent / 100) * 1000;
        public double MolybdenumPpm => (MolybdenumPercent / 100) * 1000;
        public double ZincPpm => (ZincPercent / 100) * 1000;
        public double ChlorinePpm => (ChlorinePercent / 100) * 1000;
        public double SilicaPpm => (SilicaPercent / 100) * 1000;
        public double HumicAcidPpm => (HumicAcidPercent / 100) * 1000;
        public double FulvicAcidPpm => (FulvicAcidPercent / 100) * 1000;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable]
    public class FertilizerQuantity : INotifyPropertyChanged
    {
        private string fertilizerName;
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
        public string Name { get; set; }
        public List<FertilizerQuantity> Ingredients { get; set; } = new List<FertilizerQuantity>();
    }

    [Serializable]
    public class ExportData
    {
        public List<Fertilizer> Fertilizers { get; set; } = new List<Fertilizer>();
        public List<FertilizerMix> Mixes { get; set; } = new List<FertilizerMix>();
    }
}