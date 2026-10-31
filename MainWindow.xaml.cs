using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using FertCalculator.Core;
using FertCalculator.Properties;

namespace FertilizerCalculator
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Fertilizer> availableFertilizers;
        private ObservableCollection<FertilizerMix> savedMixes;
        private ObservableCollection<FertilizerQuantity> currentMix;
        private bool isMetricUnits = true; // true = per liter, false = per gallon
        private string loadedMixName = null; // name of the mix the current mix was loaded/saved as
        private readonly string fertilizerDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fertilizers.xml");
        private readonly string mixesDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mixes.xml");

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

            // Restore saved unit preference (true = imperial)
            isMetricUnits = !Settings.Default.UnitPreference;
            UpdateUnitDisplay();
            UpdateCurrentMixLabel();

            // Live filtering for the fertilizer list
            CollectionViewSource.GetDefaultView(availableFertilizers).Filter = item =>
                string.IsNullOrEmpty(FertilizerSearchBox.Text) ||
                ((Fertilizer)item).Name?.IndexOf(FertilizerSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void FertilizerSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(availableFertilizers).Refresh();
        }

        // Shows which saved mix the current ingredients belong to
        private void UpdateCurrentMixLabel()
        {
            CurrentMixNameText.Text = loadedMixName == null ? "— unsaved —" : $"— {loadedMixName} —";
        }

        // Any edit to the current mix means it no longer matches the loaded mix
        private void MarkMixModified()
        {
            if (loadedMixName != null)
            {
                loadedMixName = null;
                UpdateCurrentMixLabel();
            }
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
                        foreach (var fertilizer in fertilizers.OrderBy(f => f.Name, new FertilizerNameComparer()))
                        {
                            availableFertilizers.Add(fertilizer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    QuarantineCorruptFile(fertilizerDbPath);
                    MessageBox.Show($"Error loading fertilizers: {ex.Message}\n\nThe file was moved to {fertilizerDbPath}.corrupt and a fresh database will be used.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Writes XML via a temp file and keeps a .bak copy of the previous version,
        // so a crash or bad save never leaves the real file truncated.
        private static void SaveXml<T>(T data, string path)
        {
            var serializer = new XmlSerializer(typeof(T));
            string tempPath = path + ".tmp";
            using (var writer = new StreamWriter(tempPath))
            {
                serializer.Serialize(writer, data);
            }
            if (File.Exists(path))
            {
                File.Copy(path, path + ".bak", true);
            }
            File.Copy(tempPath, path, true);
            File.Delete(tempPath);
        }

        // Renames an unreadable data file aside so the app can start clean
        // without silently overwriting the user's data on the next save.
        private static void QuarantineCorruptFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string quarantinePath = path + ".corrupt";
                    File.Copy(path, quarantinePath, true);
                    File.Delete(path);
                }
            }
            catch { }
        }

        private void SaveFertilizers()
        {
            try
            {
                SaveXml(availableFertilizers.ToList(), fertilizerDbPath);
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
                if (currentMix.Any(i => string.Equals(i.FertilizerName, selectedFertilizer.Name, StringComparison.OrdinalIgnoreCase)))
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
                MarkMixModified();

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
                    QuarantineCorruptFile(mixesDbPath);
                    MessageBox.Show($"Error loading mixes: {ex.Message}\n\nThe file was moved to {mixesDbPath}.corrupt and a fresh database will be used.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Returns true if the proposed text is a valid partial/complete non-negative
        // decimal number using the current culture's decimal separator.
        private static bool IsValidDecimalText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (text == separator)
                return true; // user just typed the separator

            return double.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out _);
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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Check for Ctrl+V (paste)
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                TextBox textBox = sender as TextBox;
                if (textBox == null || !Clipboard.ContainsText())
                {
                    e.Handled = true;
                    return;
                }

                string clipboardText;
                try
                {
                    clipboardText = Clipboard.GetText();
                }
                catch
                {
                    e.Handled = true;
                    return;
                }

                // Get the text that would result after pasting (replacing any selection)
                string proposedText = textBox.Text
                    .Remove(textBox.SelectionStart, textBox.SelectionLength)
                    .Insert(textBox.SelectionStart, clipboardText);

                if (!IsValidDecimalText(proposedText))
                {
                    e.Handled = true; // Prevent the paste operation
                }
            }
        }

        private void MixListBox_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column == QuantityColumn)
            {
                if (e.EditingElement is TextBox textBox && e.Row.Item is FertilizerQuantity item)
                {
                    if (double.TryParse(textBox.Text, out double quantity))
                    {
                        item.Quantity = quantity;
                        UpdateNutrientTotals();
                        MarkMixModified();
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
                    MarkMixModified();

                    // Update status
                    StatusText.Text = $"Removed {selectedItem.FertilizerName} from mix";

                    // Mark the event as handled to prevent the default DataGrid delete behavior
                    e.Handled = true;
                }
            }
        }

        private void MixListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked element
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            // Get the clicked cell — Column is null when nothing is current
            // (e.g. double-clicking an empty area of the grid)
            var cell = dataGrid.CurrentCell;
            if (cell.Column == null) return;

            // Check if the clicked column is the fertilizer name column (first column)
            if (cell.Column.DisplayIndex == 0 && MixListBox.SelectedItem is FertilizerQuantity selectedItem)
            {
                // Remove the selected item
                currentMix.Remove(selectedItem);
                UpdateNutrientTotals();
                MarkMixModified();

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
                SaveXml(savedMixes.ToList(), mixesDbPath);
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
                if (availableFertilizers.Any(f => string.Equals(f.Name, dialog.NewFertilizer.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A fertilizer with this name already exists.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Add the new fertilizer
                availableFertilizers.Add(dialog.NewFertilizer);

                // Re-sort the list alphabetically
                var sortedList = availableFertilizers.OrderBy(f => f.Name, new FertilizerNameComparer()).ToList();
                availableFertilizers.Clear();
                foreach (var fertilizer in sortedList)
                {
                    availableFertilizers.Add(fertilizer);
                }
                
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
                    if (!string.Equals(dialog.NewFertilizer.Name, selectedFertilizer.Name, StringComparison.OrdinalIgnoreCase) && 
                        availableFertilizers.Any(f => string.Equals(f.Name, dialog.NewFertilizer.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A fertilizer with this name already exists.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Update the fertilizer
                    int index = availableFertilizers.IndexOf(selectedFertilizer);
                    availableFertilizers[index] = dialog.NewFertilizer;

                    // Re-sort the list alphabetically
                    var sortedList = availableFertilizers.OrderBy(f => f.Name, new FertilizerNameComparer()).ToList();
                    availableFertilizers.Clear();
                    foreach (var fertilizer in sortedList)
                    {
                        availableFertilizers.Add(fertilizer);
                    }
                    
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
                if (string.Equals(item.FertilizerName, oldName, StringComparison.OrdinalIgnoreCase))
                {
                    item.FertilizerName = newName;
                }
            }
            
            // Update saved mixes
            foreach (var mix in savedMixes)
            {
                foreach (var item in mix.Ingredients)
                {
                    if (string.Equals(item.FertilizerName, oldName, StringComparison.OrdinalIgnoreCase))
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
                bool isUsed = currentMix.Any(i => string.Equals(i.FertilizerName, selectedFertilizer.Name, StringComparison.OrdinalIgnoreCase)) ||
                              savedMixes.Any(m => m.Ingredients.Any(i => string.Equals(i.FertilizerName, selectedFertilizer.Name, StringComparison.OrdinalIgnoreCase)));
                
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
                        if (string.Equals(currentMix[i].FertilizerName, selectedFertilizer.Name, StringComparison.OrdinalIgnoreCase))
                            currentMix.RemoveAt(i);
                    }
                    
                    // Remove from saved mixes
                    foreach (var mix in savedMixes)
                    {
                        for (int i = mix.Ingredients.Count - 1; i >= 0; i--)
                        {
                            if (string.Equals(mix.Ingredients[i].FertilizerName, selectedFertilizer.Name, StringComparison.OrdinalIgnoreCase))
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
                if (currentMix.Any(i => string.Equals(i.FertilizerName, selectedFertilizer.Name, StringComparison.OrdinalIgnoreCase)))
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
                MarkMixModified();
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
                MarkMixModified();
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
            var totals = MixCalculator.Calculate(currentMix, availableFertilizers, !isMetricUnits);

            NitrogenValueTextBlock.Text = totals.Nitrogen.ToString("F2");
            PhosphorusValueTextBlock.Text = totals.Phosphorus.ToString("F2");
            PotassiumValueTextBlock.Text = totals.Potassium.ToString("F2");
            CalciumValueTextBlock.Text = totals.Calcium.ToString("F2");
            MagnesiumValueTextBlock.Text = totals.Magnesium.ToString("F2");
            SulfurValueTextBlock.Text = totals.Sulfur.ToString("F2");
            BoronValueTextBlock.Text = totals.Boron.ToString("F2");
            CopperValueTextBlock.Text = totals.Copper.ToString("F2");
            IronValueTextBlock.Text = totals.Iron.ToString("F2");
            ManganeseValueTextBlock.Text = totals.Manganese.ToString("F2");
            MolybdenumValueTextBlock.Text = totals.Molybdenum.ToString("F2");
            ZincValueTextBlock.Text = totals.Zinc.ToString("F2");
            ChlorineValueTextBlock.Text = totals.Chlorine.ToString("F2");
            SilicaValueTextBlock.Text = totals.Silica.ToString("F2");
            HumicAcidValueTextBlock.Text = totals.HumicAcid.ToString("F2");
            FulvicAcidValueTextBlock.Text = totals.FulvicAcid.ToString("F2");
            KelpValueTextBlock.Text = totals.Kelp.ToString("F2");
            TotalPpmValueTextBlock.Text = totals.Total.ToString("F2");
        }

        private void CopyBreakdown_Click(object sender, RoutedEventArgs e)
        {
            var rows = new (string Name, TextBox Box)[]
            {
                ("Nitrogen (N)", NitrogenValueTextBlock),
                ("Phosphorus (P)", PhosphorusValueTextBlock),
                ("Potassium (K)", PotassiumValueTextBlock),
                ("Calcium (Ca)", CalciumValueTextBlock),
                ("Magnesium (Mg)", MagnesiumValueTextBlock),
                ("Sulfur (S)", SulfurValueTextBlock),
                ("Boron (B)", BoronValueTextBlock),
                ("Copper (Cu)", CopperValueTextBlock),
                ("Iron (Fe)", IronValueTextBlock),
                ("Manganese (Mn)", ManganeseValueTextBlock),
                ("Molybdenum (Mo)", MolybdenumValueTextBlock),
                ("Zinc (Zn)", ZincValueTextBlock),
                ("Chlorine (Cl)", ChlorineValueTextBlock),
                ("Silica (Si)", SilicaValueTextBlock),
                ("Humic Acid", HumicAcidValueTextBlock),
                ("Fulvic Acid", FulvicAcidValueTextBlock),
                ("Kelp", KelpValueTextBlock),
                ("TOTAL PPM", TotalPpmValueTextBlock),
            };

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Nutrient\t{(isMetricUnits ? "PPM (per liter)" : "PPM (per gallon)")}");
            foreach (var (name, box) in rows)
                sb.AppendLine($"{name}\t{box.Text}");

            try
            {
                Clipboard.SetText(sb.ToString());
                StatusText.Text = "Nutrient breakdown copied to clipboard";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not access the clipboard: {ex.Message}", "Copy Failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UnitToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isMetricUnits = !isMetricUnits;
            Settings.Default.UnitPreference = !isMetricUnits;
            Settings.Default.Save();
            UpdateUnitDisplay();
            UpdateNutrientTotals();
        }

        private void UpdateUnitDisplay()
        {
            UnitToggleButton.Content = isMetricUnits ? "Switch to Gallon" : "Switch to Liter";
            UnitLabel.Text = isMetricUnits ? "PPM (mg/L) — quantities in g or mL/L" : "PPM (mg/L) — quantities in g or mL/gal";
            QuantityColumn.Header = isMetricUnits ? "Quantity (g or mL/L)" : "Quantity (g or mL/gal)";
        }

        private void SaveMixButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentMix.Count == 0)
            {
                MessageBox.Show("Cannot save an empty mix.", "Empty Mix", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var dialog = new SaveMixWindow(savedMixes.Select(m => m.Name));
            if (dialog.ShowDialog() == true)
            {
                string mixName = dialog.MixName;

                // If the name matches an existing mix, offer to update it in place
                var existingMix = savedMixes.FirstOrDefault(m => string.Equals(m.Name, mixName, StringComparison.OrdinalIgnoreCase));
                if (existingMix != null)
                {
                    var result = MessageBox.Show(
                        $"Update the saved mix '{existingMix.Name}' with the current mix?",
                        "Update Mix",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                        return;

                    // Replace the existing mix's ingredients (keeping it in place)
                    existingMix.Name = mixName;
                    existingMix.Ingredients = currentMix.Select(item => new FertilizerQuantity
                    {
                        FertilizerName = item.FertilizerName,
                        Quantity = item.Quantity
                    }).ToList();

                    SaveMixes();
                    loadedMixName = mixName;
                    UpdateCurrentMixLabel();
                    SavedMixesComboBox.SelectedItem = existingMix;
                    StatusText.Text = $"Updated mix '{mixName}'";
                    return;
                }

                // Create new mix
                var newMix = new FertilizerMix
                {
                    Name = mixName,
                    Ingredients = currentMix.Select(item => new FertilizerQuantity
                    {
                        FertilizerName = item.FertilizerName,
                        Quantity = item.Quantity
                    }).ToList()
                };

                savedMixes.Add(newMix);
                SaveMixes();
                loadedMixName = mixName;
                UpdateCurrentMixLabel();

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
                int skipped = 0;
                foreach (var ingredient in selectedMix.Ingredients)
                {
                    // Verify the fertilizer still exists
                    if (availableFertilizers.Any(f => string.Equals(f.Name, ingredient.FertilizerName, StringComparison.OrdinalIgnoreCase)))
                    {
                        currentMix.Add(new FertilizerQuantity
                        {
                            FertilizerName = ingredient.FertilizerName,
                            Quantity = ingredient.Quantity
                        });
                    }
                    else
                    {
                        skipped++;
                    }
                }
                
                loadedMixName = selectedMix.Name;
                UpdateCurrentMixLabel();
                UpdateNutrientTotals();

                if (skipped > 0)
                    StatusText.Text = $"Loaded '{selectedMix.Name}' ({skipped} ingredient(s) skipped — fertilizer not found)";
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
                var result = MessageBox.Show($"Are you sure you want to delete the mix '{selectedMix.Name}'?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    savedMixes.Remove(selectedMix);
                    SaveMixes();
                    StatusText.Text = $"Deleted mix '{selectedMix.Name}'";
                }
            }
            else
            {
                MessageBox.Show("Please select a mix to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CompareMixesButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if we have at least one mix to compare
            if (savedMixes.Count == 0)
            {
                MessageBox.Show("You need to have at least one saved mix to compare.", 
                    "No Mixes Available", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Open the comparison window
            var compareWindow = new CompareMixesWindow(savedMixes.ToList(), availableFertilizers.ToList(), isMetricUnits);
            compareWindow.Owner = this;
            compareWindow.ShowDialog();
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
                    // Deserialize import data (accepts ExportData files or raw
                    // Fertilizers.xml / Mixes.xml database files)
                    ExportData importData = DeserializeImportFile(dialog.FileName);
                    if (importData == null)
                    {
                        MessageBox.Show("The selected file is not a recognized FertCalculator data file.",
                            "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
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
                                if (!availableFertilizers.Any(f => string.Equals(f.Name, fertilizer.Name, StringComparison.OrdinalIgnoreCase)))
                                {
                                    availableFertilizers.Add(fertilizer);
                                    importedFertilizers++;
                                }
                            }
                            
                            // Sort the fertilizers alphabetically after import
                            var sortedFertilizers = availableFertilizers.OrderBy(f => f.Name, new FertilizerNameComparer()).ToList();
                            availableFertilizers.Clear();
                            foreach (var fertilizer in sortedFertilizers)
                            {
                                availableFertilizers.Add(fertilizer);
                            }
                            
                            SaveFertilizers();
                        }
                        
                        // Import mixes if selected
                        if (importWindow.ImportMixes)
                        {
                            foreach (var mix in importData.Mixes)
                            {
                                if (!savedMixes.Any(m => string.Equals(m.Name, mix.Name, StringComparison.OrdinalIgnoreCase)))
                                {
                                    // Verify all ingredients exist
                                    bool allIngredientsExist = true;
                                    foreach (var ingredient in mix.Ingredients)
                                    {
                                        if (!availableFertilizers.Any(f => string.Equals(f.Name, ingredient.FertilizerName, StringComparison.OrdinalIgnoreCase)))
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

        // Tries to read the file as an ExportData document, then falls back to the
        // raw database formats (ArrayOfFertilizer / ArrayOfFertilizerMix).
        private static ExportData DeserializeImportFile(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ExportData));
                using (var reader = new StreamReader(path))
                {
                    return (ExportData)serializer.Deserialize(reader);
                }
            }
            catch (InvalidOperationException) { }

            try
            {
                var serializer = new XmlSerializer(typeof(List<Fertilizer>));
                using (var reader = new StreamReader(path))
                {
                    var fertilizers = (List<Fertilizer>)serializer.Deserialize(reader);
                    return new ExportData { Fertilizers = fertilizers };
                }
            }
            catch (InvalidOperationException) { }

            try
            {
                var serializer = new XmlSerializer(typeof(List<FertilizerMix>));
                using (var reader = new StreamReader(path))
                {
                    var mixes = (List<FertilizerMix>)serializer.Deserialize(reader);
                    return new ExportData { Mixes = mixes };
                }
            }
            catch (InvalidOperationException) { }

            return null;
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
                    MarkMixModified();
                    StatusText.Text = $"Cleared mix";
                }
            }
        }
    }
}