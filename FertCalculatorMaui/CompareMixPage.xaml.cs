using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

public partial class CompareMixPage : ContentPage
{
    private CompareMixViewModel? _viewModel;
    private FileService _fileService;

    public CompareMixPage(FileService fileService, List<FertilizerQuantity> currentMixIngredients, bool useImperialUnits)
    {
        InitializeComponent();
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        
        // Create temporary mixes for comparison
        var currentMix = new FertilizerMix { 
            Name = "Current Mix", 
            Ingredients = currentMixIngredients 
        };
        
        var emptyMix = new FertilizerMix { 
            Name = "Empty Mix", 
            Ingredients = new List<FertilizerQuantity>() 
        };
        
        // Load fertilizers to populate available list
        _ = LoadAndInitializeAsync(currentMix, emptyMix, useImperialUnits);
    }
    
    private async Task LoadAndInitializeAsync(FertilizerMix mix1, FertilizerMix mix2, bool useImperialUnits)
    {
        try
        {
            // Load available fertilizers
            var availableFertilizers = await _fileService.LoadFertilizersAsync();
            
            if (availableFertilizers != null)
            {
                // Initialize view model with loaded data
                _viewModel = new CompareMixViewModel(mix1, mix2, availableFertilizers, useImperialUnits);
                _viewModel.CloseCommand = new Command(async () => await Navigation.PopAsync());
                BindingContext = _viewModel;
            }
            else
            {
                await DisplayAlert("Error", "Failed to load fertilizers.", "OK");
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            await Navigation.PopAsync();
        }
    }

    public CompareMixPage(FertilizerMix mix1, FertilizerMix mix2, List<Fertilizer> availableFertilizers, bool useImperialUnits)
    {
        InitializeComponent();
        _viewModel = new CompareMixViewModel(mix1, mix2, availableFertilizers, useImperialUnits);
        _viewModel.CloseCommand = new Command(async () => await Navigation.PopModalAsync());
        BindingContext = _viewModel;
    }
}

public class CompareMixViewModel : INotifyPropertyChanged
{
    private FertilizerMix _mix1;
    private FertilizerMix _mix2;
    private List<Fertilizer> _availableFertilizers;
    private bool _useImperialUnits;
    private const double GALLON_TO_LITER = 3.78541;

    // Mix 1 (Left side) nutrients
    private double _mix1NitrogenPpm;
    private double _mix1PhosphorusPpm;
    private double _mix1PotassiumPpm;
    private double _mix1CalciumPpm;
    private double _mix1MagnesiumPpm;
    private double _mix1SulfurPpm;
    private double _mix1BoronPpm;
    private double _mix1CopperPpm;
    private double _mix1IronPpm;
    private double _mix1ManganesePpm;
    private double _mix1MolybdenumPpm;
    private double _mix1ZincPpm;
    private double _mix1ChlorinePpm;
    private double _mix1SilicaPpm;
    private double _mix1HumicAcidPpm;
    private double _mix1FulvicAcidPpm;

    // Mix 2 (Right side) nutrients
    private double _mix2NitrogenPpm;
    private double _mix2PhosphorusPpm;
    private double _mix2PotassiumPpm;
    private double _mix2CalciumPpm;
    private double _mix2MagnesiumPpm;
    private double _mix2SulfurPpm;
    private double _mix2BoronPpm;
    private double _mix2CopperPpm;
    private double _mix2IronPpm;
    private double _mix2ManganesePpm;
    private double _mix2MolybdenumPpm;
    private double _mix2ZincPpm;
    private double _mix2ChlorinePpm;
    private double _mix2SilicaPpm;
    private double _mix2HumicAcidPpm;
    private double _mix2FulvicAcidPpm;

    public string UnitLabel => UseImperialUnits ? "PPM per gallon" : "PPM per liter";

    // Command for closing the page
    public ICommand? CloseCommand { get; set; }

