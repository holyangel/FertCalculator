using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FertCalculator.Core;

namespace FertilizerCalculator
{
    public partial class AddFertilizerWindow : Window
    {
        public Fertilizer NewFertilizer { get; private set; }
        private bool isEditMode;

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

                CalciumTextBox.Text = existingFertilizer.CalciumPercent.ToString("F3");
                MagnesiumTextBox.Text = existingFertilizer.MagnesiumPercent.ToString("F3");
                SulfurTextBox.Text = existingFertilizer.SulfurPercent.ToString("F3");
                BoronTextBox.Text = existingFertilizer.BoronPercent.ToString("F3");
                CopperTextBox.Text = existingFertilizer.CopperPercent.ToString("F3");
                IronTextBox.Text = existingFertilizer.IronPercent.ToString("F3");
                ManganeseTextBox.Text = existingFertilizer.ManganesePercent.ToString("F3");
                MolybdenumTextBox.Text = existingFertilizer.MolybdenumPercent.ToString("F3");
                ZincTextBox.Text = existingFertilizer.ZincPercent.ToString("F3");
                ChlorineTextBox.Text = existingFertilizer.ChlorinePercent.ToString("F3");
                SilicaTextBox.Text = existingFertilizer.SilicaPercent.ToString("F3");
                HumicAcidTextBox.Text = existingFertilizer.HumicAcidPercent.ToString("F3");
                FulvicAcidTextBox.Text = existingFertilizer.FulvicAcidPercent.ToString("F3");
                KelpTextBox.Text = existingFertilizer.KelpPercent.ToString("F3");
                IsLiquidCheckBox.IsChecked = existingFertilizer.IsLiquid;
                DensityTextBox.Text = existingFertilizer.Density.ToString("F3");
            }
            else
            {
                // Set default values for new fertilizer
                NitrogenTextBox.Text = "";
                PhosphorusTextBox.Text = "";
                PotassiumTextBox.Text = "";
                CalciumTextBox.Text = "";
                MagnesiumTextBox.Text = "";
                SulfurTextBox.Text = "";
                BoronTextBox.Text = "";
                CopperTextBox.Text = "";
                IronTextBox.Text = "";
                ManganeseTextBox.Text = "";
                MolybdenumTextBox.Text = "";
                ZincTextBox.Text = "";
                ChlorineTextBox.Text = "";
                SilicaTextBox.Text = "";
                HumicAcidTextBox.Text = "";
                FulvicAcidTextBox.Text = "";
                KelpTextBox.Text = "";
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
                phosphorusValue = Fertilizer.P2O5ToP(phosphorusValue);
            }

            // Get potassium value with conversion if needed
            double potassiumValue = ParseTextBoxValue(PotassiumTextBox);
            bool isPotassiumInOxideForm = PotassiumFormatComboBox.SelectedIndex == 1;
            double originalPotassiumValue = potassiumValue;

            if (isPotassiumInOxideForm)
            {
                // Convert from K₂O to K for storage
                potassiumValue = Fertilizer.K2OToK(potassiumValue);
            }

            double density = ParseTextBoxValue(DensityTextBox);
            if (density <= 0) density = 1.0;

            // Create new fertilizer object
            NewFertilizer = new Fertilizer
            {
                Name = NameTextBox.Text.Trim(),
                NitrogenPercent = ParseTextBoxValue(NitrogenTextBox),
                PhosphorusPercent = phosphorusValue,
                PotassiumPercent = potassiumValue,
                CalciumPercent = ParseTextBoxValue(CalciumTextBox),
                MagnesiumPercent = ParseTextBoxValue(MagnesiumTextBox),
                SulfurPercent = ParseTextBoxValue(SulfurTextBox),
                BoronPercent = ParseTextBoxValue(BoronTextBox),
                CopperPercent = ParseTextBoxValue(CopperTextBox),
                IronPercent = ParseTextBoxValue(IronTextBox),
                ManganesePercent = ParseTextBoxValue(ManganeseTextBox),
                MolybdenumPercent = ParseTextBoxValue(MolybdenumTextBox),
                ZincPercent = ParseTextBoxValue(ZincTextBox),
                ChlorinePercent = ParseTextBoxValue(ChlorineTextBox),
                SilicaPercent = ParseTextBoxValue(SilicaTextBox),
                HumicAcidPercent = ParseTextBoxValue(HumicAcidTextBox),
                FulvicAcidPercent = ParseTextBoxValue(FulvicAcidTextBox),
                KelpPercent = ParseTextBoxValue(KelpTextBox),

                // Store original input format and values
                IsPhosphorusInOxideForm = isPhosphorusInOxideForm,
                IsPotassiumInOxideForm = isPotassiumInOxideForm,
                OriginalPhosphorusValue = originalPhosphorusValue,
                OriginalPotassiumValue = originalPotassiumValue,

                // Liquid handling: quantities for liquid products are mL,
                // converted to grams via density
                IsLiquid = IsLiquidCheckBox.IsChecked == true,
                Density = density
            };

            DialogResult = true;
            Close();
        }

        private double ParseTextBoxValue(TextBox textBox)
        {
            if (double.TryParse(textBox.Text, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.CurrentCulture, out double value))
            {
                return value;
            }
            return 0.0;
        }

        // Returns true if the proposed text is a valid partial/complete non-negative
        // decimal number using the current culture's decimal separator.
        private static bool IsValidDecimalText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            string separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (text == separator)
                return true;

            return double.TryParse(text, System.Globalization.NumberStyles.AllowDecimalPoint,
                System.Globalization.CultureInfo.CurrentCulture, out _);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DecimalInputValidation(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) { e.Handled = true; return; }

            // Account for any selected text being replaced
            string proposedText = textBox.Text
                .Remove(textBox.SelectionStart, textBox.SelectionLength)
                .Insert(textBox.SelectionStart, e.Text);

            e.Handled = !IsValidDecimalText(proposedText);
        }
    }
}