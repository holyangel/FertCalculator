using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace FertCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Fertilizer
    {
        public string Name { get; set; }
        public double N { get; set; }
        public double P { get; set; }
        public double K { get; set; }
        public double Mg { get; set; }
        public double Ca { get; set; }
        public double S { get; set; }
        public double Fe { get; set; }
        public double Zn { get; set; }
        public double B { get; set; }
        public double Mn { get; set; }
        public double Cu { get; set; }
        public double Mo { get; set; }
        public double TotalPPM { get; set; }
    }

    [XmlRoot("MixCollection")]
    public class MixCollection
    {
        [XmlElement("Mix")]
        public List<Mix> Mixes { get; set; } = new List<Mix>();
    }

    public class Mix
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlArray("Fertilizers"), XmlArrayItem("Fertilizer")]
        public List<FertilizerQuantity> FertilizerQuantities { get; set; } = new List<FertilizerQuantity>();
    }

    public class FertilizerQuantity
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("quantity")]
        public double Quantity { get; set; }
    }

    public partial class MainWindow : Window
    {
        private Dictionary<string, Fertilizer> fertilizers = new Dictionary<string, Fertilizer>();
        private Dictionary<string, Dictionary<string, double>> savedMixes = new Dictionary<string, Dictionary<string, double>>();
        private const string PlaceholderText = "Enter mix name"; // Your placeholder text
        private string mixesFilePath = "UserMixes.xml"; // Path to save the mixes

        public MainWindow()
        {
            InitializeComponent();
            SaveMixNameTextBox.Text = PlaceholderText; // Initialize with placeholder text
            InitializeFertilizers();
            LoadMixesFromFile();
            PopulateMixesComboBox(); // Existing method to populate predefined mixes
            SetComparisonVisibility(Visibility.Collapsed); // Hide comparison elements initially
            PopulateComparisonMixesComboBox(); // Ensure the comparison ComboBox is also populated
            UpdateUnitToggleButtonLabel();

            // Attach event handlers to Check Boxes
            BioBizzAlgamicCheck.Checked += UpdateNutrientValues;
            BioBizzBloomCheck.Checked += UpdateNutrientValues;
            BioBizzGrowCheck.Checked += UpdateNutrientValues;
            CalciumNitrateCheck.Checked += UpdateNutrientValues;
            CaliMagicCheck.Checked += UpdateNutrientValues;
            CannaBoostCheck.Checked += UpdateNutrientValues;
            CannaCalmagCheck.Checked += UpdateNutrientValues;
            CannaCocoACheck.Checked += UpdateNutrientValues;
            CannaCocoBCheck.Checked += UpdateNutrientValues;
            CannaFloresACheck.Checked += UpdateNutrientValues;
            CannaFloresBCheck.Checked += UpdateNutrientValues;
            CannaPHDownCheck.Checked += UpdateNutrientValues;
            CannaPK1314Check.Checked += UpdateNutrientValues;
            CannaVegaACheck.Checked += UpdateNutrientValues;
            CannaVegaBCheck.Checked += UpdateNutrientValues;
            CannaStartCheck.Checked += UpdateNutrientValues;
            Chempak3Check.Checked += UpdateNutrientValues;
            Chempak8Check.Checked += UpdateNutrientValues;
            ChempakSeqIronCheck.Checked += UpdateNutrientValues;
            DryPartBloomCheck.Checked += UpdateNutrientValues;
            DynaBloomCheck.Checked += UpdateNutrientValues;
            DynaGrowCheck.Checked += UpdateNutrientValues;
            DynaMagProCheck.Checked += UpdateNutrientValues;
            EpsomSaltCheck.Checked += UpdateNutrientValues;
            GypsumCheck.Checked += UpdateNutrientValues;
            Jacks01226Check.Checked += UpdateNutrientValues;
            Jacks51226Check.Checked += UpdateNutrientValues;
            Jacks55018Check.Checked += UpdateNutrientValues;
            Jacks71530Check.Checked += UpdateNutrientValues;
            Jacks103020Check.Checked += UpdateNutrientValues;
            Jacks12416Check.Checked += UpdateNutrientValues;
            Jacks1500Check.Checked += UpdateNutrientValues;
            Jacks15520Check.Checked += UpdateNutrientValues;
            Jacks15617Check.Checked += UpdateNutrientValues;
            Jacks18823Check.Checked += UpdateNutrientValues;
            Jacks201020Check.Checked += UpdateNutrientValues;
            Jacks202020Check.Checked += UpdateNutrientValues;
            KTrateLXCheck.Checked += UpdateNutrientValues;
            KoolbloomCheck.Checked += UpdateNutrientValues;
            LKoolbloomCheck.Checked += UpdateNutrientValues;
            MagNitCheck.Checked += UpdateNutrientValues;
            MagTrateLXCheck.Checked += UpdateNutrientValues;
            MAPCheck.Checked += UpdateNutrientValues;
            MaxiBloomCheck.Checked += UpdateNutrientValues;
            MaxiGrowCheck.Checked += UpdateNutrientValues;
            MegacropCheck.Checked += UpdateNutrientValues;
            MegacropACheck.Checked += UpdateNutrientValues;
            MegacropBCheck.Checked += UpdateNutrientValues;
            MCCalMagCheck.Checked += UpdateNutrientValues;
            MCSweetCandyCheck.Checked += UpdateNutrientValues;
            MOABCheck.Checked += UpdateNutrientValues;
            MonsterBloomCheck.Checked += UpdateNutrientValues;
            MPKCheck.Checked += UpdateNutrientValues;
            PPBloomCheck.Checked += UpdateNutrientValues;
            PPBoostCheck.Checked += UpdateNutrientValues;
            PPCalKickCheck.Checked += UpdateNutrientValues;
            PPFinisherCheck.Checked += UpdateNutrientValues;
            PPGrowCheck.Checked += UpdateNutrientValues;
            PPSpikeCheck.Checked += UpdateNutrientValues;
            VitalinkCalmagCheck.Checked += UpdateNutrientValues;
            // Add event handlers for other modifier Check Boxes

            // Attach event handlers to quantity input boxes
            BioBizzAlgamicBox.TextChanged += UpdateNutrientValues;
            BioBizzBloomBox.TextChanged += UpdateNutrientValues;
            BioBizzGrowBox.TextChanged += UpdateNutrientValues;
            CalciumNitrateBox.TextChanged += UpdateNutrientValues;
            CaliMagicBox.TextChanged += UpdateNutrientValues;
            CannaBoostBox.TextChanged += UpdateNutrientValues;
            CannaCalmagBox.TextChanged += UpdateNutrientValues;
            CannaCocoABox.TextChanged += UpdateNutrientValues;
            CannaCocoBBox.TextChanged += UpdateNutrientValues;
            CannaFloresABox.TextChanged += UpdateNutrientValues;
            CannaFloresBBox.TextChanged += UpdateNutrientValues;
            CannaPHDownBox.TextChanged += UpdateNutrientValues;
            CannaPK1314Box.TextChanged += UpdateNutrientValues;
            CannaVegaABox.TextChanged += UpdateNutrientValues;
            CannaVegaBBox.TextChanged += UpdateNutrientValues;
            CannaStartBox.TextChanged += UpdateNutrientValues;
            Chempak3Box.TextChanged += UpdateNutrientValues;
            Chempak8Box.TextChanged += UpdateNutrientValues;
            ChempakSeqIronBox.TextChanged += UpdateNutrientValues;
            DryPartBloomBox.TextChanged += UpdateNutrientValues;
            DynaBloomBox.TextChanged += UpdateNutrientValues;
            DynaGrowBox.TextChanged += UpdateNutrientValues;
            DynaMagProBox.TextChanged += UpdateNutrientValues;
            EpsomSaltBox.TextChanged += UpdateNutrientValues;
            GypsumBox.TextChanged += UpdateNutrientValues;
            Jacks01226Box.TextChanged += UpdateNutrientValues;
            Jacks51226Box.TextChanged += UpdateNutrientValues;
            Jacks55018Box.TextChanged += UpdateNutrientValues;
            Jacks71530Box.TextChanged += UpdateNutrientValues;
            Jacks103020Box.TextChanged += UpdateNutrientValues;
            Jacks12416Box.TextChanged += UpdateNutrientValues;
            Jacks1500Box.TextChanged += UpdateNutrientValues;
            Jacks15520Box.TextChanged += UpdateNutrientValues;
            Jacks15617Box.TextChanged += UpdateNutrientValues;
            Jacks18823Box.TextChanged += UpdateNutrientValues;
            Jacks201020Box.TextChanged += UpdateNutrientValues;
            Jacks202020Box.TextChanged += UpdateNutrientValues;
            KTrateLXBox.TextChanged += UpdateNutrientValues;
            KoolbloomBox.TextChanged += UpdateNutrientValues;
            LKoolbloomBox.TextChanged += UpdateNutrientValues;
            MagNitBox.TextChanged += UpdateNutrientValues;
            MagTrateLXBox.TextChanged += UpdateNutrientValues;
            MAPBox.TextChanged += UpdateNutrientValues;
            MaxiBloomBox.TextChanged += UpdateNutrientValues;
            MaxiGrowBox.TextChanged += UpdateNutrientValues;
            MegacropBox.TextChanged += UpdateNutrientValues;
            MegacropABox.TextChanged += UpdateNutrientValues;
            MegacropBBox.TextChanged += UpdateNutrientValues;
            MCCalMagBox.TextChanged += UpdateNutrientValues;
            MCSweetCandyBox.TextChanged += UpdateNutrientValues;
            MOABBox.TextChanged += UpdateNutrientValues;
            MonsterBloomBox.TextChanged += UpdateNutrientValues;
            MPKBox.TextChanged += UpdateNutrientValues;
            PPBloomBox.TextChanged += UpdateNutrientValues;
            PPBoostBox.TextChanged += UpdateNutrientValues;
            PPCalKickBox.TextChanged += UpdateNutrientValues;
            PPFinisherBox.TextChanged += UpdateNutrientValues;
            PPGrowBox.TextChanged += UpdateNutrientValues;
            PPSpikeBox.TextChanged += UpdateNutrientValues;
            VitalinkCalmagBox.TextChanged += UpdateNutrientValues;
            // Add event handlers for other quantity input boxes

            // Disable all quantity TextBoxes initially
            DisableAllQuantityTextBoxes();
        }

        private void SaveMixNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SaveMixNameTextBox.Text == PlaceholderText)
            {
                SaveMixNameTextBox.Text = ""; // Clear the placeholder text
                SaveMixNameTextBox.Foreground = Brushes.Black; // Reset text color to default
            }
        }

        private void SaveMixNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SaveMixNameTextBox.Text))
            {
                SaveMixNameTextBox.Text = PlaceholderText; // Reset to placeholder text
                SaveMixNameTextBox.Foreground = Brushes.Gray; // Set text color to gray to indicate placeholder
            }
        }

        private void DisableAllCheckBoxes()
        {
            var checkBoxes = FindVisualChildren<CheckBox>(this); // 'this' refers to the current window

            foreach (var checkBox in checkBoxes)
            {
                if (checkBox != null)
                {
                    checkBox.IsChecked = false;
                }
            }
        }

        private void DisableAllQuantityCheckBoxes()
        {
            var checkBoxes = FindVisualChildren<CheckBox>(this); // 'this' refers to the current window

            // Determine if a comparison mix is selected and should be preserved
            bool preserveComparison = ComparisonMixesComboBox.SelectedIndex > 1; // Assuming index 0 is "Select a Mix to Compare", and index 1 is "Reset"

            foreach (var checkBox in checkBoxes)
            {
                if (checkBox != null)
                {
                    // If preserveComparison is true, check if the current checkbox is the comparison checkbox and skip unchecking it
                    if (!(preserveComparison && checkBox == ShowComparisonCheckBox))
                    {
                        checkBox.IsChecked = false;
                    }
                }
            }
        }

        // Helper method to find all children of a specific type in the visual tree
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void DisableAllQuantityTextBoxes()
        {
            // Explicitly disable all quantity TextBoxes
            BioBizzAlgamicBox.IsEnabled = false;
            BioBizzBloomBox.IsEnabled = false;
            BioBizzGrowBox.IsEnabled = false;
            CalciumNitrateBox.IsEnabled = false;
            CaliMagicBox.IsEnabled = false;
            CannaBoostBox.IsEnabled = false;
            CannaCalmagBox.IsEnabled = false;
            CannaCocoABox.IsEnabled = false;
            CannaCocoBBox.IsEnabled = false;
            CannaFloresABox.IsEnabled = false;
            CannaFloresBBox.IsEnabled = false;
            CannaPHDownBox.IsEnabled = false;
            CannaPK1314Box.IsEnabled = false;
            CannaVegaABox.IsEnabled = false;
            CannaVegaBBox.IsEnabled = false;
            CannaBoostBox.IsEnabled = false;
            Chempak3Box.IsEnabled = false;
            Chempak8Box.IsEnabled = false;
            ChempakSeqIronBox.IsEnabled = false;
            DryPartBloomBox.IsEnabled = false;
            DynaBloomBox.IsEnabled = false;
            DynaGrowBox.IsEnabled = false;
            DynaMagProBox.IsEnabled = false;
            EpsomSaltBox.IsEnabled = false;
            GypsumBox.IsEnabled = false;
            Jacks01226Box.IsEnabled = false;
            Jacks51226Box.IsEnabled = false;
            Jacks55018Box.IsEnabled = false;
            Jacks71530Box.IsEnabled = false;
            Jacks103020Box.IsEnabled = false;
            Jacks12416Box.IsEnabled = false;
            Jacks1500Box.IsEnabled = false;
            Jacks15520Box.IsEnabled = false;
            Jacks15617Box.IsEnabled = false;
            Jacks18823Box.IsEnabled = false;
            Jacks201020Box.IsEnabled = false;
            Jacks202020Box.IsEnabled = false;
            KTrateLXBox.IsEnabled = false;
            KoolbloomBox.IsEnabled = false;
            LKoolbloomBox.IsEnabled = false;
            MagNitBox.IsEnabled = false;
            MagTrateLXBox.IsEnabled = false;
            MAPBox.IsEnabled = false;
            MaxiBloomBox.IsEnabled = false;
            MaxiGrowBox.IsEnabled = false;
            MegacropBox.IsEnabled = false;
            MegacropABox.IsEnabled = false;
            MegacropBBox.IsEnabled = false;
            MCCalMagBox.IsEnabled = false;
            MCSweetCandyBox.IsEnabled = false;
            MOABBox.IsEnabled = false;
            MonsterBloomBox.IsEnabled = false;
            MPKBox.IsEnabled = false;
            PPBloomBox.IsEnabled = false;
            PPBoostBox.IsEnabled = false;
            PPCalKickBox.IsEnabled = false;
            PPFinisherBox.IsEnabled = false;
            PPGrowBox.IsEnabled = false;
            PPSpikeBox.IsEnabled = false;
            VitalinkCalmagBox.IsEnabled = false;
            // Repeat for other TextBoxes as necessary
        }

        private void ShowComparisonCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Make the ComboBox visible
            ComparisonMixesComboBox.Visibility = Visibility.Visible;
            // Make all comparison TextBoxes and Labels visible
            SetComparisonVisibility(Visibility.Visible);
        }

        private void ShowComparisonCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Reset the values
            ClearComparisonNutrientValues();
            // Reset the selection
            ComparisonMixesComboBox.SelectedIndex = 0;
            // Hide the ComboBox
            ComparisonMixesComboBox.Visibility = Visibility.Collapsed;
            // Hide all comparison TextBoxes and Labels
            SetComparisonVisibility(Visibility.Collapsed);
        }

        private void SetComparisonVisibility(Visibility visibility)
        {
            // Update the visibility of all comparison-related UI elements
            ComparisonMixesComboBox.Visibility = visibility;
            CompareTotalNitrogenBox.Visibility = visibility;
            ComparePhosphorousBox.Visibility = visibility;
            ComparePotassiumBox.Visibility = visibility;
            CompareMagnesiumBox.Visibility = visibility;
            CompareCalciumBox.Visibility = visibility;
            CompareSulfurBox.Visibility = visibility;
            CompareIronBox.Visibility = visibility;
            CompareZincBox.Visibility = visibility;
            CompareBoronBox.Visibility = visibility;
            CompareManganeseBox.Visibility = visibility;
            CompareCopperBox.Visibility = visibility;
            CompareMolybdenumBox.Visibility = visibility;
            CompareTotalPPMBox.Visibility = visibility;
            // Also, don't forget to adjust for any labels or additional UI elements related to comparison
            CompareMix.Visibility = visibility;
        }

        private void DecimalInputValidation(object sender, TextCompositionEventArgs e)
        {
            // Regex to match numeric input and a single decimal point
            var regex = new Regex(@"[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);

            // Check to prevent more than one decimal point
            var textBox = sender as TextBox;
            if (textBox != null && e.Text == "." && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Allow control keys such as Backspace and Delete
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right)
            {
                return;
            }

            // Prevent pasting of non-numeric values
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var textBox = sender as TextBox;
                string clipboard = Clipboard.GetText();
                var regex = new Regex(@"[^0-9.]+");
                if (regex.IsMatch(clipboard) || clipboard.Count(f => f == '.') > 1)
                {
                    e.Handled = true;
                }
            }
        }

        private string SanitizeFertilizerName(string name)
        {
            // Replace forward slash, backward slash, and hyphen with an underscore
            return name.Replace("/", "_").Replace("\\", "_").Replace("-", "").Replace(" ", "").Replace("'", "").Replace(".", "");
        }

        private void InitializeFertilizers()
        {
            fertilizers.Add("BioBizz Algamic", new Fertilizer { Name = "BioBizz Algamic", N = 2, P = 0.873, K = 0.83, Mg = 0.04, Ca = 0.12, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0.018, Cu = 0, Mo = 0, TotalPPM = 3.881 });
            fertilizers.Add("BioBizz Bloom", new Fertilizer { Name = "BioBizz Bloom", N = 20, P = 28.3, K = 36.2, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 84.5 });
            fertilizers.Add("BioBizz Grow", new Fertilizer { Name = "BioBizz Grow", N = 40, P = 13.1, K = 49.8, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 102.9 });
            fertilizers.Add("Calcium Nitrate", new Fertilizer { Name = "Calcium Nitrate", N = 40.95, P = 0, K = 0, Mg = 0, Ca = 50.19, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 91.14 });
            fertilizers.Add("CaliMagic", new Fertilizer { Name = "CaliMagic", N = 2.64, P = 0, K = 0, Mg = 3.962, Ca = 13.208, S = 0, Fe = 0.264, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 20.06 });
            fertilizers.Add("Canna Boost", new Fertilizer { Name = "Canna Boost", N = 0, P = 1.153, K = 2.193, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 3.34 });
            fertilizers.Add("Canna Calmag", new Fertilizer { Name = "Canna Calmag", N = 8.242, P = 0, K = 0, Mg = 3.3497, Ca = 9.51, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 21.1017 });
            fertilizers.Add("CannaCoco A", new Fertilizer { Name = "CannaCoco A", N = 10.0039, P = 0, K = 1.32, Mg = 1.06, Ca = 11.9, S = 0, Fe = 0.0528, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 24.3328 });
            fertilizers.Add("CannaCoco B", new Fertilizer { Name = "CannaCoco B", N = 2.64, P = 4.61, K = 4.39, Mg = 2.91, Ca = 0, S = 1.85, Fe = 0, Zn = 0.0185, B = 0.0185, Mn = 0.0317, Cu = 0.00264, Mo = 0.00528, TotalPPM = 16.47662 });
            fertilizers.Add("Canna Flores A", new Fertilizer { Name = "Canna Flores A", N = 7.925, P = 0, K = 13.158, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 21.083 });
            fertilizers.Add("Canna Flores B", new Fertilizer { Name = "Canna Flores B", N = 0, P = 4.612, K = 6.579, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 11.191 });
            fertilizers.Add("Canna PH Down", new Fertilizer { Name = "Canna PH Down", N = 2.64, P = 0, K = 0, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 2.64 });
            fertilizers.Add("Canna PK13/14", new Fertilizer { Name = "Canna PK13/14", N = 0, P = 11.53, K = 24.123, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 35.6 });
            fertilizers.Add("Canna Vega A", new Fertilizer { Name = "Canna Vega A", N = 13.2, P = 0, K = 4.39, Mg = 0.5581, Ca = 2.2333, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 12.0867 });
            fertilizers.Add("Canna Vega B", new Fertilizer { Name = "Canna Vega B", N = 0, P = 3.459, K = 8.772, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 12.231 });
            fertilizers.Add("Canna Start", new Fertilizer { Name = "Canna Start", N = 3.434, P = 1.1517, K = 3.936, Mg = 0.7925, Ca = 2.9059, S = 0, Fe = 0.05283, Zn = 0.01849, B = 0.01849, Mn = 0.02641, Cu = 0.002641, Mo = 0.005283, TotalPPM = 12.23442 });
            fertilizers.Add("Chempak 3", new Fertilizer { Name = "Chempak 3", N = 52.8345, P = 23.0622, K = 43.85267, Mg = 0, Ca = 0, S = 0, Fe = 0.528345, Zn = 0.132086, B = 0.052834, Mn = 0.052834, Cu = 0.02641, Mo = 0.0052834, TotalPPM = 120.54724 });
            fertilizers.Add("Chempak 8", new Fertilizer { Name = "Chempak 8", N = 33.0216, P = 28.79484, K = 54.94795, Mg = 0, Ca = 0, S = 0, Fe = 0.52834, Zn = 0.132086, B = 0.052834, Mn = 0.052834, Cu = 0.02641, Mo = 0, TotalPPM = 117.556894 });
            fertilizers.Add("Chempak Seq.Iron", new Fertilizer { Name = "Chempak Seq.Iron", N = 0, P = 0, K = 0, Mg = 26.417287, Ca = 0, S = 0, Fe = 5.283457, Zn = 0, B = 0, Mn = 7.925186, Cu = 0, Mo = 0, TotalPPM = 39.62593 });
            fertilizers.Add("DryPart Bloom", new Fertilizer { Name = "DryPart Bloom", N = 15.8, P = 19.6, K = 32.9, Mg = 18.5, Ca = 18.5, S = 37, Fe = 0.291, Zn = 0.0396, B = 0.037, Mn = 0.0687, Cu = 0.0396, Mo = 0.0528, TotalPPM = 142.8287 });
            fertilizers.Add("Dyna-Bloom", new Fertilizer { Name = "Dyna-Bloom", N = 7.93, P = 13.8, K = 13.2, Mg = 1.32, Ca = 5.28, S = 0.238, Fe = 0.264, Zn = 0.132, B = 0.0528, Mn = 0.132, Cu = 0.132, Mo = 0.00238, TotalPPM = 42.48318 });
            fertilizers.Add("Dyna-Grow", new Fertilizer { Name = "Dyna-Grow", N = 18.5, P = 10.4, K = 11, Mg = 1.32, Ca = 5.28, S = 1.32, Fe = 0.264, Zn = 0.132, B = 0.0528, Mn = 0.132, Cu = 0.132, Mo = 0.00238, TotalPPM = 43.52518 });
            fertilizers.Add("Dyna-MagPro", new Fertilizer { Name = "Dyna-MagPro", N = 5.28, P = 17.3, K = 8.77, Mg = 5.28, Ca = 0, S = 3.96, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 40.59 });
            fertilizers.Add("Epsom Salt", new Fertilizer { Name = "Epsom Salt", N = 0, P = 0, K = 0, Mg = 26, Ca = 0, S = 103, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 129 });
            fertilizers.Add("Gypsum", new Fertilizer { Name = "Gypsum", N = 0, P = 0, K = 0, Mg = 0, Ca = 58.12, S = 47.55, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 105.67 });
            fertilizers.Add("Jacks 0-12-26", new Fertilizer { Name = "Jacks 0-12-26", N = 0, P = 13.8, K = 57, Mg = 15.8, Ca = 0, S = 34.3, Fe = 0.792, Zn = 0.0396, B = 0.132, Mn = 0.132, Cu = 0.0396, Mo = 0.0238, TotalPPM = 122.059 });
            fertilizers.Add("Jacks 5-12-26", new Fertilizer { Name = "Jacks 5-12-26", N = 13.209, P = 13.835, K = 57.02, Mg = 16.644, Ca = 0, S = 22.456, Fe = 0.7925, Zn = 0.0396, B = 0.1321, Mn = 0.132, Cu = 0.0396, Mo = 0.0502, TotalPPM = 124.35 });
            fertilizers.Add("Jacks 5-50-18", new Fertilizer { Name = "Jacks 5-50-18", N = 13.2, P = 57.6, K = 39.5, Mg = 2.64, Ca = 0, S = 3.7, Fe = 0.132, Zn = 0.00528, B = 0.0264, Mn = 0.0528, Cu = 0.0106, Mo = 0.00264, TotalPPM = 116.86972 });
            fertilizers.Add("Jacks 7-15-30", new Fertilizer { Name = "Jacks 7-15-30", N = 18.5, P = 17.3, K = 65.8, Mg = 5.283, Ca = 0, S = 25.626, Fe = 0.185, Zn = 0.132, B = 0.0528, Mn = 0.132, Cu = 0.132, Mo = 0.005283, TotalPPM = 133.148083 });
            fertilizers.Add("Jacks 10-30-20", new Fertilizer { Name = "Jacks 10-30-20", N = 26.42, P = 34.59, K = 43.86, Mg = 1.321, Ca = 0, S = 0, Fe = 0.265, Zn = 0.132, B = 0.0528, Mn = 0.132, Cu = 0.132, Mo = 0.00265, TotalPPM = 106.90745 });
            fertilizers.Add("Jacks 12-4-16", new Fertilizer { Name = "Jacks 12-4-16", N = 31.7, P = 4.61, K = 35.1, Mg = 5.28, Ca = 18.5, S = 0, Fe = 0.396, Zn = 0.0925, B = 0.0528, Mn = 0.132, Cu = 0.0528, Mo = 0.00264, TotalPPM = 95.91874 });
            fertilizers.Add("Jacks 15-0-0", new Fertilizer { Name = "Jacks 15-0-0", N = 39.623, P = 0, K = 0, Mg = 0, Ca = 47.55, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 87.173 });
            fertilizers.Add("Jacks 15-5-20", new Fertilizer { Name = "Jacks 15-5-20", N = 39.622, P = 5.76, K = 43.9, Mg = 3.96, Ca = 7.92, S = 0, Fe = 0.396, Zn = 0.132, B = 0.0528, Mn = 0.211, Cu = 0.0528, Mo = 0.00264, TotalPPM = 102.00924 });
            fertilizers.Add("Jacks 15-6-17", new Fertilizer { Name = "Jacks 15-6-17", N = 39.622, P = 6.917, K = 37.28, Mg = 4.755, Ca = 10.567, S = 0, Fe = 0.6076, Zn = 0.13208, B = 0.0528, Mn = 0.132, Cu = 0.026415, Mo = 0.023774, TotalPPM = 100.115669 });
            fertilizers.Add("Jacks 18-8-23", new Fertilizer { Name = "Jacks 18-8-23", N = 47.55, P = 9.22, K = 50.4, Mg = 1.32, Ca = 0, S = 4.2, Fe = 0.396, Zn = 0.132, B = 0.0528, Mn = 0.132, Cu = 0.0528, Mo = 0.00264, TotalPPM = 113.45824 });
            fertilizers.Add("Jacks 20-10-20", new Fertilizer { Name = "Jacks 20-10-20", N = 52.89, P = 11.53, K = 43.86, Mg = 2.0013, Ca = 0, S = 12.945, Fe = 0.39626, Zn = 0.132, B = 0.0528, Mn = 0.1981, Cu = 0.132, Mo = 0.0023775, TotalPPM = 124.1398375 });
            fertilizers.Add("Jacks 20-20-20", new Fertilizer { Name = "Jacks 20-20-20", N = 52.89, P = 23.06, K = 43.86, Mg = 0, Ca = 0, S = 0, Fe = 0.265, Zn = 0.132, B = 0.0528, Mn = 0.132, Cu = 0.132, Mo = 0.00265, TotalPPM = 120.52645 });
            fertilizers.Add("K-Trate LX", new Fertilizer { Name = "K-Trate LX", N = 37, P = 5.76, K = 83.3, Mg = 0, Ca = 0, S = 0, Fe = 0.185, Zn = 0.0925, B = 0.037, Mn = 0.0925, Cu = 0.0185, Mo = 0.0185, TotalPPM = 126.504 });
            fertilizers.Add("MagNit", new Fertilizer { Name = "MagNit", N = 29.1, P = 0, K = 0, Mg = 25.4, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 54.5 });
            fertilizers.Add("Mag-Trate LX", new Fertilizer { Name = "Mag-Trate LX", N = 26.4, P = 0, K = 0, Mg = 23.8, Ca = 0, S = 0, Fe = 0.132, Zn = 0.066, B = 0.01, Mn = 0.025, Cu = 0.005, Mo = 0.005, TotalPPM = 50.443 });
            fertilizers.Add("MAP", new Fertilizer { Name = "MAP", N = 31.7, P = 70.3, K = 0, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 102 });
            fertilizers.Add("Koolbloom", new Fertilizer { Name = "Koolbloom", N = 2, P = 45, K = 28, Mg = 1, Ca = 0, S = 1.5, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0.001, TotalPPM = 77.501 });
            fertilizers.Add("L.Koolbloom", new Fertilizer { Name = "L.Koolbloom", N = 0, P = 11.5, K = 21.9, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 33.4 });
            fertilizers.Add("MaxiBloom", new Fertilizer { Name = "MaxiBloom", N = 13.2, P = 17.2, K = 30.8, Mg = 9.2, Ca = 13.2, S = 10.6, Fe = 0.264, Zn = 0, B = 0, Mn = 0, Cu = 0.015, Mo = 0.000405, TotalPPM = 94.479405 });
            fertilizers.Add("MaxiGrow", new Fertilizer { Name = "MaxiGrow", N = 26.42, P = 5.77, K = 30.7, Mg = 5.3, Ca = 15.9, S = 8, Fe = 0.317, Zn = 0, B = 0, Mn = 0.133, Cu = 0, Mo = 0.005283, TotalPPM = 92.545283 });
            fertilizers.Add("Megacrop", new Fertilizer { Name = "Megacrop", N = 25.705, P = 9.01, K = 33.39, Mg = 7.95, Ca = 21.2, S = 10.6, Fe = 0.3975, Zn = 0.159, B = 0.0689, Mn = 0.0742, Cu = 0.0212, Mo = 0.0345, TotalPPM = 108.6103 });
            fertilizers.Add("Megacrop A", new Fertilizer { Name = "Megacrop A", N = 29.979, P = 14.283, K = 65.3315, Mg = 7.9615, Ca = 0, S = 10.6593, Fe = 0.4629, Zn = 0.1323, B = 0.1349, Mn = 0.611, Cu = 0.0079, Mo = 0.0032, TotalPPM = 129.5665 });
            fertilizers.Add("Megacrop B", new Fertilizer { Name = "Megacrop B", N = 40.9975, P = 0, K = 0, Mg = 0, Ca = 50.255, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 91.2525 });
            fertilizers.Add("MC Cal-Mag", new Fertilizer { Name = "MC Cal-Mag", N = 34.34, P = 0, K = 0, Mg = 9.25, Ca = 29.1, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 72.69 });
            fertilizers.Add("MC Sweet Candy", new Fertilizer { Name = "MC Sweet Candy", N = 0, P = 0, K = 35.1, Mg = 14.5, Ca = 0, S = 34.3, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 83.9 });
            fertilizers.Add("MOAB", new Fertilizer { Name = "MOAB", N = 0, P = 59.95, K = 70.17, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 130.12 });
            fertilizers.Add("Monster Bloom", new Fertilizer { Name = "Monster Bloom", N = 0, P = 57.6, K = 65.8, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 123.4 });
            fertilizers.Add("MPK", new Fertilizer { Name = "MPK", N = 0, P = 59.95, K = 74.56, Mg = 0, Ca = 0, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 134.51 });
            fertilizers.Add("PP CalKick", new Fertilizer { Name = "PP CalKick", N = 39.623, P = 0, K = 30.7, Mg = 29.06, Ca = 0, S = 0, Fe = 0.6604, Zn = 0.1323, B = 0.0528, Mn = 0.1323, Cu = 0.1323, Mo = 0.039625, TotalPPM = 100.532725 });
            fertilizers.Add("PP Bloom", new Fertilizer { Name = "PP Bloom", N = 26.415, P = 34.59, K = 43.86, Mg = 3.434, Ca = 0, S = 4.491, Fe = 0.6604, Zn = 0.1323, B = 0.0528, Mn = 0.1323, Cu = 0.1323, Mo = 0.007925, TotalPPM = 113.908025 });
            fertilizers.Add("PP Boost", new Fertilizer { Name = "PP Boost", N = 39.623, P = 34.59, K = 32.9, Mg = 0, Ca = 0, S = 4.491, Fe = 0.26415, Zn = 0.1323, B = 0.0528, Mn = 0.1323, Cu = 0.1323, Mo = 0.013209, TotalPPM = 112.331059 });
            fertilizers.Add("PP Grow", new Fertilizer { Name = "PP Grow", N = 31.7, P = 9.224, K = 57.02, Mg = 6.604, Ca = 0, S = 13.474, Fe = 0.6604, Zn = 0.1323, B = 0.0528, Mn = 0.1323, Cu = 0.1323, Mo = 0.013209, TotalPPM = 119.145309 });
            fertilizers.Add("PP Finisher", new Fertilizer { Name = "PP Finisher", N = 10.566, P = 35.74, K = 81.14, Mg = 0, Ca = 0, S = 0, Fe = 0.26415, Zn = 0.1323, B = 0.0528, Mn = 0.1323, Cu = 0.1323, Mo = 0.013209, TotalPPM = 128.173059 });
            fertilizers.Add("PP Spike", new Fertilizer { Name = "PP Spike", N = 0, P = 0, K = 0, Mg = 7.132, Ca = 14.265, S = 0, Fe = 0, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 21.397 });
            fertilizers.Add("Vitalink Calmag+", new Fertilizer { Name = "Vitalink Calmag+", N = 4.755, P = 0, K = 0, Mg = 4.49, Ca = 10.566, S = 0, Fe = 0.23775, Zn = 0, B = 0, Mn = 0, Cu = 0, Mo = 0, TotalPPM = 20.04875 });
        }

        private void PopulateMixesComboBox()
        {
            PredefinedMixesComboBox.Items.Clear(); // Clear existing items first

            // Add placeholder as the first item, disabled and selected by default
            var placeholderItem = new ComboBoxItem
            {
                Content = "Select a Made Mix", // Adjust the text as needed
                IsEnabled = false,
                IsSelected = true
            };
            PredefinedMixesComboBox.Items.Add(placeholderItem);

            // Add "Reset" option first
            PredefinedMixesComboBox.Items.Add(new ComboBoxItem { Content = "Reset" });

            // Extract mix names and sort them
            var sortedMixNames = savedMixes.Keys.ToList();
            sortedMixNames.Sort();

            // Add sorted mix names to the ComboBox
            foreach (var mixName in sortedMixNames)
            {
                PredefinedMixesComboBox.Items.Add(new ComboBoxItem { Content = mixName });
            }
        }

        private void PopulateComparisonMixesComboBox()
        {
            // Clear existing items first, except for the first "Select a Mix to Compare" item if it exists
            ComparisonMixesComboBox.Items.Clear();

            // Add placeholder as the first item, disabled and selected by default
            var placeholderItem = new ComboBoxItem
            {
                Content = "Select a Mix to Compare",
                IsEnabled = false,
                IsSelected = true
            };
            ComparisonMixesComboBox.Items.Add(placeholderItem);

            // Add "Reset" option first
            ComparisonMixesComboBox.Items.Add(new ComboBoxItem { Content = "Reset" }); // Add a Reset option

            // Extract mix names and sort them
            var sortedMixNames = savedMixes.Keys.ToList();
            sortedMixNames.Sort();

            // Add sorted mix names to the ComboBox
            foreach (var mixName in sortedMixNames)
            {
                ComparisonMixesComboBox.Items.Add(new ComboBoxItem { Content = mixName });
            }
        }

        private void RefreshMixesInComboBox()
        {
            PredefinedMixesComboBox.Items.Clear(); // Clear existing items
            LoadMixesFromFile(); // Reload mixes from file
            PopulateMixesComboBox(); // Repopulate ComboBox
        }

        private void UpdateNutrientValues(object sender, EventArgs e)
        {
            // Determine the conversion factor based on the current unit preference
            double conversionFactor = Properties.Settings.Default.UnitPreference ? 3.78541 : 1;

            // Reset nutrient values
            double totalNitrogen = 0.0;
            double phosphorous = 0.0;
            double potassium = 0.0;
            double magnesium = 0.0;
            double calcium = 0.0;
            double sulfur = 0.0;
            double iron = 0.0;
            double zinc = 0.0;
            double boron = 0.0;
            double manganese = 0.0;
            double copper = 0.0;
            double molybdenum = 0.0;
            double totalPPM = 0.0;

            // Iterate over all checkboxes to find checked ones and their corresponding quantities
            foreach (var checkBox in this.GetFertilizerCheckBoxes())
            {
                if (checkBox.IsChecked == true)
                {
                    string fertilizerName = checkBox.Content.ToString();
                    double quantity = GetFertilizerQuantity(fertilizerName);

                    if (fertilizers.TryGetValue(fertilizerName, out Fertilizer fertilizer))
                    {
                        // Aggregate nutrient values based on the quantity
                        totalNitrogen += fertilizer.N * quantity;
                        phosphorous += fertilizer.P * quantity;
                        potassium += fertilizer.K * quantity;
                        magnesium += fertilizer.Mg * quantity;
                        calcium += fertilizer.Ca * quantity;
                        sulfur += fertilizer.S * quantity;
                        iron += fertilizer.Fe * quantity;
                        zinc += fertilizer.Zn * quantity;
                        boron += fertilizer.B * quantity;
                        manganese += fertilizer.Mn * quantity;
                        copper += fertilizer.Cu * quantity;
                        molybdenum += fertilizer.Mo * quantity;
                        totalPPM += fertilizer.TotalPPM * quantity;
                    }
                }
            }

            // Update the UI with the aggregated values
            TotalNitrogenBox.Text = $"{(totalNitrogen * conversionFactor):N2}";
            PhosphorousBox.Text = $"{(phosphorous * conversionFactor):N2}";
            PotassiumBox.Text = $"{(potassium * conversionFactor):N2}";
            MagnesiumBox.Text = $"{(magnesium * conversionFactor):N2}";
            CalciumBox.Text = $"{(calcium * conversionFactor):N2}";
            SulfurBox.Text = $"{(sulfur * conversionFactor):N2}";
            IronBox.Text = $"{(iron * conversionFactor):N2}";
            ZincBox.Text = $"{(zinc * conversionFactor):N2}";
            BoronBox.Text = $"{(boron * conversionFactor):N2}";
            ManganeseBox.Text = $"{(manganese * conversionFactor):N2}";
            CopperBox.Text = $"{(copper * conversionFactor):N2}";
            MolybdenumBox.Text = $"{(molybdenum * conversionFactor):N2}";
            TotalPPMBox.Text = $"{(totalPPM * conversionFactor):N2}";

            if (ComparisonMixesComboBox.SelectedItem != null &&
                ComparisonMixesComboBox.SelectedIndex > -1 &&
                ComparisonMixesComboBox.SelectedItem is ComboBoxItem selectedItem &&
                !selectedItem.Content.ToString().Equals("Select a Mix to Compare"))
            {
                ApplyComparisonMix(selectedItem.Content.ToString());
            }
        }

        // Utility method to get all fertilizer CheckBoxes
        private IEnumerable<CheckBox> GetFertilizerCheckBoxes()
        {
            // You might need to adjust this method to correctly find all CheckBoxes
            // depending on your application's structure. This is just a conceptual example.
            return new CheckBox[] { BioBizzAlgamicCheck, BioBizzBloomCheck, BioBizzGrowCheck, CalciumNitrateCheck, CaliMagicCheck, CannaBoostCheck, CannaCalmagCheck, CannaCocoACheck, CannaCocoBCheck, CannaFloresACheck, CannaFloresBCheck, CannaPHDownCheck, CannaPK1314Check, CannaVegaACheck, CannaVegaBCheck, DryPartBloomCheck, DynaBloomCheck, DynaGrowCheck, DynaMagProCheck, EpsomSaltCheck, GypsumCheck, Jacks01226Check, Jacks51226Check, Jacks55018Check, Jacks71530Check, Jacks103020Check, Jacks12416Check, Jacks1500Check, Jacks15520Check, Jacks15617Check, Jacks18823Check, Jacks201020Check, Jacks202020Check, KTrateLXCheck, KoolbloomCheck, LKoolbloomCheck, MagNitCheck, MagTrateLXCheck, MAPCheck, MaxiBloomCheck, MaxiGrowCheck, MegacropCheck, MegacropACheck, MegacropBCheck, MCCalMagCheck, MCSweetCandyCheck, MOABCheck, MonsterBloomCheck, MPKCheck, PPBloomCheck, PPBoostCheck, PPCalKickCheck, PPFinisherCheck, PPGrowCheck, PPSpikeCheck };
        }

        private double GetFertilizerQuantity(string fertilizerName)
        {
            var quantityTextBoxes = new Dictionary<string, TextBox>
    {
        // Fertilizer Quantity TextBoxes
        {"BioBizz Algamic", BioBizzAlgamicBox},
        {"BioBizz Bloom", BioBizzBloomBox},
        {"BioBizz Grow", BioBizzGrowBox},
        {"Calcium Nitrate", CalciumNitrateBox},
        {"CaliMagic", CaliMagicBox},
        {"Canna Boost", CannaBoostBox},
        {"Canna Calmag", CannaCalmagBox},
        {"CannaCoco A", CannaCocoABox},
        {"CannaCoco B", CannaCocoBBox},
        {"Canna Flores A", CannaFloresABox},
        {"Canna Flores B", CannaFloresBBox},
        {"Canna PH Down", CannaPHDownBox},
        {"Canna PK13/14", CannaPK1314Box},
        {"Canna Vega A", CannaVegaABox},
        {"Canna Vega B", CannaVegaBBox},
        {"Canna Start", CannaStartBox},
        {"Chempak 3", Chempak3Box},
        {"Chempak 8", Chempak8Box},
        {"Chempak Seq.Iron", ChempakSeqIronBox},
        {"DryPart Bloom", DryPartBloomBox},
        {"Dyna-Bloom", DynaBloomBox},
        {"Dyna-Grow", DynaGrowBox},
        {"Dyna-MagPro", DynaMagProBox},
        {"Epsom Salt", EpsomSaltBox},
        {"Gypsum", GypsumBox},
        {"Jacks 0-12-26", Jacks01226Box},
        {"Jacks 5-12-26", Jacks51226Box},
        {"Jacks 5-50-18", Jacks55018Box},
        {"Jacks 7-15-30", Jacks71530Box},
        {"Jacks 10-30-20", Jacks103020Box},
        {"Jacks 12-4-16", Jacks12416Box},
        {"Jacks 15-0-0", Jacks1500Box},
        {"Jacks 15-5-20", Jacks15520Box},
        {"Jacks 15-6-17", Jacks15617Box},
        {"Jacks 18-8-23", Jacks18823Box},
        {"Jacks 20-10-20", Jacks201020Box},
        {"Jacks 20-20-20", Jacks202020Box},
        {"K-Trate LX", KTrateLXBox},
        {"Koolbloom", KoolbloomBox},
        {"L.Koolbloom", LKoolbloomBox},
        {"MagNit", MagNitBox},
        {"Mag-Trate LX", MagTrateLXBox},
        {"MAP", MAPBox},
        {"MaxiBloom", MaxiBloomBox},
        {"MaxiGrow", MaxiGrowBox},
        {"Megacrop", MegacropBox},
        {"Megacrop A", MegacropABox},
        {"Megacrop B", MegacropBBox},
        {"MC Cal-Mag", MCCalMagBox},
        {"MC Sweet Candy", MCSweetCandyBox},
        {"MOAB", MOABBox},
        {"Monster Bloom", MonsterBloomBox},
        {"MPK", MPKBox},
        {"PP Bloom", PPBloomBox},
        {"PP Boost", PPBoostBox},
        {"PP CalKick", PPCalKickBox},
        {"PP Finisher", PPFinisherBox},
        {"PP Grow", PPGrowBox},
        {"PP Spike", PPSpikeBox},
        {"Vitalink Calmag+", VitalinkCalmagBox}
        // Add any additional mappings here as necessary
    };

            if (quantityTextBoxes.TryGetValue(fertilizerName, out TextBox quantityBox))
            {
                if (double.TryParse(quantityBox.Text, out double quantity))
                {
                    return quantity;
                }
            }

            return 0.0; // Return 0 if the TextBox is not found or the value is not a valid double
        }

        private void CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            // Determine the corresponding TextBox based on the CheckBox name
            var textBoxName = checkBox.Name.Replace("Check", "Box"); // Adjust this line if your naming convention differs
            var textBox = this.FindName(textBoxName) as TextBox;

            if (textBox != null)
            {
                textBox.IsEnabled = checkBox.IsChecked ?? true; // Enable or disable based on CheckBox
                if (!(checkBox.IsChecked ?? false))
                {
                    textBox.Text = ""; // Clear the TextBox if CheckBox is unchecked
                }
            }
        }

        private void PredefinedMixesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PredefinedMixesComboBox.SelectedItem == null) return;

            var selectedItem = PredefinedMixesComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null || !selectedItem.IsEnabled) return; // Ignore if the selected item is the placeholder or null

            var selectedMix = selectedItem.Content.ToString();

            // Handle the Reset option
            if (selectedMix == "Reset")
            {
                DisableAllCheckBoxes(); // This should clear and disable all CheckBoxes
                DisableAllQuantityTextBoxes(); // Optionally clear all TextBoxes if needed
                SaveMixNameTextBox.Text = ""; // Clear the SaveMixNameTextBox
                // Reset nutrient value text boxes to empty
                ClearNutrientValues();
                // Reset the ComboBox selection to the placeholder
                PredefinedMixesComboBox.SelectedIndex = 0;
            }
            else if (savedMixes.ContainsKey(selectedMix))
            {
                DisableAllQuantityCheckBoxes(); // Ensure a clean state before applying new selections
                var mixDetails = savedMixes[selectedMix];
                // Apply mix details to CheckBoxes and TextBoxes
                foreach (var detail in mixDetails)
                {
                    var checkBoxName = SanitizeFertilizerName(detail.Key) + "Check";
                    var textBoxName = SanitizeFertilizerName(detail.Key) + "Box";
                    var checkBox = this.FindName(checkBoxName) as CheckBox;
                    var textBox = this.FindName(textBoxName) as TextBox;
                    if (checkBox != null && textBox != null)
                    {
                        checkBox.IsChecked = true;
                        textBox.Text = detail.Value.ToString();
                    }
                }

                SaveMixNameTextBox.Text = selectedMix; // Set the mix name in the SaveMixNameTextBox
            }
        }

        private void SaveMixButton_Click(object sender, RoutedEventArgs e)
        {
            var mixName = SaveMixNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(mixName))
            {
                MessageBox.Show("Please enter a name for the mix.", "Save Mix", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if the mix name already exists
            var existingItem = PredefinedMixesComboBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString().Equals(mixName, StringComparison.OrdinalIgnoreCase));

            if (existingItem != null)
            {
                // Prompt the user to confirm update
                var result = MessageBox.Show("This mix name already exists. Would you like to update it?", "Update Mix", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Update the existing mix details
                    UpdateExistingMix(mixName);
                }
                // If No, just return without doing anything
                return;
            }

            // Proceed with saving new mix if it does not exist
            SaveNewMix(mixName);
            RefreshMixesInComboBox();
            PopulateComparisonMixesComboBox(); // Update comparison mixes as well
        }

        private void SaveNewMix(string mixName)
        {
            var newItem = new ComboBoxItem { Content = mixName };
            PredefinedMixesComboBox.Items.Add(newItem);
            PredefinedMixesComboBox.SelectedItem = newItem;

            var currentMixDetails = GetCurrentMixDetails();
            savedMixes[mixName] = currentMixDetails;
            SaveMixesToFile(); // Make sure to save to file

            MessageBox.Show("Mix saved successfully!", "Save Mix", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshMixesInComboBox();
            SaveMixNameTextBox.Text = ""; // Optionally clear or keep the mix name
        }

        private void UpdateExistingMix(string mixName)
        {
            var currentMixDetails = GetCurrentMixDetails();
            savedMixes[mixName] = currentMixDetails; // Update the existing mix
            SaveMixesToFile(); // Make sure to save the updates to file
            RefreshMixesInComboBox();

            MessageBox.Show("Mix updated successfully!", "Update Mix", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // This method should contain the logic to gather current mix details
        private Dictionary<string, double> GetCurrentMixDetails()
        {
            var details = new Dictionary<string, double>();
            foreach (var checkBox in GetFertilizerCheckBoxes())
            {
                if (checkBox.IsChecked == true)
                {
                    string fertilizerName = checkBox.Content.ToString();
                    double quantity = GetFertilizerQuantity(fertilizerName);
                    details[fertilizerName] = quantity;
                }
            }
            return details;
        }

        private MixCollection ConvertToSerializableForm()
        {
            var mixCollection = new MixCollection();
            foreach (var mixEntry in savedMixes)
            {
                var mix = new Mix { Name = mixEntry.Key };
                foreach (var fertEntry in mixEntry.Value)
                {
                    mix.FertilizerQuantities.Add(new FertilizerQuantity { Name = fertEntry.Key, Quantity = fertEntry.Value });
                }
                mixCollection.Mixes.Add(mix);
            }
            return mixCollection;
        }

        private void SaveMixesToFile()
        {
            var mixCollection = ConvertToSerializableForm();
            var serializer = new XmlSerializer(typeof(MixCollection));
            using (var writer = new StreamWriter(mixesFilePath))
            {
                serializer.Serialize(writer, mixCollection);
            }
        }

        private void LoadMixesFromFile()
        {
            if (File.Exists(mixesFilePath))
            {
                var serializer = new XmlSerializer(typeof(MixCollection));
                using (var reader = new StreamReader(mixesFilePath))
                {
                    var mixCollection = (MixCollection)serializer.Deserialize(reader);
                    savedMixes = mixCollection.Mixes.ToDictionary(
                        mix => mix.Name,
                        mix => mix.FertilizerQuantities.ToDictionary(fq => fq.Name, fq => fq.Quantity)
                    );
                }
            }
        }

        private void DeleteMixButton_Click(object sender, RoutedEventArgs e)
        {
            if (PredefinedMixesComboBox.SelectedItem is ComboBoxItem selectedMixItem && selectedMixItem.Content.ToString() != "Reset")
            {
                var mixName = selectedMixItem.Content.ToString();

                // Confirm deletion with the user
                var result = MessageBox.Show($"Are you sure you want to delete the mix '{mixName}'?", "Delete Mix", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Remove from in-memory dictionary
                    savedMixes.Remove(mixName);

                    // Remove from ComboBox
                    PredefinedMixesComboBox.Items.Remove(PredefinedMixesComboBox.SelectedItem);

                    // Save the updated mix collection to file
                    SaveMixesToFile();

                    MessageBox.Show("Mix deleted successfully!", "Delete Mix", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reset UI elements or select another mix
                    DisableAllCheckBoxes();
                    DisableAllQuantityTextBoxes();
                }
            }
            else
            {
                MessageBox.Show("Please select a mix to delete.", "Delete Mix", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ComparisonMixesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComparisonMixesComboBox.SelectedItem == null) return;

            var selectedItem = ComparisonMixesComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null || !selectedItem.IsEnabled) return; // Ignore if the selected item is the placeholder or null

            var selectedMix = selectedItem.Content.ToString();
            // Check if "Reset" is selected
            if (selectedMix.Equals("Reset"))
            {
                // Reset comparison nutrient value text boxes to empty
                ClearComparisonNutrientValues();

                // Optionally, reset the ComboBox selection to the placeholder
                ComparisonMixesComboBox.SelectedIndex = 0;
            }
            else if (selectedMix != "Select a Mix to Compare")
            {
                ApplyComparisonMix(selectedMix);
            }
        }

        private void ApplyComparisonMix(string mixName)
        {
            // Determine the conversion factor based on the current unit preference
            double conversionFactor = Properties.Settings.Default.UnitPreference ? 3.78541 : 1;

            // Assuming savedMixes dictionary is already populated and has the mix details
            if (!savedMixes.ContainsKey(mixName)) return;

                var mixDetails = savedMixes[mixName];

                // Initialize nutrient totals
                double totalN = 0, totalP = 0, totalK = 0, totalMg = 0, totalCa = 0, totalS = 0;
                double totalFe = 0, totalZn = 0, totalB = 0, totalMn = 0, totalCu = 0, totalMo = 0, totalPPM = 0;

                // Calculate total nutrients based on the mix details
                foreach (var detail in mixDetails)
                {
                    if (fertilizers.TryGetValue(detail.Key, out Fertilizer fertilizer))
                    {
                        // Calculate the contribution of each fertilizer to the total nutrient values
                        double quantity = detail.Value; // This is the quantity of the fertilizer in the mix
                        totalN += fertilizer.N * quantity;
                        totalP += fertilizer.P * quantity;
                        totalK += fertilizer.K * quantity;
                        totalMg += fertilizer.Mg * quantity;
                        totalCa += fertilizer.Ca * quantity;
                        totalS += fertilizer.S * quantity;
                        totalFe += fertilizer.Fe * quantity;
                        totalZn += fertilizer.Zn * quantity;
                        totalB += fertilizer.B * quantity;
                        totalMn += fertilizer.Mn * quantity;
                        totalCu += fertilizer.Cu * quantity;
                        totalMo += fertilizer.Mo * quantity;
                        totalPPM += fertilizer.TotalPPM * quantity;
                    }
                }

            // Update the comparison textboxes
            CompareTotalNitrogenBox.Text = (totalN * conversionFactor).ToString("N2");
            ComparePhosphorousBox.Text = (totalP * conversionFactor).ToString("N2");
            ComparePotassiumBox.Text = (totalK * conversionFactor).ToString("N2");
            CompareMagnesiumBox.Text = (totalMg * conversionFactor).ToString("N2");
            CompareCalciumBox.Text = (totalCa * conversionFactor).ToString("N2");
            CompareSulfurBox.Text = (totalS * conversionFactor).ToString("N2");
            CompareIronBox.Text = (totalFe * conversionFactor).ToString("N2");
            CompareZincBox.Text = (totalZn * conversionFactor).ToString("N2");
            CompareBoronBox.Text = (totalB * conversionFactor).ToString("N2");
            CompareManganeseBox.Text = (totalMn * conversionFactor).ToString("N2");
            CompareCopperBox.Text = (totalCu * conversionFactor).ToString("N2");
            CompareMolybdenumBox.Text = (totalMo * conversionFactor).ToString("N2");
            CompareTotalPPMBox.Text = (totalPPM * conversionFactor).ToString("N2");

            UpdateComparisonBoxBackground(CompareTotalNitrogenBox, TotalNitrogenBox);
            UpdateComparisonBoxBackground(ComparePhosphorousBox, PhosphorousBox);
            UpdateComparisonBoxBackground(ComparePotassiumBox, PotassiumBox);
            UpdateComparisonBoxBackground(CompareMagnesiumBox, MagnesiumBox);
            UpdateComparisonBoxBackground(CompareCalciumBox, CalciumBox);
            UpdateComparisonBoxBackground(CompareSulfurBox, SulfurBox);
            UpdateComparisonBoxBackground(CompareIronBox, IronBox);
            UpdateComparisonBoxBackground(CompareZincBox, ZincBox);
            UpdateComparisonBoxBackground(CompareBoronBox, BoronBox);
            UpdateComparisonBoxBackground(CompareManganeseBox, ManganeseBox);
            UpdateComparisonBoxBackground(CompareCopperBox, CopperBox);
            UpdateComparisonBoxBackground(CompareMolybdenumBox, MolybdenumBox);
            UpdateComparisonBoxBackground(CompareTotalPPMBox, TotalPPMBox);

        }

        private void UpdateComparisonBoxBackground(TextBox compareBox, TextBox regularBox)
        {
            double compareValue, regularValue;
            bool compareSuccess = double.TryParse(compareBox.Text, out compareValue);
            bool regularSuccess = double.TryParse(regularBox.Text, out regularValue);

            // Determine the tolerance based on the nutrient type
            double tolerance = DetermineTolerance(compareBox.Name);

            if (compareSuccess && regularSuccess)
            {
                if (compareValue == regularValue)
                {
                    // If values are exactly the same, set background to white
                    compareBox.Background = Brushes.White;
                }
                else if (Math.Abs(compareValue - regularValue) <= tolerance)
                {
                    // If values are within tolerance, set background to light blue
                    compareBox.Background = Brushes.LightBlue;
                }
                else if (compareValue > regularValue)
                {
                    // If comparison value is higher, set background to green
                    compareBox.Background = Brushes.Green;
                }
                else
                {
                    // If comparison value is lower, set background to red
                    compareBox.Background = Brushes.Red;
                }
            }
            else
            {
                // Use white color if comparison isn't possible due to parsing failure
                compareBox.Background = Brushes.White;
            }
        }

        // Helper method to determine tolerance based on the nutrient type
        private double DetermineTolerance(string textBoxName)
        {
            // List of TextBox names for primary nutrients with higher tolerance
            var primaryNutrients = new List<string>
    {
        "TotalNitrogenBox", "PhosphorousBox", "PotassiumBox",
        "MagnesiumBox", "CalciumBox", "SulfurBox", "TotalPPMBox"
    };

            // Check if the textBoxName contains any of the primary nutrient identifiers
            // Adjust the logic if your TextBox naming convention is different
            if (primaryNutrients.Any(n => textBoxName.Contains(n)))
            {
                return 1.0; // Higher tolerance for primary nutrients
            }
            else
            {
                return 0.1; // Lower tolerance for micronutrients
            }
        }

        private void ClearNutrientValues()
        {
            TotalNitrogenBox.Text = "";
            PhosphorousBox.Text = "";
            PotassiumBox.Text = "";
            MagnesiumBox.Text = "";
            CalciumBox.Text = "";
            SulfurBox.Text = "";
            IronBox.Text = "";
            ZincBox.Text = "";
            BoronBox.Text = "";
            ManganeseBox.Text = "";
            CopperBox.Text = "";
            MolybdenumBox.Text = "";
            TotalPPMBox.Text = "";
        }

        private void ClearComparisonNutrientValues()
        {
            var defaultBrush = new SolidColorBrush(Colors.White); // Assuming white is the default

            // List of all comparison boxes
            var comparisonBoxes = new TextBox[]
            {
        CompareTotalNitrogenBox, ComparePhosphorousBox, ComparePotassiumBox,
        CompareMagnesiumBox, CompareCalciumBox, CompareSulfurBox,
        CompareIronBox, CompareZincBox, CompareBoronBox,
        CompareManganeseBox, CompareCopperBox, CompareMolybdenumBox,
        CompareTotalPPMBox
            };

            // Clear the text and reset the background color for each comparison box
            foreach (var box in comparisonBoxes)
            {
                box.Text = "";
                box.Background = defaultBrush;
            }
        }

        private void ImportMixesButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml", // Filter to only show XML files
                Title = "Select UserMixes.xml to Import"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImportMixes(openFileDialog.FileName);
            }
        }

        private void ImportMixes(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(MixCollection));
                MixCollection importedMixCollection;
                using (var reader = new StreamReader(filePath))
                {
                    importedMixCollection = (MixCollection)serializer.Deserialize(reader);
                }

                bool mixesUpdated = false;
                foreach (var importedMix in importedMixCollection.Mixes)
                {
                    if (!savedMixes.ContainsKey(importedMix.Name))
                    {
                        savedMixes.Add(importedMix.Name, importedMix.FertilizerQuantities.ToDictionary(fq => fq.Name, fq => fq.Quantity));
                        mixesUpdated = true;
                    }
                }

                if (mixesUpdated)
                {
                    SaveMixesToFile(); // Update the current UserMixes.xml file
                    PopulateMixesComboBox(); // Refresh the UI
                    PopulateComparisonMixesComboBox(); // Refresh the comparison ComboBox as well
                    MessageBox.Show("New mixes have been imported successfully.", "Import Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No new mixes were found to import.", "No New Mixes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to import mixes: {ex.Message}", "Import Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnitToggle_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UnitPreference = true;
            Properties.Settings.Default.Save(); // Save the settings
            UpdateUnitToggleButtonLabel();
            UpdateNutrientValues(null, null);
        }

        private void UnitToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UnitPreference = false;
            Properties.Settings.Default.Save(); // Save the settings
            UpdateUnitToggleButtonLabel();
            UpdateNutrientValues(null, null);
        }

        private void UpdateUnitToggleButtonLabel()
        {
            // Update the unit toggle button label
            UnitToggle.Content = Properties.Settings.Default.UnitPreference ? "g|mL/Liter" : "g|mL/Gallon";

            // Update unitbox1 and unitbox2 based on the current unit preference
            string unitText = Properties.Settings.Default.UnitPreference ? "g|mL/L" : "g|mL/Gal";
            unitbox1.Text = unitText;
            unitbox2.Text = unitText;
        }

    }
}