    public CompareMixViewModel(FertilizerMix mix1, FertilizerMix mix2, List<Fertilizer> availableFertilizers, bool useImperialUnits)
    {
        _mix1 = mix1;
        _mix2 = mix2;
        _availableFertilizers = availableFertilizers;
        _useImperialUnits = useImperialUnits;
        
        // Calculate the nutrient values for both mixes
        CalculateAllNutrients();
    }

    // Properties for unit selection
    public bool UseImperialUnits
    {
        get => _useImperialUnits;
        set
        {
            if (_useImperialUnits != value)
            {
                _useImperialUnits = value;
                CalculateAllNutrients();
                OnPropertyChanged(nameof(UseImperialUnits));
                OnPropertyChanged(nameof(UnitLabel));
            }
        }
    }

    // Mix names
    public string Mix1Name => _mix1.Name;
    public string Mix2Name => _mix2.Name;

    // Mix 1 nutrient properties
    public double Mix1NitrogenPpm
    {
        get => _mix1NitrogenPpm;
        set
        {
            if (_mix1NitrogenPpm != value)
            {
                _mix1NitrogenPpm = value;
                OnPropertyChanged(nameof(Mix1NitrogenPpm));
                OnPropertyChanged(nameof(NitrogenDifference));
            }
        }
    }

    public double Mix1PhosphorusPpm
    {
        get => _mix1PhosphorusPpm;
        set
        {
            if (_mix1PhosphorusPpm != value)
            {
                _mix1PhosphorusPpm = value;
                OnPropertyChanged(nameof(Mix1PhosphorusPpm));
                OnPropertyChanged(nameof(PhosphorusDifference));
            }
        }
    }

    public double Mix1PotassiumPpm
    {
        get => _mix1PotassiumPpm;
        set
        {
            if (_mix1PotassiumPpm != value)
            {
                _mix1PotassiumPpm = value;
                OnPropertyChanged(nameof(Mix1PotassiumPpm));
                OnPropertyChanged(nameof(PotassiumDifference));
            }
        }
    }

    public double Mix1CalciumPpm
    {
        get => _mix1CalciumPpm;
        set
        {
            if (_mix1CalciumPpm != value)
            {
                _mix1CalciumPpm = value;
                OnPropertyChanged(nameof(Mix1CalciumPpm));
                OnPropertyChanged(nameof(CalciumDifference));
            }
        }
    }

    public double Mix1MagnesiumPpm
    {
        get => _mix1MagnesiumPpm;
        set
        {
            if (_mix1MagnesiumPpm != value)
            {
                _mix1MagnesiumPpm = value;
                OnPropertyChanged(nameof(Mix1MagnesiumPpm));
                OnPropertyChanged(nameof(MagnesiumDifference));
            }
        }
    }

    public double Mix1SulfurPpm
    {
        get => _mix1SulfurPpm;
        set
        {
            if (_mix1SulfurPpm != value)
            {
                _mix1SulfurPpm = value;
                OnPropertyChanged(nameof(Mix1SulfurPpm));
                OnPropertyChanged(nameof(SulfurDifference));
            }
        }
    }

    public double Mix1BoronPpm
    {
        get => _mix1BoronPpm;
        set
        {
            if (_mix1BoronPpm != value)
            {
                _mix1BoronPpm = value;
                OnPropertyChanged(nameof(Mix1BoronPpm));
                OnPropertyChanged(nameof(BoronDifference));
            }
        }
    }

    public double Mix1CopperPpm
    {
        get => _mix1CopperPpm;
        set
        {
            if (_mix1CopperPpm != value)
            {
                _mix1CopperPpm = value;
                OnPropertyChanged(nameof(Mix1CopperPpm));
                OnPropertyChanged(nameof(CopperDifference));
            }
        }
    }

    public double Mix1IronPpm
    {
        get => _mix1IronPpm;
        set
        {
            if (_mix1IronPpm != value)
            {
                _mix1IronPpm = value;
                OnPropertyChanged(nameof(Mix1IronPpm));
                OnPropertyChanged(nameof(IronDifference));
            }
        }
    }

