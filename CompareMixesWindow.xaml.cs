using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FertCalculator.Core;

namespace FertilizerCalculator
{
    public partial class CompareMixesWindow : Window
    {
        private List<FertilizerMix> availableMixes;
        private List<Fertilizer> availableFertilizers;
        private bool isMetricUnits; // true = per liter, false = per gallon
        
        public CompareMixesWindow(List<FertilizerMix> mixes, List<Fertilizer> fertilizers, bool startMetric = true)
        {
            InitializeComponent();
            
            availableMixes = mixes;
            availableFertilizers = fertilizers;
            isMetricUnits = startMetric;
            
            // Populate the dropdown lists
            FirstMixComboBox.ItemsSource = availableMixes;
            SecondMixComboBox.ItemsSource = availableMixes;
            
            // Set default selections if possible
            if (availableMixes.Count > 0)
            {
                FirstMixComboBox.SelectedIndex = 0;
                
                if (availableMixes.Count > 1)
                {
                    SecondMixComboBox.SelectedIndex = 1;
                }
                else
                {
                    SecondMixComboBox.SelectedIndex = 0;
                }
            }
            
            // Initialize unit display
            UpdateUnitDisplay();
        }
        
        private void MixComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateComparison();
        }
        
        private void UnitToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between metric and imperial units
            isMetricUnits = !isMetricUnits;
            
