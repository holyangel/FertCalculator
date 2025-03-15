using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FertCalculatorMaui;

public partial class CompareMixPage : ContentPage
{
    private CompareMixViewModel _viewModel;

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
    private const double GALLON_TO_LITER = 3.78541;
    private FertilizerMix _mix1;
    private FertilizerMix _mix2;
    private List<Fertilizer> _availableFertilizers;
    private bool _useImperialUnits;

    public CompareMixViewModel(FertilizerMix mix1, FertilizerMix mix2, List<Fertilizer> availableFertilizers, bool useImperialUnits)
    {
        _mix1 = mix1;
        _mix2 = mix2;
        _availableFertilizers = availableFertilizers;
        _useImperialUnits = useImperialUnits;

        CalculateAllNutrients();
    }

    private void CalculateAllNutrients()
    {
        // Create a copy of ingredients for binding
        Mix1Ingredients = new ObservableCollection<FertilizerQuantity>(_mix1.Ingredients);
        Mix2Ingredients = new ObservableCollection<FertilizerQuantity>(_mix2.Ingredients);

        // Calculate nutrient totals for Mix 1
        CalculateMixNutrients(
            _mix1.Ingredients,
            out double n1, out double p1, out double k1, out double ca1, out double mg1,
            out double s1, out double b1, out double cu1, out double fe1, out double mn1,
            out double mo1, out double zn1, out double cl1, out double si1, out double ha1, out double fa1);

        // Calculate nutrient totals for Mix 2
        CalculateMixNutrients(
            _mix2.Ingredients,
            out double n2, out double p2, out double k2, out double ca2, out double mg2,
            out double s2, out double b2, out double cu2, out double fe2, out double mn2,
            out double mo2, out double zn2, out double cl2, out double si2, out double ha2, out double fa2);

        // Apply unit conversion if necessary
        if (_useImperialUnits)
        {
            // Convert from PPM per liter to PPM per gallon by dividing by gallon-to-liter conversion factor
            double convertToImperial(double value) => value / GALLON_TO_LITER;

            n1 = convertToImperial(n1);
            p1 = convertToImperial(p1);
            k1 = convertToImperial(k1);
            ca1 = convertToImperial(ca1);
            mg1 = convertToImperial(mg1);
            s1 = convertToImperial(s1);
            b1 = convertToImperial(b1);
            cu1 = convertToImperial(cu1);
            fe1 = convertToImperial(fe1);
            mn1 = convertToImperial(mn1);
            mo1 = convertToImperial(mo1);
            zn1 = convertToImperial(zn1);
            cl1 = convertToImperial(cl1);
            si1 = convertToImperial(si1);
            ha1 = convertToImperial(ha1);
            fa1 = convertToImperial(fa1);

            n2 = convertToImperial(n2);
            p2 = convertToImperial(p2);
            k2 = convertToImperial(k2);
            ca2 = convertToImperial(ca2);
            mg2 = convertToImperial(mg2);
            s2 = convertToImperial(s2);
            b2 = convertToImperial(b2);
            cu2 = convertToImperial(cu2);
            fe2 = convertToImperial(fe2);
            mn2 = convertToImperial(mn2);
            mo2 = convertToImperial(mo2);
            zn2 = convertToImperial(zn2);
            cl2 = convertToImperial(cl2);
            si2 = convertToImperial(si2);
            ha2 = convertToImperial(ha2);
            fa2 = convertToImperial(fa2);
        }

        // Set the calculated values to properties
        Mix1NitrogenPpm = n1;
        Mix1PhosphorusPpm = p1;
        Mix1PotassiumPpm = k1;
        Mix1CalciumPpm = ca1;
        Mix1MagnesiumPpm = mg1;
        Mix1SulfurPpm = s1;
        Mix1BoronPpm = b1;
        Mix1CopperPpm = cu1;
        Mix1IronPpm = fe1;
        Mix1ManganesePpm = mn1;
        Mix1MolybdenumPpm = mo1;
        Mix1ZincPpm = zn1;
        Mix1ChlorinePpm = cl1;
        Mix1SilicaPpm = si1;
        Mix1HumicAcidPpm = ha1;
        Mix1FulvicAcidPpm = fa1;

        Mix2NitrogenPpm = n2;
        Mix2PhosphorusPpm = p2;
        Mix2PotassiumPpm = k2;
        Mix2CalciumPpm = ca2;
        Mix2MagnesiumPpm = mg2;
        Mix2SulfurPpm = s2;
        Mix2BoronPpm = b2;
        Mix2CopperPpm = cu2;
        Mix2IronPpm = fe2;
        Mix2ManganesePpm = mn2;
        Mix2MolybdenumPpm = mo2;
        Mix2ZincPpm = zn2;
        Mix2ChlorinePpm = cl2;
        Mix2SilicaPpm = si2;
        Mix2HumicAcidPpm = ha2;
        Mix2FulvicAcidPpm = fa2;
    }

