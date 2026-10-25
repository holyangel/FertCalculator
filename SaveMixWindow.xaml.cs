using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FertilizerCalculator
{
    public partial class SaveMixWindow : Window
    {
        public string MixName { get; private set; }
        private bool updatingTextFromSelection;

        public SaveMixWindow(IEnumerable<string> existingMixNames, string currentName = "")
        {
            InitializeComponent();

            // Populate the dropdown of saved mixes for quick updating
            foreach (var name in existingMixNames)
            {
                ExistingMixesComboBox.Items.Add(name);
            }

            // If editing an existing mix, pre-fill the name
            if (!string.IsNullOrEmpty(currentName))
            {
                MixNameTextBox.Text = currentName;
            }

            // Set focus to the text box
            MixNameTextBox.Focus();
            MixNameTextBox.SelectAll();
        }

        private void ExistingMixesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExistingMixesComboBox.SelectedItem is string name)
            {
                // Fill the name box with the selected mix so it can be updated
                updatingTextFromSelection = true;
                MixNameTextBox.Text = name;
                updatingTextFromSelection = false;
            }
        }

        private void MixNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // If the user edits the name after picking a mix, clear the dropdown
            // so the dropdown and the text can't disagree about what's being saved
            if (!updatingTextFromSelection && ExistingMixesComboBox.SelectedItem != null)
            {
                ExistingMixesComboBox.SelectedItem = null;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(MixNameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for this mix.",
                    "Missing Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                MixNameTextBox.Focus();
                return;
            }

            MixName = MixNameTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
