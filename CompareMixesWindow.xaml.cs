using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FertilizerCalculator
{
    public partial class CompareMixesWindow : Window
    {
        private List<FertilizerMix> availableMixes;
        private List<Fertilizer> availableFertilizers;
        
        public CompareMixesWindow(List<FertilizerMix> mixes, List<Fertilizer> fertilizers)
        {
            InitializeComponent();
            
            availableMixes = mixes;
            availableFertilizers = fertilizers;
            
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
        }
        
        private void MixComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateComparison();
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
            
            // Calculate nutrient totals for first mix
            var firstMixNutrients = CalculateMixNutrients(firstMix);
            
            // Calculate nutrient totals for second mix
            var secondMixNutrients = CalculateMixNutrients(secondMix);
            
            // Update UI with calculated values
            UpdateNutrientDisplay(firstMixNutrients, secondMixNutrients);
        }
        
        private Dictionary<string, double> CalculateMixNutrients(FertilizerMix mix)
        {
            var nutrients = new Dictionary<string, double>
            {
                { "Nitrogen", 0 },
                { "Phosphorus", 0 },
                { "Potassium", 0 },
                { "Calcium", 0 },
                { "Magnesium", 0 },
                { "Sulfur", 0 },
                { "Boron", 0 },
                { "Copper", 0 },
                { "Iron", 0 },
                { "Manganese", 0 },
                { "Molybdenum", 0 },
                { "Zinc", 0 },
                { "Chlorine", 0 },
                { "Silica", 0 },
                { "HumicAcid", 0 },
                { "FulvicAcid", 0 },
                { "Total", 0 }
            };
            
            foreach (var ingredient in mix.Ingredients)
            {
                var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == ingredient.FertilizerName);
                if (fertilizer != null)
                {
                    double quantity = ingredient.Quantity;
                    
                    // Add the contribution of each nutrient
                    nutrients["Nitrogen"] += (fertilizer.NitrogenPercent / 100) * quantity;
                    nutrients["Phosphorus"] += (fertilizer.PhosphorusPercent / 100) * quantity;
                    nutrients["Potassium"] += (fertilizer.PotassiumPercent / 100) * quantity;
                    nutrients["Calcium"] += (fertilizer.CalciumPercent / 100) * quantity;
                    nutrients["Magnesium"] += (fertilizer.MagnesiumPercent / 100) * quantity;
                    nutrients["Sulfur"] += (fertilizer.SulfurPercent / 100) * quantity;
                    nutrients["Boron"] += (fertilizer.BoronPercent / 100) * quantity;
                    nutrients["Copper"] += (fertilizer.CopperPercent / 100) * quantity;
                    nutrients["Iron"] += (fertilizer.IronPercent / 100) * quantity;
                    nutrients["Manganese"] += (fertilizer.ManganesePercent / 100) * quantity;
                    nutrients["Molybdenum"] += (fertilizer.MolybdenumPercent / 100) * quantity;
                    nutrients["Zinc"] += (fertilizer.ZincPercent / 100) * quantity;
                    nutrients["Chlorine"] += (fertilizer.ChlorinePercent / 100) * quantity;
                    nutrients["Silica"] += (fertilizer.SilicaPercent / 100) * quantity;
                    nutrients["HumicAcid"] += (fertilizer.HumicAcidPercent / 100) * quantity;
                    nutrients["FulvicAcid"] += (fertilizer.FulvicAcidPercent / 100) * quantity;
                }
            }
            
            // Calculate total PPM
            nutrients["Total"] = nutrients.Where(n => n.Key != "Total").Sum(n => n.Value);
            
            return nutrients;
        }
        
        private void UpdateNutrientDisplay(Dictionary<string, double> firstMixNutrients, Dictionary<string, double> secondMixNutrients)
        {
            // Convert g/L to PPM (multiply by 1000)
            FirstMixNitrogenTextBlock.Text = (firstMixNutrients["Nitrogen"] * 1000).ToString("F2");
            FirstMixPhosphorusTextBlock.Text = (firstMixNutrients["Phosphorus"] * 1000).ToString("F2");
            FirstMixPotassiumTextBlock.Text = (firstMixNutrients["Potassium"] * 1000).ToString("F2");
            FirstMixCalciumTextBlock.Text = (firstMixNutrients["Calcium"] * 1000).ToString("F2");
            FirstMixMagnesiumTextBlock.Text = (firstMixNutrients["Magnesium"] * 1000).ToString("F2");
            FirstMixSulfurTextBlock.Text = (firstMixNutrients["Sulfur"] * 1000).ToString("F2");
            FirstMixBoronTextBlock.Text = (firstMixNutrients["Boron"] * 1000).ToString("F2");
            FirstMixCopperTextBlock.Text = (firstMixNutrients["Copper"] * 1000).ToString("F2");
            FirstMixIronTextBlock.Text = (firstMixNutrients["Iron"] * 1000).ToString("F2");
            FirstMixManganeseTextBlock.Text = (firstMixNutrients["Manganese"] * 1000).ToString("F2");
            FirstMixMolybdenumTextBlock.Text = (firstMixNutrients["Molybdenum"] * 1000).ToString("F2");
            FirstMixZincTextBlock.Text = (firstMixNutrients["Zinc"] * 1000).ToString("F2");
            FirstMixChlorineTextBlock.Text = (firstMixNutrients["Chlorine"] * 1000).ToString("F2");
            FirstMixSilicaTextBlock.Text = (firstMixNutrients["Silica"] * 1000).ToString("F2");
            FirstMixHumicAcidTextBlock.Text = (firstMixNutrients["HumicAcid"] * 1000).ToString("F2");
            FirstMixFulvicAcidTextBlock.Text = (firstMixNutrients["FulvicAcid"] * 1000).ToString("F2");
            FirstMixTotalPpmTextBlock.Text = (firstMixNutrients["Total"] * 1000).ToString("F2");
            
            SecondMixNitrogenTextBlock.Text = (secondMixNutrients["Nitrogen"] * 1000).ToString("F2");
            SecondMixPhosphorusTextBlock.Text = (secondMixNutrients["Phosphorus"] * 1000).ToString("F2");
            SecondMixPotassiumTextBlock.Text = (secondMixNutrients["Potassium"] * 1000).ToString("F2");
            SecondMixCalciumTextBlock.Text = (secondMixNutrients["Calcium"] * 1000).ToString("F2");
            SecondMixMagnesiumTextBlock.Text = (secondMixNutrients["Magnesium"] * 1000).ToString("F2");
            SecondMixSulfurTextBlock.Text = (secondMixNutrients["Sulfur"] * 1000).ToString("F2");
            SecondMixBoronTextBlock.Text = (secondMixNutrients["Boron"] * 1000).ToString("F2");
            SecondMixCopperTextBlock.Text = (secondMixNutrients["Copper"] * 1000).ToString("F2");
            SecondMixIronTextBlock.Text = (secondMixNutrients["Iron"] * 1000).ToString("F2");
            SecondMixManganeseTextBlock.Text = (secondMixNutrients["Manganese"] * 1000).ToString("F2");
            SecondMixMolybdenumTextBlock.Text = (secondMixNutrients["Molybdenum"] * 1000).ToString("F2");
            SecondMixZincTextBlock.Text = (secondMixNutrients["Zinc"] * 1000).ToString("F2");
            SecondMixChlorineTextBlock.Text = (secondMixNutrients["Chlorine"] * 1000).ToString("F2");
            SecondMixSilicaTextBlock.Text = (secondMixNutrients["Silica"] * 1000).ToString("F2");
            SecondMixHumicAcidTextBlock.Text = (secondMixNutrients["HumicAcid"] * 1000).ToString("F2");
            SecondMixFulvicAcidTextBlock.Text = (secondMixNutrients["FulvicAcid"] * 1000).ToString("F2");
            SecondMixTotalPpmTextBlock.Text = (secondMixNutrients["Total"] * 1000).ToString("F2");
        }
    }
}
