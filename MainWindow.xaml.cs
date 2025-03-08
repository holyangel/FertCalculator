using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace FertilizerCalculator
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Fertilizer> availableFertilizers;
        private ObservableCollection<FertilizerMix> savedMixes;
        private ObservableCollection<FertilizerQuantity> currentMix;
        private bool isMetricUnits = true; // true = per liter, false = per gallon
        private const double GALLON_TO_LITER = 3.78541;
        private string fertilizerDbPath = "Fertilizers.xml";
        private string mixesDbPath = "Mixes.xml";

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize collections
            availableFertilizers = new ObservableCollection<Fertilizer>();
            savedMixes = new ObservableCollection<FertilizerMix>();
            currentMix = new ObservableCollection<FertilizerQuantity>();
            
            // Load saved data
            LoadFertilizers();
            LoadMixes();
            
            // Set up data bindings
            FertilizerListBox.ItemsSource = availableFertilizers;
            MixListBox.ItemsSource = currentMix;
            SavedMixesComboBox.ItemsSource = savedMixes;
            
            // Set up unit toggle
            UpdateUnitDisplay();
        }

        private void LoadFertilizers()
        {
            if (File.Exists(fertilizerDbPath))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(List<Fertilizer>));
                    using (var reader = new StreamReader(fertilizerDbPath))
                    {
                        var fertilizers = (List<Fertilizer>)serializer.Deserialize(reader);
                        availableFertilizers.Clear();
                        foreach (var fertilizer in fertilizers)
                        {
                            availableFertilizers.Add(fertilizer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading fertilizers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveFertilizers()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(List<Fertilizer>));
                using (var writer = new StreamWriter(fertilizerDbPath))
                {
                    serializer.Serialize(writer, availableFertilizers.ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving fertilizers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FertilizerListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Check if a fertilizer is selected
            if (FertilizerListBox.SelectedItem is Fertilizer selectedFertilizer)
            {
                // Check if already in mix
                if (currentMix.Any(i => i.FertilizerName == selectedFertilizer.Name))
                {
                    MessageBox.Show("This fertilizer is already in the current mix.", "Already Added", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Add to mix
                var quantity = new FertilizerQuantity
                {
                    FertilizerName = selectedFertilizer.Name,
                    Quantity = 1.0 // Default quantity
                };

                currentMix.Add(quantity);
                UpdateNutrientTotals();

                // Optional: Show a status message
                StatusText.Text = $"Added {selectedFertilizer.Name} to mix";
            }
        }

        private void LoadMixes()
        {
            if (File.Exists(mixesDbPath))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(List<FertilizerMix>));
                    using (var reader = new StreamReader(mixesDbPath))
                    {
                        var mixes = (List<FertilizerMix>)serializer.Deserialize(reader);
                        savedMixes.Clear();
                        foreach (var mix in mixes)
                        {
                            savedMixes.Add(mix);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading mixes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Check for Ctrl+V (paste)
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                TextBox textBox = sender as TextBox;
                string clipboardText = Clipboard.GetText();

                // Get the text that would result after pasting
                string proposedText = textBox.Text.Substring(0, textBox.SelectionStart) +
                                     clipboardText +
                                     textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);

                // Check if the proposed text is a valid decimal number
                Regex regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
                if (!regex.IsMatch(proposedText))
                {
                    e.Handled = true; // Prevent the paste operation
                }
            }
        }

        private void MixListBox_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Quantity (g/L)")
            {
                if (e.EditingElement is TextBox textBox && e.Row.Item is FertilizerQuantity item)
                {
                    if (double.TryParse(textBox.Text, out double quantity))
                    {
                        item.Quantity = quantity;
                        UpdateNutrientTotals();
                    }
                    else
                    {
                        // If parsing fails, revert to the previous value
                        textBox.Text = item.Quantity.ToString("F2");
                        e.Cancel = true;
                    }
                }
            }
        }

        private void MixListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // Check if an item is selected
                if (MixListBox.SelectedItem is FertilizerQuantity selectedItem)
                {
                    // Remove the selected item
                    currentMix.Remove(selectedItem);

                    // Update nutrient totals
                    UpdateNutrientTotals();

                    // Update status
                    StatusText.Text = $"Removed {selectedItem.FertilizerName} from mix";

                    // Mark the event as handled to prevent the default DataGrid delete behavior
                    e.Handled = true;
                }
            }
        }

        private void MixListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Check if an item is selected
            if (MixListBox.SelectedItem is FertilizerQuantity selectedItem)
            {
                // Remove the selected item
                currentMix.Remove(selectedItem);
                UpdateNutrientTotals();

                // Update status
                StatusText.Text = $"Removed {selectedItem.FertilizerName} from mix";

                // Mark the event as handled
                e.Handled = true;
            }
        }

        private void SaveMixes()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(List<FertilizerMix>));
                using (var writer = new StreamWriter(mixesDbPath))
                {
                    serializer.Serialize(writer, savedMixes.ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving mixes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddFertilizerButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddFertilizerWindow();
            if (dialog.ShowDialog() == true)
            {
                // Check for duplicate names
                if (availableFertilizers.Any(f => f.Name == dialog.NewFertilizer.Name))
                {
                    MessageBox.Show("A fertilizer with this name already exists.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                availableFertilizers.Add(dialog.NewFertilizer);
                SaveFertilizers();
            }
        }

        private void EditFertilizerButton_Click(object sender, RoutedEventArgs e)
        {
            if (FertilizerListBox.SelectedItem is Fertilizer selectedFertilizer)
            {
                var dialog = new AddFertilizerWindow(selectedFertilizer);
                if (dialog.ShowDialog() == true)
                {
                    // Check for duplicate names if name changed
                    if (dialog.NewFertilizer.Name != selectedFertilizer.Name && 
                        availableFertilizers.Any(f => f.Name == dialog.NewFertilizer.Name))
                    {
                        MessageBox.Show("A fertilizer with this name already exists.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Update the fertilizer
                    int index = availableFertilizers.IndexOf(selectedFertilizer);
                    availableFertilizers[index] = dialog.NewFertilizer;
                    SaveFertilizers();
                    
                    // Update any mixes that use this fertilizer
                    UpdateMixesAfterFertilizerEdit(selectedFertilizer.Name, dialog.NewFertilizer.Name);
                }
            }
            else
            {
                MessageBox.Show("Please select a fertilizer to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateMixesAfterFertilizerEdit(string oldName, string newName)
        {
            // Update current mix
            foreach (var item in currentMix)
            {
                if (item.FertilizerName == oldName)
                {
                    item.FertilizerName = newName;
                }
            }
            
            // Update saved mixes
            foreach (var mix in savedMixes)
            {
                foreach (var item in mix.Ingredients)
                {
                    if (item.FertilizerName == oldName)
                    {
                        item.FertilizerName = newName;
                    }
                }
            }
            
            SaveMixes();
            UpdateNutrientTotals();
        }

        private void DeleteFertilizerButton_Click(object sender, RoutedEventArgs e)
        {
            if (FertilizerListBox.SelectedItem is Fertilizer selectedFertilizer)
            {
                // Check if the fertilizer is used in any mixes
                bool isUsed = currentMix.Any(i => i.FertilizerName == selectedFertilizer.Name) ||
                              savedMixes.Any(m => m.Ingredients.Any(i => i.FertilizerName == selectedFertilizer.Name));
                
                if (isUsed)
                {
                    var result = MessageBox.Show(
                        "This fertilizer is used in one or more mixes. Deleting it will remove it from all mixes. Continue?",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    
                    if (result != MessageBoxResult.Yes)
                        return;
                    
                    // Remove from current mix
                    for (int i = currentMix.Count - 1; i >= 0; i--)
                    {
                        if (currentMix[i].FertilizerName == selectedFertilizer.Name)
                            currentMix.RemoveAt(i);
                    }
                    
                    // Remove from saved mixes
                    foreach (var mix in savedMixes)
                    {
                        for (int i = mix.Ingredients.Count - 1; i >= 0; i--)
                        {
                            if (mix.Ingredients[i].FertilizerName == selectedFertilizer.Name)
                                mix.Ingredients.RemoveAt(i);
                        }
                    }
                    
                    SaveMixes();
                }
                
                availableFertilizers.Remove(selectedFertilizer);
                SaveFertilizers();
                UpdateNutrientTotals();
            }
            else
            {
                MessageBox.Show("Please select a fertilizer to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddToMixButton_Click(object sender, RoutedEventArgs e)
        {
            if (FertilizerListBox.SelectedItem is Fertilizer selectedFertilizer)
            {
                // Check if already in mix
                if (currentMix.Any(i => i.FertilizerName == selectedFertilizer.Name))
                {
                    MessageBox.Show("This fertilizer is already in the current mix.", "Already Added", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                var quantity = new FertilizerQuantity
                {
                    FertilizerName = selectedFertilizer.Name,
                    Quantity = 1.0 // Default quantity
                };
                
                currentMix.Add(quantity);
                UpdateNutrientTotals();
            }
            else
            {
                MessageBox.Show("Please select a fertilizer to add to the mix.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveFromMixButton_Click(object sender, RoutedEventArgs e)
        {
            if (MixListBox.SelectedItem is FertilizerQuantity selectedItem)
            {
                currentMix.Remove(selectedItem);
                UpdateNutrientTotals();
            }
            else
            {
                MessageBox.Show("Please select a fertilizer to remove from the mix.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void QuantityTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is FertilizerQuantity item)
            {
                // When the TextBox is loaded for editing, set the raw value without formatting
                textBox.Text = item.Quantity.ToString();

                // Select all text to make it easy to replace
                textBox.SelectAll();
            }
        }

        public void UpdateNutrientTotals()
        {
            double totalN = 0, totalP = 0, totalK = 0, totalCa = 0, totalMg = 0, totalS = 0;
            double totalB = 0, totalFe = 0, totalCu = 0, totalMn = 0, totalMo = 0, totalZn = 0;
            
            foreach (var item in currentMix)
            {
                var fertilizer = availableFertilizers.FirstOrDefault(f => f.Name == item.FertilizerName);
                if (fertilizer != null)
                {
                    double quantity = item.Quantity;
                    totalN += fertilizer.NitrogenPpm * quantity;
                    totalP += fertilizer.PhosphorusPpm * quantity;
                    totalK += fertilizer.PotassiumPpm * quantity;
                    totalCa += fertilizer.CalciumPpm * quantity;
                    totalMg += fertilizer.MagnesiumPpm * quantity;
                    totalS += fertilizer.SulfurPpm * quantity;
                    totalB += fertilizer.BoronPpm * quantity;
                    totalCu += fertilizer.CopperPpm * quantity;
                    totalFe += fertilizer.IronPpm * quantity;
                    totalMn += fertilizer.ManganesePpm * quantity;
                    totalMo += fertilizer.MolybdenumPpm * quantity;
                    totalZn += fertilizer.ZincPpm * quantity;
                }
            }
            
            // Apply unit conversion if needed
            double conversionFactor = isMetricUnits ? 1.0 : 1.0 / GALLON_TO_LITER;
            
            // Update UI
            NitrogenValueTextBlock.Text = (totalN * conversionFactor).ToString("F2");
            PhosphorusValueTextBlock.Text = (totalP * conversionFactor).ToString("F2");
            PotassiumValueTextBlock.Text = (totalK * conversionFactor).ToString("F2");
            CalciumValueTextBlock.Text = (totalCa * conversionFactor).ToString("F2");
            MagnesiumValueTextBlock.Text = (totalMg * conversionFactor).ToString("F2");
            SulfurValueTextBlock.Text = (totalS * conversionFactor).ToString("F2");
            BoronValueTextBlock.Text = (totalB * conversionFactor).ToString("F2");
            CopperValueTextBlock.Text = (totalCu * conversionFactor).ToString("F2");
            IronValueTextBlock.Text = (totalFe * conversionFactor).ToString("F2");
            ManganeseValueTextBlock.Text = (totalMn * conversionFactor).ToString("F2");
            MolybdenumValueTextBlock.Text = (totalMo * conversionFactor).ToString("F2");
            ZincValueTextBlock.Text = (totalZn * conversionFactor).ToString("F2");
            
            double totalPpm = totalN + totalP + totalK + totalCa + totalMg + totalS + 
                             totalB + totalCu + totalFe + totalMn + totalMo +totalZn + 
            TotalPpmValueTextBlock.Text = (totalPpm * conversionFactor).ToString("F2");
        }

        private void UnitToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isMetricUnits = !isMetricUnits;
            UpdateUnitDisplay();
            UpdateNutrientTotals();
        }

        private void UpdateUnitDisplay()
        {
            UnitToggleButton.Content = isMetricUnits ? "Switch to Gallon" : "Switch to Liter";
            UnitLabel.Text = isMetricUnits ? "PPM (mg/L)" : "PPM (mg/gal)";
        }

        private void SaveMixButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentMix.Count == 0)
            {
                MessageBox.Show("Cannot save an empty mix.", "Empty Mix", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var dialog = new SaveMixWindow();
            if (dialog.ShowDialog() == true)
            {
                string mixName = dialog.MixName;
                
                // Check for duplicate names
                if (savedMixes.Any(m => m.Name == mixName))
                {
                    var result = MessageBox.Show(
                        "A mix with this name already exists. Do you want to overwrite it?",
                        "Duplicate Name",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        // Remove existing mix
                        var existingMix = savedMixes.First(m => m.Name == mixName);
                        savedMixes.Remove(existingMix);
                    }
                    else
                    {
                        return;
                    }
                }
                
                // Create new mix
                var newMix = new FertilizerMix
                {
                    Name = mixName,
                    Ingredients = new List<FertilizerQuantity>()
                };
                
                // Copy ingredients
                foreach (var item in currentMix)
                {
                    newMix.Ingredients.Add(new FertilizerQuantity
                    {
                        FertilizerName = item.FertilizerName,
                        Quantity = item.Quantity
                    });
                }
                
                savedMixes.Add(newMix);
                SaveMixes();
                
                // Update combobox selection
                SavedMixesComboBox.SelectedItem = newMix;
            }
        }

        private void LoadMixButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedMixesComboBox.SelectedItem is FertilizerMix selectedMix)
            {
                // Confirm if current mix is not empty
                if (currentMix.Count > 0)
                {
                    var result = MessageBox.Show(
                        "Loading a mix will replace your current mix. Continue?",
                        "Confirm Load",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    
                    if (result != MessageBoxResult.Yes)
                        return;
                }
                
                // Clear current mix
                currentMix.Clear();
                
                // Add ingredients from selected mix
                foreach (var ingredient in selectedMix.Ingredients)
                {
                    // Verify the fertilizer still exists
                    if (availableFertilizers.Any(f => f.Name == ingredient.FertilizerName))
                    {
                        currentMix.Add(new FertilizerQuantity
                        {
                            FertilizerName = ingredient.FertilizerName,
                            Quantity = ingredient.Quantity
                        });
                    }
                }
                
                UpdateNutrientTotals();
            }
            else
            {
                MessageBox.Show("Please select a mix to load.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteMixButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavedMixesComboBox.SelectedItem is FertilizerMix selectedMix)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the mix '{selectedMix.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    savedMixes.Remove(selectedMix);
                    SaveMixes();
                }
            }
            else
            {
                MessageBox.Show("Please select a mix to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Export Fertilizers and Mixes"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Create export data
                    var exportData = new ExportData
                    {
                        Fertilizers = availableFertilizers.ToList(),
                        Mixes = savedMixes.ToList()
                    };
                    
                    // Serialize and save
                    var serializer = new XmlSerializer(typeof(ExportData));
                    using (var writer = new StreamWriter(dialog.FileName))
                    {
                        serializer.Serialize(writer, exportData);
                    }
                    
                    MessageBox.Show("Export completed successfully.", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Import Fertilizers and Mixes"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Deserialize import data
                    var serializer = new XmlSerializer(typeof(ExportData));
                    ExportData importData;
                    using (var reader = new StreamReader(dialog.FileName))
                    {
                        importData = (ExportData)serializer.Deserialize(reader);
                    }
                    
                    // Ask user what to import
                    var importWindow = new ImportOptionsWindow();
                    if (importWindow.ShowDialog() == true)
                    {
                        int importedFertilizers = 0;
                        int importedMixes = 0;
                        
                        // Import fertilizers if selected
                        if (importWindow.ImportFertilizers)
                        {
                            foreach (var fertilizer in importData.Fertilizers)
                            {
                                if (!availableFertilizers.Any(f => f.Name == fertilizer.Name))
                                {
                                    availableFertilizers.Add(fertilizer);
                                    importedFertilizers++;
                                }
                            }
                            SaveFertilizers();
                        }
                        
                        // Import mixes if selected
                        if (importWindow.ImportMixes)
                        {
                            foreach (var mix in importData.Mixes)
                            {
                                if (!savedMixes.Any(m => m.Name == mix.Name))
                                {
                                    // Verify all ingredients exist
                                    bool allIngredientsExist = true;
                                    foreach (var ingredient in mix.Ingredients)
                                    {
                                        if (!availableFertilizers.Any(f => f.Name == ingredient.FertilizerName))
                                        {
                                            allIngredientsExist = false;
                                            break;
                                        }
                                    }
                                    
                                    if (allIngredientsExist)
                                    {
                                        savedMixes.Add(mix);
                                        importedMixes++;
                                    }
                                }
                            }
                            SaveMixes();
                        }
                        
                        MessageBox.Show(
                            $"Import completed. Added {importedFertilizers} fertilizers and {importedMixes} mixes.",
                            "Import Complete",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing data: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearMixButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentMix.Count > 0)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to clear the current mix?",
                    "Confirm Clear",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    currentMix.Clear();
                    UpdateNutrientTotals();
                }
            }
        }
    }

    // Data models
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

        // New properties to store original input format and values
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

        public double BoronPercent
        {
            get => boronPercent;
            set { boronPercent = value; OnPropertyChanged(nameof(BoronPercent)); OnPropertyChanged(nameof(BoronPpm)); }
        }

        public double ManganesePercent
        {
            get => manganesePercent;
            set { manganesePercent = value; OnPropertyChanged(nameof(ManganesePercent)); OnPropertyChanged(nameof(ManganesePpm)); }
        }

        public double CopperPercent
        {
            get => copperPercent;
            set { copperPercent = value; OnPropertyChanged(nameof(CopperPercent)); OnPropertyChanged(nameof(CopperPpm)); }
        }

        public double MolybdenumPercent
        {
            get => molybdenumPercent;
            set { molybdenumPercent = value; OnPropertyChanged(nameof(MolybdenumPercent)); OnPropertyChanged(nameof(MolybdenumPpm)); }
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
            set
            {
                quantity = value;
                OnPropertyChanged(nameof(Quantity));
                // This line is optional if we're handling updates in the UI events
                // But it provides an additional way to ensure updates happen
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.UpdateNutrientTotals();
                    }
                }));
            }
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