    public double Mix1ManganesePpm
    {
        get => _mix1ManganesePpm;
        set
        {
            if (_mix1ManganesePpm != value)
            {
                _mix1ManganesePpm = value;
                OnPropertyChanged(nameof(Mix1ManganesePpm));
                OnPropertyChanged(nameof(ManganeseDifference));
            }
        }
    }

    public double Mix1MolybdenumPpm
    {
        get => _mix1MolybdenumPpm;
        set
        {
            if (_mix1MolybdenumPpm != value)
            {
                _mix1MolybdenumPpm = value;
                OnPropertyChanged(nameof(Mix1MolybdenumPpm));
                OnPropertyChanged(nameof(MolybdenumDifference));
            }
        }
    }

    public double Mix1ZincPpm
    {
        get => _mix1ZincPpm;
        set
        {
            if (_mix1ZincPpm != value)
            {
                _mix1ZincPpm = value;
                OnPropertyChanged(nameof(Mix1ZincPpm));
                OnPropertyChanged(nameof(ZincDifference));
            }
        }
    }

    public double Mix1ChlorinePpm
    {
        get => _mix1ChlorinePpm;
        set
        {
            if (_mix1ChlorinePpm != value)
            {
                _mix1ChlorinePpm = value;
                OnPropertyChanged(nameof(Mix1ChlorinePpm));
                OnPropertyChanged(nameof(ChlorineDifference));
            }
        }
    }

    public double Mix1SilicaPpm
    {
        get => _mix1SilicaPpm;
        set
        {
            if (_mix1SilicaPpm != value)
            {
                _mix1SilicaPpm = value;
                OnPropertyChanged(nameof(Mix1SilicaPpm));
                OnPropertyChanged(nameof(SilicaDifference));
            }
        }
    }

    public double Mix1HumicAcidPpm
    {
        get => _mix1HumicAcidPpm;
        set
        {
            if (_mix1HumicAcidPpm != value)
            {
                _mix1HumicAcidPpm = value;
                OnPropertyChanged(nameof(Mix1HumicAcidPpm));
                OnPropertyChanged(nameof(HumicAcidDifference));
            }
        }
    }

    public double Mix1FulvicAcidPpm
    {
        get => _mix1FulvicAcidPpm;
        set
        {
            if (_mix1FulvicAcidPpm != value)
            {
                _mix1FulvicAcidPpm = value;
                OnPropertyChanged(nameof(Mix1FulvicAcidPpm));
                OnPropertyChanged(nameof(FulvicAcidDifference));
            }
        }
    }

    // Mix 2 nutrient properties
    public double Mix2NitrogenPpm
    {
        get => _mix2NitrogenPpm;
        set
        {
            if (_mix2NitrogenPpm != value)
            {
                _mix2NitrogenPpm = value;
                OnPropertyChanged(nameof(Mix2NitrogenPpm));
                OnPropertyChanged(nameof(NitrogenDifference));
            }
        }
    }

    public double Mix2PhosphorusPpm
    {
        get => _mix2PhosphorusPpm;
        set
        {
            if (_mix2PhosphorusPpm != value)
            {
                _mix2PhosphorusPpm = value;
                OnPropertyChanged(nameof(Mix2PhosphorusPpm));
                OnPropertyChanged(nameof(PhosphorusDifference));
            }
        }
    }

    public double Mix2PotassiumPpm
    {
        get => _mix2PotassiumPpm;
        set
        {
            if (_mix2PotassiumPpm != value)
            {
                _mix2PotassiumPpm = value;
                OnPropertyChanged(nameof(Mix2PotassiumPpm));
                OnPropertyChanged(nameof(PotassiumDifference));
            }
        }
    }

    public double Mix2CalciumPpm
    {
        get => _mix2CalciumPpm;
        set
        {
            if (_mix2CalciumPpm != value)
            {
                _mix2CalciumPpm = value;
                OnPropertyChanged(nameof(Mix2CalciumPpm));
                OnPropertyChanged(nameof(CalciumDifference));
            }
        }
    }