            // Update the button text and recalculate
            UpdateUnitDisplay();
            UpdateComparison();
        }
        
        private void UpdateUnitDisplay()
        {
            string quantityUnit = isMetricUnits ? "g or mL/L" : "g or mL/gal";
            string ppmUnit = isMetricUnits ? "PPM/L" : "PPM/gal";

            UnitToggleButton.Content = isMetricUnits ? "PPM per Liter" : "PPM per Gallon";
            Mix1HeaderTextBlock.Text = $"Mix 1 ({ppmUnit})";
            Mix2HeaderTextBlock.Text = $"Mix 2 ({ppmUnit})";
            FirstMixQuantityColumn.Header = $"Quantity ({quantityUnit})";
            SecondMixQuantityColumn.Header = $"Quantity ({quantityUnit})";
        }
        
        private void CopyComparison_Click(object sender, RoutedEventArgs e)
        {
            var rows = new (string Name, TextBox Box1, TextBox Box2)[]
            {
                ("Nitrogen (N)", FirstMixNitrogenTextBlock, SecondMixNitrogenTextBlock),
                ("Phosphorus (P)", FirstMixPhosphorusTextBlock, SecondMixPhosphorusTextBlock),
                ("Potassium (K)", FirstMixPotassiumTextBlock, SecondMixPotassiumTextBlock),
                ("Calcium (Ca)", FirstMixCalciumTextBlock, SecondMixCalciumTextBlock),
                ("Magnesium (Mg)", FirstMixMagnesiumTextBlock, SecondMixMagnesiumTextBlock),
                ("Sulfur (S)", FirstMixSulfurTextBlock, SecondMixSulfurTextBlock),
                ("Boron (B)", FirstMixBoronTextBlock, SecondMixBoronTextBlock),
                ("Copper (Cu)", FirstMixCopperTextBlock, SecondMixCopperTextBlock),
                ("Iron (Fe)", FirstMixIronTextBlock, SecondMixIronTextBlock),
                ("Manganese (Mn)", FirstMixManganeseTextBlock, SecondMixManganeseTextBlock),
                ("Molybdenum (Mo)", FirstMixMolybdenumTextBlock, SecondMixMolybdenumTextBlock),
                ("Zinc (Zn)", FirstMixZincTextBlock, SecondMixZincTextBlock),
                ("Chlorine (Cl)", FirstMixChlorineTextBlock, SecondMixChlorineTextBlock),
                ("Silica (Si)", FirstMixSilicaTextBlock, SecondMixSilicaTextBlock),
                ("Humic Acid", FirstMixHumicAcidTextBlock, SecondMixHumicAcidTextBlock),
                ("Fulvic Acid", FirstMixFulvicAcidTextBlock, SecondMixFulvicAcidTextBlock),
                ("Kelp", FirstMixKelpTextBlock, SecondMixKelpTextBlock),
                ("Total PPM", FirstMixTotalPpmTextBlock, SecondMixTotalPpmTextBlock),
            };

            string units = isMetricUnits ? "PPM per liter" : "PPM per gallon";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Nutrient\t{FirstMixNameTextBlock.Text} ({units})\t{SecondMixNameTextBlock.Text} ({units})");
            foreach (var (name, box1, box2) in rows)
                sb.AppendLine($"{name}\t{box1.Text}\t{box2.Text}");

            try
            {
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not access the clipboard: {ex.Message}", "Copy Failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateComparison()
        {
            var firstMix = FirstMixComboBox.SelectedItem as FertilizerMix;
            var secondMix = SecondMixComboBox.SelectedItem as FertilizerMix;
            
            if (firstMix == null || secondMix == null) return;
            
            // Update mix names
            FirstMixNameTextBlock.Text = firstMix.Name;
            SecondMixNameTextBlock.Text = secondMix.Name;
            
            // Update ingredients lists
            FirstMixIngredientsGrid.ItemsSource = firstMix.Ingredients;
            SecondMixIngredientsGrid.ItemsSource = secondMix.Ingredients;
            
            // Calculate nutrient totals for both mixes
            var firstMixNutrients = MixCalculator.Calculate(firstMix.Ingredients, availableFertilizers, !isMetricUnits);
            var secondMixNutrients = MixCalculator.Calculate(secondMix.Ingredients, availableFertilizers, !isMetricUnits);

            // Update UI with calculated values
            UpdateNutrientDisplay(firstMixNutrients, secondMixNutrients);
        }

        private void UpdateNutrientDisplay(NutrientTotals first, NutrientTotals second)
        {
            UpdateUnitDisplay();

            FirstMixNitrogenTextBlock.Text = first.Nitrogen.ToString("F2");
            FirstMixPhosphorusTextBlock.Text = first.Phosphorus.ToString("F2");
            FirstMixPotassiumTextBlock.Text = first.Potassium.ToString("F2");
            FirstMixCalciumTextBlock.Text = first.Calcium.ToString("F2");
            FirstMixMagnesiumTextBlock.Text = first.Magnesium.ToString("F2");
            FirstMixSulfurTextBlock.Text = first.Sulfur.ToString("F2");
            FirstMixBoronTextBlock.Text = first.Boron.ToString("F2");
            FirstMixCopperTextBlock.Text = first.Copper.ToString("F2");
            FirstMixIronTextBlock.Text = first.Iron.ToString("F2");
            FirstMixManganeseTextBlock.Text = first.Manganese.ToString("F2");
            FirstMixMolybdenumTextBlock.Text = first.Molybdenum.ToString("F2");
            FirstMixZincTextBlock.Text = first.Zinc.ToString("F2");
            FirstMixChlorineTextBlock.Text = first.Chlorine.ToString("F2");
            FirstMixSilicaTextBlock.Text = first.Silica.ToString("F2");
            FirstMixHumicAcidTextBlock.Text = first.HumicAcid.ToString("F2");
            FirstMixFulvicAcidTextBlock.Text = first.FulvicAcid.ToString("F2");
            FirstMixKelpTextBlock.Text = first.Kelp.ToString("F2");
            FirstMixTotalPpmTextBlock.Text = first.Total.ToString("F2");

            SecondMixNitrogenTextBlock.Text = second.Nitrogen.ToString("F2");
            SecondMixPhosphorusTextBlock.Text = second.Phosphorus.ToString("F2");
            SecondMixPotassiumTextBlock.Text = second.Potassium.ToString("F2");
            SecondMixCalciumTextBlock.Text = second.Calcium.ToString("F2");
            SecondMixMagnesiumTextBlock.Text = second.Magnesium.ToString("F2");
            SecondMixSulfurTextBlock.Text = second.Sulfur.ToString("F2");
            SecondMixBoronTextBlock.Text = second.Boron.ToString("F2");
            SecondMixCopperTextBlock.Text = second.Copper.ToString("F2");
            SecondMixIronTextBlock.Text = second.Iron.ToString("F2");
            SecondMixManganeseTextBlock.Text = second.Manganese.ToString("F2");
            SecondMixMolybdenumTextBlock.Text = second.Molybdenum.ToString("F2");
            SecondMixZincTextBlock.Text = second.Zinc.ToString("F2");
            SecondMixChlorineTextBlock.Text = second.Chlorine.ToString("F2");
            SecondMixSilicaTextBlock.Text = second.Silica.ToString("F2");
            SecondMixHumicAcidTextBlock.Text = second.HumicAcid.ToString("F2");
            SecondMixFulvicAcidTextBlock.Text = second.FulvicAcid.ToString("F2");
            SecondMixKelpTextBlock.Text = second.Kelp.ToString("F2");
            SecondMixTotalPpmTextBlock.Text = second.Total.ToString("F2");
        }
    }
}
