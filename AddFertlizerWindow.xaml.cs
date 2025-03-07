using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FertilizerCalculator
{
    public partial class AddFertilizerWindow : Window
    {
        public Fertilizer NewFertilizer { get; private set; }
        private bool isEditMode;

        // Conversion factors
        private const double P2O5_TO_P = 0.4364;  // Multiply P₂O₅ by this to get P
        private const double K2O_TO_K = 0.8301;   // Multiply K₂O by this to get K

        public AddFertilizerWindow(Fertilizer existingFertilizer = null)
        {
            InitializeComponent();

            isEditMode = existingFertilizer != null;
            Title = isEditMode ? "Edit Fertilizer" : "Add New Fertilizer";

            // Set up the P and K input mode options
            SetupConversionOptions();

            if (isEditMode)
            {
                // Fill in existing values
                NameTextBox.Text = existingFertilizer.Name;
                NitrogenTextBox.Text = existingFertilizer.NitrogenPercent.ToString("F3");

                // Use original P values if available, otherwise use converted values
                if (existingFertilizer.IsPhosphorusInOxideForm)
                {
                    PhosphorusTextBox.Text = existingFertilizer.OriginalPhosphorusValue.ToString("F3");
                    PhosphorusFormatComboBox.SelectedIndex = 1; // P₂O₅
                }
                else
                {
                    PhosphorusTextBox.Text = existingFertilizer.PhosphorusPercent.ToString("F3");
                    PhosphorusFormatComboBox.SelectedIndex = 0; // P
                }

                // Use original K values if available, otherwise use converted values
                if (existingFertilizer.IsPotassiumInOxideForm)
                {
                    PotassiumTextBox.Text = existingFertilizer.OriginalPotassiumValue.ToString("F3");
                    PotassiumFormatComboBox.SelectedIndex = 1; // K₂O
                }
                else
                {
                    PotassiumTextBox.Text = existingFertilizer.PotassiumPercent.ToString("F3");
                    PotassiumFormatComboBox.SelectedIndex = 0; // K
                }

                MagnesiumTextBox.Text = existingFertilizer.MagnesiumPercent.ToString("F3");
                CalciumTextBox.Text = existingFertilizer.CalciumPercent.ToString("F3");
                SulfurTextBox.Text = existingFertilizer.SulfurPercent.ToString("F3");
                IronTextBox.Text = existingFertilizer.IronPercent.ToString("F3");
                ZincTextBox.Text = existingFertilizer.ZincPercent.ToString("F3");
                BoronTextBox.Text = existingFertilizer.BoronPercent.ToString("F3");
                ManganeseTextBox.Text = existingFertilizer.ManganesePercent.ToString("F3");
                CopperTextBox.Text = existingFertilizer.CopperPercent.ToString("F3");
                MolybdenumTextBox.Text = existingFertilizer.MolybdenumPercent.ToString("F3");
            }
            else
            {
                // Set default values for new fertilizer
                NitrogenTextBox.Text = "0.000";
                PhosphorusTextBox.Text = "0.000";
                PotassiumTextBox.Text = "0.000";
                MagnesiumTextBox.Text = "0.000";
                CalciumTextBox.Text = "0.000";
                SulfurTextBox.Text = "0.000";
                IronTextBox.Text = "0.000";
                ZincTextBox.Text = "0.000";
                BoronTextBox.Text = "0.000";
                ManganeseTextBox.Text = "0.000";
                CopperTextBox.Text = "0.000";
                MolybdenumTextBox.Text = "0.000";
            }
        }

        private void SetupConversionOptions()
        {
            // Add these UI elements to your XAML or create them programmatically
            if (PhosphorusFormatComboBox != null)
            {
                PhosphorusFormatComboBox.Items.Clear();
                PhosphorusFormatComboBox.Items.Add("P (elemental)");
                PhosphorusFormatComboBox.Items.Add("P₂O₅ (oxide)");
                PhosphorusFormatComboBox.SelectedIndex = 1; // Default to P₂O₅ since it's more common
            }

            if (PotassiumFormatComboBox != null)
            {
                PotassiumFormatComboBox.Items.Clear();
                PotassiumFormatComboBox.Items.Add("K (elemental)");
                PotassiumFormatComboBox.Items.Add("K₂O (oxide)");
                PotassiumFormatComboBox.SelectedIndex = 1; // Default to K₂O since it's more common
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the fertilizer.", "Missing Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            // Get phosphorus value with conversion if needed
            double phosphorusValue = ParseTextBoxValue(PhosphorusTextBox);
            bool isPhosphorusInOxideForm = PhosphorusFormatComboBox.SelectedIndex == 1;
            double originalPhosphorusValue = phosphorusValue;

            if (isPhosphorusInOxideForm)
            {
                // Convert from P₂O₅ to P for storage
                phosphorusValue *= P2O5_TO_P;
            }

            // Get potassium value with conversion if needed
            double potassiumValue = ParseTextBoxValue(PotassiumTextBox);
            bool isPotassiumInOxideForm = PotassiumFormatComboBox.SelectedIndex == 1;
            double originalPotassiumValue = potassiumValue;

            if (isPotassiumInOxideForm)
            {
                // Convert from K₂O to K for storage
                potassiumValue *= K2O_TO_K;
            }

            // Create new fertilizer object
            NewFertilizer = new Fertilizer
            {
                Name = NameTextBox.Text.Trim(),
                NitrogenPercent = ParseTextBoxValue(NitrogenTextBox),
                PhosphorusPercent = phosphorusValue,
                PotassiumPercent = potassiumValue,
                MagnesiumPercent = ParseTextBoxValue(MagnesiumTextBox),
                CalciumPercent = ParseTextBoxValue(CalciumTextBox),
                SulfurPercent = ParseTextBoxValue(SulfurTextBox),
                IronPercent = ParseTextBoxValue(IronTextBox),
                ZincPercent = ParseTextBoxValue(ZincTextBox),
                BoronPercent = ParseTextBoxValue(BoronTextBox),
                ManganesePercent = ParseTextBoxValue(ManganeseTextBox),
                CopperPercent = ParseTextBoxValue(CopperTextBox),
                MolybdenumPercent = ParseTextBoxValue(MolybdenumTextBox),

                // Store original input format and values
                IsPhosphorusInOxideForm = isPhosphorusInOxideForm,
                IsPotassiumInOxideForm = isPotassiumInOxideForm,
                OriginalPhosphorusValue = originalPhosphorusValue,
                OriginalPotassiumValue = originalPotassiumValue
            };

            DialogResult = true;
            Close();
        }

        private double ParseTextBoxValue(TextBox textBox)
        {
            if (double.TryParse(textBox.Text, out double value))
            {
                return value;
            }
            return 0.0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DecimalInputValidation(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits and decimal point
            Regex regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");

            // Get the current text and the text that would result after the new input
            TextBox textBox = sender as TextBox;
            string proposedText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Check if the proposed text is a valid decimal number
            e.Handled = !regex.IsMatch(proposedText);

            // Additional check to prevent multiple decimal points
            if (e.Text == "." && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
        }
    }
}