    public double Mix2MagnesiumPpm
    {
        get => _mix2MagnesiumPpm;
        set
        {
            if (_mix2MagnesiumPpm != value)
            {
                _mix2MagnesiumPpm = value;
                OnPropertyChanged(nameof(Mix2MagnesiumPpm));
                OnPropertyChanged(nameof(MagnesiumDifference));
            }
        }
    }

    public double Mix2SulfurPpm
    {
        get => _mix2SulfurPpm;
        set
        {
            if (_mix2SulfurPpm != value)
            {
                _mix2SulfurPpm = value;
                OnPropertyChanged(nameof(Mix2SulfurPpm));
                OnPropertyChanged(nameof(SulfurDifference));
            }
        }
    }

    public double Mix2BoronPpm
    {
        get => _mix2BoronPpm;
        set
        {
            if (_mix2BoronPpm != value)
            {
                _mix2BoronPpm = value;
                OnPropertyChanged(nameof(Mix2BoronPpm));
                OnPropertyChanged(nameof(BoronDifference));
            }
        }
    }

    public double Mix2CopperPpm
    {
        get => _mix2CopperPpm;
        set
        {
            if (_mix2CopperPpm != value)
            {
                _mix2CopperPpm = value;
                OnPropertyChanged(nameof(Mix2CopperPpm));
                OnPropertyChanged(nameof(CopperDifference));
            }
        }
    }

    public double Mix2IronPpm
    {
        get => _mix2IronPpm;
        set
        {
            if (_mix2IronPpm != value)
            {
                _mix2IronPpm = value;
                OnPropertyChanged(nameof(Mix2IronPpm));
                OnPropertyChanged(nameof(IronDifference));
            }
        }
    }

    public double Mix2ManganesePpm
    {
        get => _mix2ManganesePpm;
        set
        {
            if (_mix2ManganesePpm != value)
            {
                _mix2ManganesePpm = value;
                OnPropertyChanged(nameof(Mix2ManganesePpm));
                OnPropertyChanged(nameof(ManganeseDifference));
            }
        }
    }

    public double Mix2MolybdenumPpm
    {
        get => _mix2MolybdenumPpm;
        set
        {
            if (_mix2MolybdenumPpm != value)
            {
                _mix2MolybdenumPpm = value;
                OnPropertyChanged(nameof(Mix2MolybdenumPpm));
                OnPropertyChanged(nameof(MolybdenumDifference));
            }
        }
    }

    public double Mix2ZincPpm
    {
        get => _mix2ZincPpm;
        set
        {
            if (_mix2ZincPpm != value)
            {
                _mix2ZincPpm = value;
                OnPropertyChanged(nameof(Mix2ZincPpm));
                OnPropertyChanged(nameof(ZincDifference));
            }
        }
    }

    public double Mix2ChlorinePpm
    {
        get => _mix2ChlorinePpm;
        set
        {
            if (_mix2ChlorinePpm != value)
            {
                _mix2ChlorinePpm = value;
                OnPropertyChanged(nameof(Mix2ChlorinePpm));
                OnPropertyChanged(nameof(ChlorineDifference));
            }
        }
    }

    public double Mix2SilicaPpm
    {
        get => _mix2SilicaPpm;
        set
        {
            if (_mix2SilicaPpm != value)
            {
                _mix2SilicaPpm = value;
                OnPropertyChanged(nameof(Mix2SilicaPpm));
                OnPropertyChanged(nameof(SilicaDifference));
            }
        }
    }

    public double Mix2HumicAcidPpm
    {
        get => _mix2HumicAcidPpm;
        set
        {
            if (_mix2HumicAcidPpm != value)
            {
                _mix2HumicAcidPpm = value;
                OnPropertyChanged(nameof(Mix2HumicAcidPpm));
                OnPropertyChanged(nameof(HumicAcidDifference));
            }
        }
    }