    private void CalculateMixNutrients(
        List<FertilizerQuantity> ingredients,
        out double nitrogenPpm, out double phosphorusPpm, out double potassiumPpm,
        out double calciumPpm, out double magnesiumPpm, out double sulfurPpm,
        out double boronPpm, out double copperPpm, out double ironPpm,
        out double manganesePpm, out double molybdenumPpm, out double zincPpm,
        out double chlorinePpm, out double silicaPpm, out double humicAcidPpm,
        out double fulvicAcidPpm)
    {
        // Initialize all output parameters to zero
        nitrogenPpm = 0;
        phosphorusPpm = 0;
        potassiumPpm = 0;
        calciumPpm = 0;
        magnesiumPpm = 0;
        sulfurPpm = 0;
        boronPpm = 0;
        copperPpm = 0;
        ironPpm = 0;
        manganesePpm = 0;
        molybdenumPpm = 0;
        zincPpm = 0;
        chlorinePpm = 0;
        silicaPpm = 0;
        humicAcidPpm = 0;
        fulvicAcidPpm = 0;

        // Calculate the sum for each nutrient across all ingredients
        double totalGrams = 0;

        foreach (var ingredientQuantity in ingredients)
        {
            // Find the corresponding fertilizer from available fertilizers
            var fertilizer = _availableFertilizers.FirstOrDefault(f => f.Name == ingredientQuantity.FertilizerName);
            if (fertilizer == null) continue;

            double grams = ingredientQuantity.Quantity;
            totalGrams += grams;

            // Calculate the weighted contribution of each nutrient
            nitrogenPpm += (fertilizer.NitrogenPercent / 100) * grams;
            phosphorusPpm += (fertilizer.PhosphorusPercent / 100) * grams;
            potassiumPpm += (fertilizer.PotassiumPercent / 100) * grams;
            calciumPpm += (fertilizer.CalciumPercent / 100) * grams;
            magnesiumPpm += (fertilizer.MagnesiumPercent / 100) * grams;
            sulfurPpm += (fertilizer.SulfurPercent / 100) * grams;
            boronPpm += (fertilizer.BoronPercent / 100) * grams;
            copperPpm += (fertilizer.CopperPercent / 100) * grams;
            ironPpm += (fertilizer.IronPercent / 100) * grams;
            manganesePpm += (fertilizer.ManganesePercent / 100) * grams;
            molybdenumPpm += (fertilizer.MolybdenumPercent / 100) * grams;
            zincPpm += (fertilizer.ZincPercent / 100) * grams;
            chlorinePpm += (fertilizer.ChlorinePercent / 100) * grams;
            silicaPpm += (fertilizer.SilicaPercent / 100) * grams;
            humicAcidPpm += (fertilizer.HumicAcidPercent / 100) * grams;
            fulvicAcidPpm += (fertilizer.FulvicAcidPercent / 100) * grams;
        }

        // Convert to PPM (assumes 1L of water)
        if (totalGrams > 0)
        {
            nitrogenPpm = (nitrogenPpm / totalGrams) * 1000;
            phosphorusPpm = (phosphorusPpm / totalGrams) * 1000;
            potassiumPpm = (potassiumPpm / totalGrams) * 1000;
            calciumPpm = (calciumPpm / totalGrams) * 1000;
            magnesiumPpm = (magnesiumPpm / totalGrams) * 1000;
            sulfurPpm = (sulfurPpm / totalGrams) * 1000;
            boronPpm = (boronPpm / totalGrams) * 1000;
            copperPpm = (copperPpm / totalGrams) * 1000;
            ironPpm = (ironPpm / totalGrams) * 1000;
            manganesePpm = (manganesePpm / totalGrams) * 1000;
            molybdenumPpm = (molybdenumPpm / totalGrams) * 1000;
            zincPpm = (zincPpm / totalGrams) * 1000;
            chlorinePpm = (chlorinePpm / totalGrams) * 1000;
            silicaPpm = (silicaPpm / totalGrams) * 1000;
            humicAcidPpm = (humicAcidPpm / totalGrams) * 1000;
            fulvicAcidPpm = (fulvicAcidPpm / totalGrams) * 1000;
        }
    }

