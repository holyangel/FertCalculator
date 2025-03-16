using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly FileService _fileService;
        private const double GALLON_TO_LITER = 3.78541;

        [ObservableProperty]
        private ObservableCollection<Fertilizer> availableFertilizers;

        [ObservableProperty]
        private ObservableCollection<FertilizerMix> savedMixes;

        [ObservableProperty]
        private ObservableCollection<FertilizerQuantity> currentMix;

        [ObservableProperty]
        private bool useImperialUnits;

        [ObservableProperty]
        private string unitsTypeLabel;

        [ObservableProperty]
        private string ppmHeaderLabel;

        // Nutrient percentage properties
        [ObservableProperty]
        private double nitrogenPercent;

        [ObservableProperty]
        private double phosphorusPercent;

        [ObservableProperty]
        private double potassiumPercent;

        [ObservableProperty]
        private double calciumPercent;

        [ObservableProperty]
        private double magnesiumPercent;

        [ObservableProperty]
        private double sulfurPercent;

        [ObservableProperty]
        private double boronPercent;

        [ObservableProperty]
        private double copperPercent;

        [ObservableProperty]
        private double ironPercent;

        [ObservableProperty]
        private double manganesePercent;

        [ObservableProperty]
        private double molybdenumPercent;

        [ObservableProperty]
        private double zincPercent;

        [ObservableProperty]
        private double chlorinePercent;

        [ObservableProperty]
        private double silicaPercent;

        [ObservableProperty]
        private double humicAcidPercent;

        [ObservableProperty]
        private double fulvicAcidPercent;

        [ObservableProperty]
        private double totalNutrientPercent;

        // Nutrient PPM properties
        [ObservableProperty]
        private double nitrogenPpm;

        [ObservableProperty]
        private double phosphorusPpm;

        [ObservableProperty]
        private double potassiumPpm;

        [ObservableProperty]
        private double calciumPpm;

        [ObservableProperty]
        private double magnesiumPpm;

        [ObservableProperty]
        private double sulfurPpm;

        [ObservableProperty]
        private double boronPpm;

        [ObservableProperty]
        private double copperPpm;

        [ObservableProperty]
        private double ironPpm;

        [ObservableProperty]
        private double manganesePpm;

        [ObservableProperty]
        private double molybdenumPpm;

        [ObservableProperty]
        private double zincPpm;

        [ObservableProperty]
        private double chlorinePpm;

        [ObservableProperty]
        private double silicaPpm;

        [ObservableProperty]
        private double humicAcidPpm;

        [ObservableProperty]
        private double fulvicAcidPpm;

        [ObservableProperty]
        private double totalNutrientPpm;

        public MainPageViewModel(FileService fileService)
        {
            _fileService = fileService ?? new FileService();
            
            // Initialize collections
            AvailableFertilizers = new ObservableCollection<Fertilizer>();
            SavedMixes = new ObservableCollection<FertilizerMix>();
            CurrentMix = new ObservableCollection<FertilizerQuantity>();
            
            // Load settings
            var appSettings = new AppSettings();
            UseImperialUnits = appSettings.UseImperialUnits;
            
            // Initialize unit labels
            UpdateUnitDisplay();
            
            // Register for messages
            WeakReferenceMessenger.Default.Register<AddFertilizerToMixMessage>(this, (r, message) => {
                AddFertilizerToMix(message.Value);
            });
        }

        public void UpdateUnitDisplay()
        {
            // Update the units type label
            UnitsTypeLabel = UseImperialUnits ? "g or ml (per gallon)" : "g or ml (per liter)";
            
            // Update PPM header label
            PpmHeaderLabel = UseImperialUnits ? "PPM (per gallon)" : "PPM (per liter)";
            
            // Save the setting
            var appSettings = new AppSettings();
            appSettings.UseImperialUnits = UseImperialUnits;
        }

        public void UpdateNutrientTotals()
        {
            // Reset all totals
            NitrogenPercent = 0;
            PhosphorusPercent = 0;
            PotassiumPercent = 0;
            CalciumPercent = 0;
            MagnesiumPercent = 0;
            SulfurPercent = 0;
            BoronPercent = 0;
            CopperPercent = 0;
            IronPercent = 0;
            ManganesePercent = 0;
            MolybdenumPercent = 0;
            ZincPercent = 0;
            ChlorinePercent = 0;
            SilicaPercent = 0;
            HumicAcidPercent = 0;
            FulvicAcidPercent = 0;
            TotalNutrientPercent = 0;

            NitrogenPpm = 0;
            PhosphorusPpm = 0;
            PotassiumPpm = 0;
            CalciumPpm = 0;
            MagnesiumPpm = 0;
            SulfurPpm = 0;
            BoronPpm = 0;
            CopperPpm = 0;
            IronPpm = 0;
            ManganesePpm = 0;
            MolybdenumPpm = 0;
            ZincPpm = 0;
            ChlorinePpm = 0;
            SilicaPpm = 0;
            HumicAcidPpm = 0;
            FulvicAcidPpm = 0;
            TotalNutrientPpm = 0;

            double totalQuantity = 0;
            
            // Calculate the contribution of each fertilizer to the total
            foreach (var item in CurrentMix)
            {
                var fertilizer = AvailableFertilizers.FirstOrDefault(f => f.Name == item.FertilizerName);
                if (fertilizer != null)
                {
                    double quantity = item.Quantity;
                    totalQuantity += quantity;
                    
                    // Calculate weighted contribution for percentage display
                    NitrogenPercent += (fertilizer.NitrogenPercent / 100) * quantity;
                    PhosphorusPercent += (fertilizer.PhosphorusPercent / 100) * quantity;
                    PotassiumPercent += (fertilizer.PotassiumPercent / 100) * quantity;
                    CalciumPercent += (fertilizer.CalciumPercent / 100) * quantity;
                    MagnesiumPercent += (fertilizer.MagnesiumPercent / 100) * quantity;
                    SulfurPercent += (fertilizer.SulfurPercent / 100) * quantity;
                    BoronPercent += (fertilizer.BoronPercent / 100) * quantity;
                    CopperPercent += (fertilizer.CopperPercent / 100) * quantity;
                    IronPercent += (fertilizer.IronPercent / 100) * quantity;
                    ManganesePercent += (fertilizer.ManganesePercent / 100) * quantity;
                    MolybdenumPercent += (fertilizer.MolybdenumPercent / 100) * quantity;
                    ZincPercent += (fertilizer.ZincPercent / 100) * quantity;
                    ChlorinePercent += (fertilizer.ChlorinePercent / 100) * quantity;
                    SilicaPercent += (fertilizer.SilicaPercent / 100) * quantity;
                    HumicAcidPercent += (fertilizer.HumicAcidPercent / 100) * quantity;
                    FulvicAcidPercent += (fertilizer.FulvicAcidPercent / 100) * quantity;
                    
                    // Calculate PPM directly from fertilizer PPM values
                    NitrogenPpm += fertilizer.NitrogenPpm(UseImperialUnits) * quantity;
                    PhosphorusPpm += fertilizer.PhosphorusPpm(UseImperialUnits) * quantity;
                    PotassiumPpm += fertilizer.PotassiumPpm(UseImperialUnits) * quantity;
                    CalciumPpm += fertilizer.CalciumPpm(UseImperialUnits) * quantity;
                    MagnesiumPpm += fertilizer.MagnesiumPpm(UseImperialUnits) * quantity;
                    SulfurPpm += fertilizer.SulfurPpm(UseImperialUnits) * quantity;
                    BoronPpm += fertilizer.BoronPpm(UseImperialUnits) * quantity;
                    CopperPpm += fertilizer.CopperPpm(UseImperialUnits) * quantity;
                    IronPpm += fertilizer.IronPpm(UseImperialUnits) * quantity;
                    ManganesePpm += fertilizer.ManganesePpm(UseImperialUnits) * quantity;
                    MolybdenumPpm += fertilizer.MolybdenumPpm(UseImperialUnits) * quantity;
                    ZincPpm += fertilizer.ZincPpm(UseImperialUnits) * quantity;
                    ChlorinePpm += fertilizer.ChlorinePpm(UseImperialUnits) * quantity;
                    SilicaPpm += fertilizer.SilicaPpm(UseImperialUnits) * quantity;
                    HumicAcidPpm += fertilizer.HumicAcidPpm(UseImperialUnits) * quantity;
                    FulvicAcidPpm += fertilizer.FulvicAcidPpm(UseImperialUnits) * quantity;
                }
            }
            
            // Normalize by total quantity
            if (totalQuantity > 0)
            {
                NitrogenPercent = (NitrogenPercent / totalQuantity) * 100;
                PhosphorusPercent = (PhosphorusPercent / totalQuantity) * 100;
                PotassiumPercent = (PotassiumPercent / totalQuantity) * 100;
                CalciumPercent = (CalciumPercent / totalQuantity) * 100;
                MagnesiumPercent = (MagnesiumPercent / totalQuantity) * 100;
                SulfurPercent = (SulfurPercent / totalQuantity) * 100;
                BoronPercent = (BoronPercent / totalQuantity) * 100;
                CopperPercent = (CopperPercent / totalQuantity) * 100;
                IronPercent = (IronPercent / totalQuantity) * 100;
                ManganesePercent = (ManganesePercent / totalQuantity) * 100;
                MolybdenumPercent = (MolybdenumPercent / totalQuantity) * 100;
                ZincPercent = (ZincPercent / totalQuantity) * 100;
                ChlorinePercent = (ChlorinePercent / totalQuantity) * 100;
                SilicaPercent = (SilicaPercent / totalQuantity) * 100;
                HumicAcidPercent = (HumicAcidPercent / totalQuantity) * 100;
                FulvicAcidPercent = (FulvicAcidPercent / totalQuantity) * 100;
            }
            
            // Calculate total nutrient percentage
            TotalNutrientPercent = NitrogenPercent + PhosphorusPercent + PotassiumPercent +
                                  CalciumPercent + MagnesiumPercent + SulfurPercent +
                                  BoronPercent + CopperPercent + IronPercent +
                                  ManganesePercent + MolybdenumPercent + ZincPercent +
                                  ChlorinePercent + SilicaPercent + HumicAcidPercent + FulvicAcidPercent;
            
            // Calculate total PPM
            TotalNutrientPpm = NitrogenPpm + PhosphorusPpm + PotassiumPpm +
                              CalciumPpm + MagnesiumPpm + SulfurPpm +
                              BoronPpm + CopperPpm + IronPpm +
                              ManganesePpm + MolybdenumPpm + ZincPpm +
                              ChlorinePpm + SilicaPpm + HumicAcidPpm + FulvicAcidPpm;
        }

        private void AddFertilizerToMix(Fertilizer fertilizer)
        {
            try
            {
                Debug.WriteLine($"Adding fertilizer to mix: {fertilizer.Name}");
                
                // Check if the fertilizer is already in the mix
                var existingItem = CurrentMix.FirstOrDefault(item => item.FertilizerName == fertilizer.Name);
                
                if (existingItem == null)
                {
                    // Add new fertilizer to the mix with default quantity of 1.0
                    CurrentMix.Add(new FertilizerQuantity
                    {
                        FertilizerName = fertilizer.Name,
                        Quantity = 1.0
                    });
                    
                    // Update nutrient totals
                    UpdateNutrientTotals();
                }
                else
                {
                    // Increment the quantity if it already exists
                    existingItem.Quantity += 1.0;
                    UpdateNutrientTotals();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding fertilizer to mix: {ex.Message}");
            }
        }
    }
}