    public double Mix2FulvicAcidPpm
    {
        get => _mix2FulvicAcidPpm;
        set
        {
            if (_mix2FulvicAcidPpm != value)
            {
                _mix2FulvicAcidPpm = value;
                OnPropertyChanged(nameof(Mix2FulvicAcidPpm));
                OnPropertyChanged(nameof(FulvicAcidDifference));
            }
        }
    }

    // Difference properties (Mix1 - Mix2)
    public double NitrogenDifference => Mix1NitrogenPpm - Mix2NitrogenPpm;
    public double PhosphorusDifference => Mix1PhosphorusPpm - Mix2PhosphorusPpm;
    public double PotassiumDifference => Mix1PotassiumPpm - Mix2PotassiumPpm;
    public double CalciumDifference => Mix1CalciumPpm - Mix2CalciumPpm;
    public double MagnesiumDifference => Mix1MagnesiumPpm - Mix2MagnesiumPpm;
    public double SulfurDifference => Mix1SulfurPpm - Mix2SulfurPpm;
    public double BoronDifference => Mix1BoronPpm - Mix2BoronPpm;
    public double CopperDifference => Mix1CopperPpm - Mix2CopperPpm;
    public double IronDifference => Mix1IronPpm - Mix2IronPpm;
    public double ManganeseDifference => Mix1ManganesePpm - Mix2ManganesePpm;
    public double MolybdenumDifference => Mix1MolybdenumPpm - Mix2MolybdenumPpm;
    public double ZincDifference => Mix1ZincPpm - Mix2ZincPpm;
    public double ChlorineDifference => Mix1ChlorinePpm - Mix2ChlorinePpm;
    public double SilicaDifference => Mix1SilicaPpm - Mix2SilicaPpm;
    public double HumicAcidDifference => Mix1HumicAcidPpm - Mix2HumicAcidPpm;
    public double FulvicAcidDifference => Mix1FulvicAcidPpm - Mix2FulvicAcidPpm;

    private void CalculateAllNutrients()
    {
        // Calculate nutrient totals for Mix 1 and Mix 2
        CalculateMixNutrients(_mix1, 1);
        CalculateMixNutrients(_mix2, 2);
        
        // Notify all difference properties
        OnPropertyChanged(nameof(NitrogenDifference));
        OnPropertyChanged(nameof(PhosphorusDifference));
        OnPropertyChanged(nameof(PotassiumDifference));
        OnPropertyChanged(nameof(CalciumDifference));
        OnPropertyChanged(nameof(MagnesiumDifference));
        OnPropertyChanged(nameof(SulfurDifference));
        OnPropertyChanged(nameof(BoronDifference));
        OnPropertyChanged(nameof(CopperDifference));
        OnPropertyChanged(nameof(IronDifference));
        OnPropertyChanged(nameof(ManganeseDifference));
        OnPropertyChanged(nameof(MolybdenumDifference));
        OnPropertyChanged(nameof(ZincDifference));
        OnPropertyChanged(nameof(ChlorineDifference));
        OnPropertyChanged(nameof(SilicaDifference));
        OnPropertyChanged(nameof(HumicAcidDifference));
        OnPropertyChanged(nameof(FulvicAcidDifference));
    }