    // Mix 1 Properties
    public string Mix1Name => _mix1.Name;
    public int Mix1IngredientCount => _mix1.Ingredients.Count;
    public ObservableCollection<FertilizerQuantity> Mix1Ingredients { get; private set; }

    private double _mix1NitrogenPpm;
    public double Mix1NitrogenPpm
    {
        get => _mix1NitrogenPpm;
        set
        {
            if (_mix1NitrogenPpm != value)
            {
                _mix1NitrogenPpm = value;
                OnPropertyChanged(nameof(Mix1NitrogenPpm));
            }
        }
    }

    private double _mix1PhosphorusPpm;
    public double Mix1PhosphorusPpm
    {
        get => _mix1PhosphorusPpm;
        set
        {
            if (_mix1PhosphorusPpm != value)
            {
                _mix1PhosphorusPpm = value;
                OnPropertyChanged(nameof(Mix1PhosphorusPpm));
            }
        }
    }

    private double _mix1PotassiumPpm;
    public double Mix1PotassiumPpm
    {
        get => _mix1PotassiumPpm;
        set
        {
            if (_mix1PotassiumPpm != value)
            {
                _mix1PotassiumPpm = value;
                OnPropertyChanged(nameof(Mix1PotassiumPpm));
            }
        }
    }

    private double _mix1CalciumPpm;
    public double Mix1CalciumPpm
    {
        get => _mix1CalciumPpm;
        set
        {
            if (_mix1CalciumPpm != value)
            {
                _mix1CalciumPpm = value;
                OnPropertyChanged(nameof(Mix1CalciumPpm));
            }
        }
    }

    private double _mix1MagnesiumPpm;
    public double Mix1MagnesiumPpm
    {
        get => _mix1MagnesiumPpm;
        set
        {
            if (_mix1MagnesiumPpm != value)
            {
                _mix1MagnesiumPpm = value;
                OnPropertyChanged(nameof(Mix1MagnesiumPpm));
            }
        }
    }

    private double _mix1SulfurPpm;
    public double Mix1SulfurPpm
    {
        get => _mix1SulfurPpm;
        set
        {
            if (_mix1SulfurPpm != value)
            {
                _mix1SulfurPpm = value;
                OnPropertyChanged(nameof(Mix1SulfurPpm));
            }
        }
    }

    private double _mix1BoronPpm;
    public double Mix1BoronPpm
    {
        get => _mix1BoronPpm;
        set
        {
            if (_mix1BoronPpm != value)
            {
                _mix1BoronPpm = value;
                OnPropertyChanged(nameof(Mix1BoronPpm));
            }
        }
    }

    private double _mix1CopperPpm;
    public double Mix1CopperPpm
    {
        get => _mix1CopperPpm;
        set
        {
            if (_mix1CopperPpm != value)
            {
                _mix1CopperPpm = value;
                OnPropertyChanged(nameof(Mix1CopperPpm));
            }
        }
    }

    private double _mix1IronPpm;
    public double Mix1IronPpm
    {
        get => _mix1IronPpm;
        set
        {
            if (_mix1IronPpm != value)
            {
                _mix1IronPpm = value;
                OnPropertyChanged(nameof(Mix1IronPpm));
            }
        }
    }

    private double _mix1ManganesePpm;
    public double Mix1ManganesePpm
    {
        get => _mix1ManganesePpm;
        set
        {
            if (_mix1ManganesePpm != value)
            {
                _mix1ManganesePpm = value;
                OnPropertyChanged(nameof(Mix1ManganesePpm));
            }
        }
    }

