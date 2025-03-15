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
        
        // Load fertilizers and mixes to populate available lists
        _ = LoadAndInitializeAsync(currentMix, useImperialUnits);
    }
    
    private async Task LoadAndInitializeAsync(FertilizerMix currentMix, bool useImperialUnits)
    {
        try
        {
            // Load available fertilizers
            var availableFertilizers = await _fileService.LoadFertilizersAsync();
            
            // Load available mixes
            var availableMixes = await _fileService.LoadMixesAsync();
            
            if (availableFertilizers != null)
            {
                // Add the current mix to the available mixes for selection
                if (availableMixes == null)
                {
                    availableMixes = new ObservableCollection<FertilizerMix>();
                }
                
                // Check if current mix has ingredients before adding
                if (currentMix.Ingredients != null && currentMix.Ingredients.Count > 0)
                {
                    // Add current mix at the beginning of the list
                    availableMixes.Insert(0, currentMix);
                }
                
                // Initialize view model with loaded data
                _viewModel = new CompareMixViewModel(currentMix, availableFertilizers, availableMixes, useImperialUnits);
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
        
        // Create a collection with just these two mixes
        var mixes = new ObservableCollection<FertilizerMix> { mix1, mix2 };
        
        _viewModel = new CompareMixViewModel(mix1, availableFertilizers, mixes, useImperialUnits);
        _viewModel.SelectedMix1 = mix1;
        _viewModel.SelectedMix2 = mix2;
        _viewModel.CloseCommand = new Command(async () => await Navigation.PopModalAsync());
        BindingContext = _viewModel;
    }
}

public class CompareMixViewModel : INotifyPropertyChanged
{
    private FertilizerMix _mix1;
    private FertilizerMix _mix2;
    private List<Fertilizer> _availableFertilizers;
    private ObservableCollection<FertilizerMix> _availableMixes;
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
    
    // Command for toggling between metric and imperial units
    public ICommand ToggleUnitCommand { get; private set; }
    
    // Color for the unit toggle button
    public Color UnitButtonColor => UseImperialUnits ? Colors.DarkOrange : Colors.DarkGreen;

    public CompareMixViewModel(FertilizerMix initialMix, List<Fertilizer> availableFertilizers, ObservableCollection<FertilizerMix> availableMixes, bool useImperialUnits)
    {
        _mix1 = initialMix;
        _mix2 = availableMixes.Count > 1 ? availableMixes[1] : new FertilizerMix { Name = "Empty Mix", Ingredients = new List<FertilizerQuantity>() };
        _availableFertilizers = availableFertilizers;
        _availableMixes = availableMixes;
        _useImperialUnits = useImperialUnits;
        
        // Initialize commands
        ToggleUnitCommand = new Command(ExecuteToggleUnit);
        
        // Calculate the nutrient values for both mixes
        CalculateAllNutrients();
    }

    // Toggle between metric and imperial units
    private void ExecuteToggleUnit()
    {
        UseImperialUnits = !UseImperialUnits;
        OnPropertyChanged(nameof(UnitButtonColor));
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
                OnPropertyChanged(nameof(UnitButtonColor));
            }
        }
    }

    // Available mixes for selection
    public ObservableCollection<FertilizerMix> AvailableMixes => _availableMixes;

    // Selected mixes
    private FertilizerMix _selectedMix1;
    public FertilizerMix SelectedMix1
    {
        get => _selectedMix1 ?? _mix1;
        set
        {
            if (_selectedMix1 != value && value != null)
            {
                _selectedMix1 = value;
                _mix1 = value;
                CalculateAllNutrients();
                OnPropertyChanged(nameof(SelectedMix1));
                OnPropertyChanged(nameof(Mix1Name));
                OnPropertyChanged(nameof(Mix1IngredientCount));
                OnPropertyChanged(nameof(Mix1Ingredients));
            }
        }
    }

    private FertilizerMix _selectedMix2;
    public FertilizerMix SelectedMix2
    {
        get => _selectedMix2 ?? _mix2;
        set
        {
            if (_selectedMix2 != value && value != null)
            {
                _selectedMix2 = value;
                _mix2 = value;
                CalculateAllNutrients();
                OnPropertyChanged(nameof(SelectedMix2));
                OnPropertyChanged(nameof(Mix2Name));
                OnPropertyChanged(nameof(Mix2IngredientCount));
                OnPropertyChanged(nameof(Mix2Ingredients));
            }
        }
    }

    // Mix names and ingredient counts
    public string Mix1Name => _mix1?.Name ?? "Mix 1";
    public string Mix2Name => _mix2?.Name ?? "Mix 2";
    
    public int Mix1IngredientCount => _mix1?.Ingredients?.Count ?? 0;
    public int Mix2IngredientCount => _mix2?.Ingredients?.Count ?? 0;
    
    public List<FertilizerQuantity> Mix1Ingredients => _mix1?.Ingredients ?? new List<FertilizerQuantity>();
    public List<FertilizerQuantity> Mix2Ingredients => _mix2?.Ingredients ?? new List<FertilizerQuantity>();

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
        // If mix is null or has no ingredients, set all values to 0
        if (mix == null || mix.Ingredients == null || mix.Ingredients.Count == 0)
        {
            if (mixNumber == 1)
            {
                _mix1NitrogenPpm = 0;
                _mix1PhosphorusPpm = 0;
                _mix1PotassiumPpm = 0;
                _mix1CalciumPpm = 0;
                _mix1MagnesiumPpm = 0;
                _mix1SulfurPpm = 0;
                _mix1BoronPpm = 0;
                _mix1CopperPpm = 0;
                _mix1IronPpm = 0;
                _mix1ManganesePpm = 0;
                _mix1MolybdenumPpm = 0;
                _mix1ZincPpm = 0;
                _mix1ChlorinePpm = 0;
                _mix1SilicaPpm = 0;
                _mix1HumicAcidPpm = 0;
                _mix1FulvicAcidPpm = 0;
                
                // Notify all Mix 1 properties
                OnPropertyChanged(nameof(Mix1NitrogenPpm));
                OnPropertyChanged(nameof(Mix1PhosphorusPpm));
                OnPropertyChanged(nameof(Mix1PotassiumPpm));
                OnPropertyChanged(nameof(Mix1CalciumPpm));
                OnPropertyChanged(nameof(Mix1MagnesiumPpm));
                OnPropertyChanged(nameof(Mix1SulfurPpm));
                OnPropertyChanged(nameof(Mix1BoronPpm));
                OnPropertyChanged(nameof(Mix1CopperPpm));
                OnPropertyChanged(nameof(Mix1IronPpm));
                OnPropertyChanged(nameof(Mix1ManganesePpm));
                OnPropertyChanged(nameof(Mix1MolybdenumPpm));
                OnPropertyChanged(nameof(Mix1ZincPpm));
                OnPropertyChanged(nameof(Mix1ChlorinePpm));
                OnPropertyChanged(nameof(Mix1SilicaPpm));
                OnPropertyChanged(nameof(Mix1HumicAcidPpm));
                OnPropertyChanged(nameof(Mix1FulvicAcidPpm));
            }
            else if (mixNumber == 2)
            {
                _mix2NitrogenPpm = 0;
                _mix2PhosphorusPpm = 0;
                _mix2PotassiumPpm = 0;
                _mix2CalciumPpm = 0;
                _mix2MagnesiumPpm = 0;
                _mix2SulfurPpm = 0;
                _mix2BoronPpm = 0;
                _mix2CopperPpm = 0;
                _mix2IronPpm = 0;
                _mix2ManganesePpm = 0;
                _mix2MolybdenumPpm = 0;
                _mix2ZincPpm = 0;
                _mix2ChlorinePpm = 0;
                _mix2SilicaPpm = 0;
                _mix2HumicAcidPpm = 0;
                _mix2FulvicAcidPpm = 0;
                
                // Notify all Mix 2 properties
                OnPropertyChanged(nameof(Mix2NitrogenPpm));
                OnPropertyChanged(nameof(Mix2PhosphorusPpm));
                OnPropertyChanged(nameof(Mix2PotassiumPpm));
                OnPropertyChanged(nameof(Mix2CalciumPpm));
                OnPropertyChanged(nameof(Mix2MagnesiumPpm));
                OnPropertyChanged(nameof(Mix2SulfurPpm));
                OnPropertyChanged(nameof(Mix2BoronPpm));
                OnPropertyChanged(nameof(Mix2CopperPpm));
                OnPropertyChanged(nameof(Mix2IronPpm));
                OnPropertyChanged(nameof(Mix2ManganesePpm));
                OnPropertyChanged(nameof(Mix2MolybdenumPpm));
                OnPropertyChanged(nameof(Mix2ZincPpm));
                OnPropertyChanged(nameof(Mix2ChlorinePpm));
                OnPropertyChanged(nameof(Mix2SilicaPpm));
                OnPropertyChanged(nameof(Mix2HumicAcidPpm));
                OnPropertyChanged(nameof(Mix2FulvicAcidPpm));
            }
            return;
        }

        // Initialize nutrient totals
        double nitrogenPpm = 0;
        double phosphorusPpm = 0;
        double potassiumPpm = 0;
        double calciumPpm = 0;
        double magnesiumPpm = 0;
        double sulfurPpm = 0;
        double boronPpm = 0;
        double copperPpm = 0;
        double ironPpm = 0;
        double manganesePpm = 0;
        double molybdenumPpm = 0;
        double zincPpm = 0;
        double chlorinePpm = 0;
        double silicaPpm = 0;
        double humicAcidPpm = 0;
        double fulvicAcidPpm = 0;

        // Calculate total nutrients for the mix
        foreach (var ingredient in mix.Ingredients)
        {
            // Find the fertilizer in the available list
            var fertilizer = _availableFertilizers.FirstOrDefault(f => f.Name == ingredient.FertilizerName);
            if (fertilizer != null)
            {
                // Calculate PPM based on quantity and fertilizer percentages
                double quantity = ingredient.Quantity;
                
                // Apply unit conversion if using imperial units
                nitrogenPpm += fertilizer.NitrogenPpm(_useImperialUnits) * quantity;
                phosphorusPpm += fertilizer.PhosphorusPpm(_useImperialUnits) * quantity;
                potassiumPpm += fertilizer.PotassiumPpm(_useImperialUnits) * quantity;
                calciumPpm += fertilizer.CalciumPpm(_useImperialUnits) * quantity;
                magnesiumPpm += fertilizer.MagnesiumPpm(_useImperialUnits) * quantity;
                sulfurPpm += fertilizer.SulfurPpm(_useImperialUnits) * quantity;
                boronPpm += fertilizer.BoronPpm(_useImperialUnits) * quantity;
                copperPpm += fertilizer.CopperPpm(_useImperialUnits) * quantity;
                ironPpm += fertilizer.IronPpm(_useImperialUnits) * quantity;
                manganesePpm += fertilizer.ManganesePpm(_useImperialUnits) * quantity;
                molybdenumPpm += fertilizer.MolybdenumPpm(_useImperialUnits) * quantity;
                zincPpm += fertilizer.ZincPpm(_useImperialUnits) * quantity;
                chlorinePpm += fertilizer.ChlorinePpm(_useImperialUnits) * quantity;
                silicaPpm += fertilizer.SilicaPpm(_useImperialUnits) * quantity;
                humicAcidPpm += fertilizer.HumicAcidPpm(_useImperialUnits) * quantity;
                fulvicAcidPpm += fertilizer.FulvicAcidPpm(_useImperialUnits) * quantity;
            }
        }

        // Update the appropriate mix properties
        if (mixNumber == 1)
        {
            _mix1NitrogenPpm = Math.Round(nitrogenPpm, 2);
            _mix1PhosphorusPpm = Math.Round(phosphorusPpm, 2);
            _mix1PotassiumPpm = Math.Round(potassiumPpm, 2);
            _mix1CalciumPpm = Math.Round(calciumPpm, 2);
            _mix1MagnesiumPpm = Math.Round(magnesiumPpm, 2);
            _mix1SulfurPpm = Math.Round(sulfurPpm, 2);
            _mix1BoronPpm = Math.Round(boronPpm, 2);
            _mix1CopperPpm = Math.Round(copperPpm, 2);
            _mix1IronPpm = Math.Round(ironPpm, 2);
            _mix1ManganesePpm = Math.Round(manganesePpm, 2);
            _mix1MolybdenumPpm = Math.Round(molybdenumPpm, 2);
            _mix1ZincPpm = Math.Round(zincPpm, 2);
            _mix1ChlorinePpm = Math.Round(chlorinePpm, 2);
            _mix1SilicaPpm = Math.Round(silicaPpm, 2);
            _mix1HumicAcidPpm = Math.Round(humicAcidPpm, 2);
            _mix1FulvicAcidPpm = Math.Round(fulvicAcidPpm, 2);
            
            // Notify all Mix 1 properties
            OnPropertyChanged(nameof(Mix1NitrogenPpm));
            OnPropertyChanged(nameof(Mix1PhosphorusPpm));
            OnPropertyChanged(nameof(Mix1PotassiumPpm));
            OnPropertyChanged(nameof(Mix1CalciumPpm));
            OnPropertyChanged(nameof(Mix1MagnesiumPpm));
            OnPropertyChanged(nameof(Mix1SulfurPpm));
            OnPropertyChanged(nameof(Mix1BoronPpm));
            OnPropertyChanged(nameof(Mix1CopperPpm));
            OnPropertyChanged(nameof(Mix1IronPpm));
            OnPropertyChanged(nameof(Mix1ManganesePpm));
            OnPropertyChanged(nameof(Mix1MolybdenumPpm));
            OnPropertyChanged(nameof(Mix1ZincPpm));
            OnPropertyChanged(nameof(Mix1ChlorinePpm));
            OnPropertyChanged(nameof(Mix1SilicaPpm));
            OnPropertyChanged(nameof(Mix1HumicAcidPpm));
            OnPropertyChanged(nameof(Mix1FulvicAcidPpm));
        }
        else if (mixNumber == 2)
        {
            _mix2NitrogenPpm = Math.Round(nitrogenPpm, 2);
            _mix2PhosphorusPpm = Math.Round(phosphorusPpm, 2);
            _mix2PotassiumPpm = Math.Round(potassiumPpm, 2);
            _mix2CalciumPpm = Math.Round(calciumPpm, 2);
            _mix2MagnesiumPpm = Math.Round(magnesiumPpm, 2);
            _mix2SulfurPpm = Math.Round(sulfurPpm, 2);
            _mix2BoronPpm = Math.Round(boronPpm, 2);
            _mix2CopperPpm = Math.Round(copperPpm, 2);
            _mix2IronPpm = Math.Round(ironPpm, 2);
            _mix2ManganesePpm = Math.Round(manganesePpm, 2);
            _mix2MolybdenumPpm = Math.Round(molybdenumPpm, 2);
            _mix2ZincPpm = Math.Round(zincPpm, 2);
            _mix2ChlorinePpm = Math.Round(chlorinePpm, 2);
            _mix2SilicaPpm = Math.Round(silicaPpm, 2);
            _mix2HumicAcidPpm = Math.Round(humicAcidPpm, 2);
            _mix2FulvicAcidPpm = Math.Round(fulvicAcidPpm, 2);
            
            // Notify all Mix 2 properties
            OnPropertyChanged(nameof(Mix2NitrogenPpm));
            OnPropertyChanged(nameof(Mix2PhosphorusPpm));
            OnPropertyChanged(nameof(Mix2PotassiumPpm));
            OnPropertyChanged(nameof(Mix2CalciumPpm));
            OnPropertyChanged(nameof(Mix2MagnesiumPpm));
            OnPropertyChanged(nameof(Mix2SulfurPpm));
            OnPropertyChanged(nameof(Mix2BoronPpm));
            OnPropertyChanged(nameof(Mix2CopperPpm));
            OnPropertyChanged(nameof(Mix2IronPpm));
            OnPropertyChanged(nameof(Mix2ManganesePpm));
            OnPropertyChanged(nameof(Mix2MolybdenumPpm));
            OnPropertyChanged(nameof(Mix2ZincPpm));
            OnPropertyChanged(nameof(Mix2ChlorinePpm));
            OnPropertyChanged(nameof(Mix2SilicaPpm));
            OnPropertyChanged(nameof(Mix2HumicAcidPpm));
            OnPropertyChanged(nameof(Mix2FulvicAcidPpm));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