    private void CalculateMixNutrients(FertilizerMix mix, int mixNumber)
    {
        // Reset all nutrient totals
        double nitrogenTotal = 0;
        double phosphorusTotal = 0;
        double potassiumTotal = 0;
        double calciumTotal = 0;
        double magnesiumTotal = 0;
        double sulfurTotal = 0;
        double boronTotal = 0;
        double copperTotal = 0;
        double ironTotal = 0;
        double manganeseTotal = 0;
        double molybdenumTotal = 0;
        double zincTotal = 0;
        double chlorineTotal = 0;
        double silicaTotal = 0;
        double humicAcidTotal = 0;
        double fulvicAcidTotal = 0;
        
        // Calculate totals for each ingredient in the mix
        foreach (var ingredient in mix.Ingredients)
        {
            // Find the fertilizer details
            var fertilizer = _availableFertilizers.FirstOrDefault(f => f.Name == ingredient.FertilizerName);
            if (fertilizer != null)
            {
                double quantity = ingredient.Quantity;
                
                // Add to totals for each nutrient, using the selected unit system
                nitrogenTotal += fertilizer.NitrogenPpm(UseImperialUnits) * quantity;
                phosphorusTotal += fertilizer.PhosphorusPpm(UseImperialUnits) * quantity;
                potassiumTotal += fertilizer.PotassiumPpm(UseImperialUnits) * quantity;
                calciumTotal += fertilizer.CalciumPpm(UseImperialUnits) * quantity;
                magnesiumTotal += fertilizer.MagnesiumPpm(UseImperialUnits) * quantity;
                sulfurTotal += fertilizer.SulfurPpm(UseImperialUnits) * quantity;
                boronTotal += fertilizer.BoronPpm(UseImperialUnits) * quantity;
                copperTotal += fertilizer.CopperPpm(UseImperialUnits) * quantity;
                ironTotal += fertilizer.IronPpm(UseImperialUnits) * quantity;
                manganeseTotal += fertilizer.ManganesePpm(UseImperialUnits) * quantity;
                molybdenumTotal += fertilizer.MolybdenumPpm(UseImperialUnits) * quantity;
                zincTotal += fertilizer.ZincPpm(UseImperialUnits) * quantity;
                chlorineTotal += fertilizer.ChlorinePpm(UseImperialUnits) * quantity;
                silicaTotal += fertilizer.SilicaPpm(UseImperialUnits) * quantity;
                humicAcidTotal += fertilizer.HumicAcidPpm(UseImperialUnits) * quantity;
                fulvicAcidTotal += fertilizer.FulvicAcidPpm(UseImperialUnits) * quantity;
            }
        }
        
        // Update the appropriate mix properties
        if (mixNumber == 1)
        {
            Mix1NitrogenPpm = nitrogenTotal;
            Mix1PhosphorusPpm = phosphorusTotal;
            Mix1PotassiumPpm = potassiumTotal;
            Mix1CalciumPpm = calciumTotal;
            Mix1MagnesiumPpm = magnesiumTotal;
            Mix1SulfurPpm = sulfurTotal;
            Mix1BoronPpm = boronTotal;
            Mix1CopperPpm = copperTotal;
            Mix1IronPpm = ironTotal;
            Mix1ManganesePpm = manganeseTotal;
            Mix1MolybdenumPpm = molybdenumTotal;
            Mix1ZincPpm = zincTotal;
            Mix1ChlorinePpm = chlorineTotal;
            Mix1SilicaPpm = silicaTotal;
            Mix1HumicAcidPpm = humicAcidTotal;
            Mix1FulvicAcidPpm = fulvicAcidTotal;
        }
        else if (mixNumber == 2)
        {
            Mix2NitrogenPpm = nitrogenTotal;
            Mix2PhosphorusPpm = phosphorusTotal;
            Mix2PotassiumPpm = potassiumTotal;
            Mix2CalciumPpm = calciumTotal;
            Mix2MagnesiumPpm = magnesiumTotal;
            Mix2SulfurPpm = sulfurTotal;
            Mix2BoronPpm = boronTotal;
            Mix2CopperPpm = copperTotal;
            Mix2IronPpm = ironTotal;
            Mix2ManganesePpm = manganeseTotal;
            Mix2MolybdenumPpm = molybdenumTotal;
            Mix2ZincPpm = zincTotal;
            Mix2ChlorinePpm = chlorineTotal;
            Mix2SilicaPpm = silicaTotal;
            Mix2HumicAcidPpm = humicAcidTotal;
            Mix2FulvicAcidPpm = fulvicAcidTotal;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