    private double _mix1MolybdenumPpm;
    public double Mix1MolybdenumPpm
    {
        get => _mix1MolybdenumPpm;
        set
        {
            if (_mix1MolybdenumPpm != value)
            {
                _mix1MolybdenumPpm = value;
                OnPropertyChanged(nameof(Mix1MolybdenumPpm));
            }
        }
    }

    private double _mix1ZincPpm;
    public double Mix1ZincPpm
    {
        get => _mix1ZincPpm;
        set
        {
            if (_mix1ZincPpm != value)
            {
                _mix1ZincPpm = value;
                OnPropertyChanged(nameof(Mix1ZincPpm));
            }
        }
    }

    private double _mix1ChlorinePpm;
    public double Mix1ChlorinePpm
    {
        get => _mix1ChlorinePpm;
        set
        {
            if (_mix1ChlorinePpm != value)
            {
                _mix1ChlorinePpm = value;
                OnPropertyChanged(nameof(Mix1ChlorinePpm));
            }
        }
    }

    private double _mix1SilicaPpm;
    public double Mix1SilicaPpm
    {
        get => _mix1SilicaPpm;
        set
        {
            if (_mix1SilicaPpm != value)
            {
                _mix1SilicaPpm = value;
                OnPropertyChanged(nameof(Mix1SilicaPpm));
            }
        }
    }

    private double _mix1HumicAcidPpm;
    public double Mix1HumicAcidPpm
    {
        get => _mix1HumicAcidPpm;
        set
        {
            if (_mix1HumicAcidPpm != value)
            {
                _mix1HumicAcidPpm = value;
                OnPropertyChanged(nameof(Mix1HumicAcidPpm));
            }
        }
    }

    private double _mix1FulvicAcidPpm;
    public double Mix1FulvicAcidPpm
    {
        get => _mix1FulvicAcidPpm;
        set
        {
            if (_mix1FulvicAcidPpm != value)
            {
                _mix1FulvicAcidPpm = value;
                OnPropertyChanged(nameof(Mix1FulvicAcidPpm));
            }
        }
    }

    // Mix 2 Properties
    public string Mix2Name => _mix2.Name;
    public int Mix2IngredientCount => _mix2.Ingredients.Count;
    public ObservableCollection<FertilizerQuantity> Mix2Ingredients { get; private set; }

    private double _mix2NitrogenPpm;
    public double Mix2NitrogenPpm
    {
        get => _mix2NitrogenPpm;
        set
        {
            if (_mix2NitrogenPpm != value)
            {
                _mix2NitrogenPpm = value;
                OnPropertyChanged(nameof(Mix2NitrogenPpm));
            }
        }
    }

    private double _mix2PhosphorusPpm;
    public double Mix2PhosphorusPpm
    {
        get => _mix2PhosphorusPpm;
        set
        {
            if (_mix2PhosphorusPpm != value)
            {
                _mix2PhosphorusPpm = value;
                OnPropertyChanged(nameof(Mix2PhosphorusPpm));
            }
        }
    }

    private double _mix2PotassiumPpm;
    public double Mix2PotassiumPpm
    {
        get => _mix2PotassiumPpm;
        set
        {
            if (_mix2PotassiumPpm != value)
            {
                _mix2PotassiumPpm = value;
                OnPropertyChanged(nameof(Mix2PotassiumPpm));
            }
        }
    }

    private double _mix2CalciumPpm;
    public double Mix2CalciumPpm
    {
        get => _mix2CalciumPpm;
        set
        {
            if (_mix2CalciumPpm != value)
            {
                _mix2CalciumPpm = value;
                OnPropertyChanged(nameof(Mix2CalciumPpm));
            }
        }
    }

    private double _mix2MagnesiumPpm;
    public double Mix2MagnesiumPpm
    {
        get => _mix2MagnesiumPpm;
        set
        {
            if (_mix2MagnesiumPpm != value)
            {
                _mix2MagnesiumPpm = value;
                OnPropertyChanged(nameof(Mix2MagnesiumPpm));
            }
        }
    }

    private double _mix2SulfurPpm;
    public double Mix2SulfurPpm
    {
        get => _mix2SulfurPpm;
        set
        {
            if (_mix2SulfurPpm != value)
            {
                _mix2SulfurPpm = value;
                OnPropertyChanged(nameof(Mix2SulfurPpm));
            }
        }
    }

    private double _mix2BoronPpm;
    public double Mix2BoronPpm
    {
        get => _mix2BoronPpm;
        set
        {
            if (_mix2BoronPpm != value)
            {
                _mix2BoronPpm = value;
                OnPropertyChanged(nameof(Mix2BoronPpm));
            }
        }
    }

    private double _mix2CopperPpm;
    public double Mix2CopperPpm
    {
        get => _mix2CopperPpm;
        set
        {
            if (_mix2CopperPpm != value)
            {
                _mix2CopperPpm = value;
                OnPropertyChanged(nameof(Mix2CopperPpm));
            }
        }
    }

    private double _mix2IronPpm;
    public double Mix2IronPpm
    {
        get => _mix2IronPpm;
        set
        {
            if (_mix2IronPpm != value)
            {
                _mix2IronPpm = value;
                OnPropertyChanged(nameof(Mix2IronPpm));
            }
        }
    }

    private double _mix2ManganesePpm;
    public double Mix2ManganesePpm
    {
        get => _mix2ManganesePpm;
        set
        {
            if (_mix2ManganesePpm != value)
            {
                _mix2ManganesePpm = value;
                OnPropertyChanged(nameof(Mix2ManganesePpm));
            }
        }
    }

    private double _mix2MolybdenumPpm;
    public double Mix2MolybdenumPpm
    {
        get => _mix2MolybdenumPpm;
        set
        {
            if (_mix2MolybdenumPpm != value)
            {
                _mix2MolybdenumPpm = value;
                OnPropertyChanged(nameof(Mix2MolybdenumPpm));
            }
        }
    }

    private double _mix2ZincPpm;
    public double Mix2ZincPpm
    {
        get => _mix2ZincPpm;
        set
        {
            if (_mix2ZincPpm != value)
            {
                _mix2ZincPpm = value;
                OnPropertyChanged(nameof(Mix2ZincPpm));
            }
        }
    }

    private double _mix2ChlorinePpm;
    public double Mix2ChlorinePpm
    {
        get => _mix2ChlorinePpm;
        set
        {
            if (_mix2ChlorinePpm != value)
            {
                _mix2ChlorinePpm = value;
                OnPropertyChanged(nameof(Mix2ChlorinePpm));
            }
        }
    }

    private double _mix2SilicaPpm;
    public double Mix2SilicaPpm
    {
        get => _mix2SilicaPpm;
        set
        {
            if (_mix2SilicaPpm != value)
            {
                _mix2SilicaPpm = value;
                OnPropertyChanged(nameof(Mix2SilicaPpm));
            }
        }
    }

    private double _mix2HumicAcidPpm;
    public double Mix2HumicAcidPpm
    {
        get => _mix2HumicAcidPpm;
        set
        {
            if (_mix2HumicAcidPpm != value)
            {
                _mix2HumicAcidPpm = value;
                OnPropertyChanged(nameof(Mix2HumicAcidPpm));
            }
        }
    }

    private double _mix2FulvicAcidPpm;
    public double Mix2FulvicAcidPpm
    {
        get => _mix2FulvicAcidPpm;
        set
        {
            if (_mix2FulvicAcidPpm != value)
            {
                _mix2FulvicAcidPpm = value;
                OnPropertyChanged(nameof(Mix2FulvicAcidPpm));
            }
        }
    }

    // Unit conversion properties
    public bool UseImperialUnits
    {
        get => _useImperialUnits;
        set
        {
            if (_useImperialUnits != value)
            {
                _useImperialUnits = value;
                OnPropertyChanged(nameof(UseImperialUnits));
                OnPropertyChanged(nameof(UnitLabel));
                CalculateAllNutrients(); // Recalculate with new unit settings
            }
        }
    }

    public string UnitLabel => UseImperialUnits ? "PPM (per gallon)" : "PPM (per liter)";

    // Command to close the page
    public ICommand CloseCommand { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
