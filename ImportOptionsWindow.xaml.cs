using System.Windows;

namespace FertilizerCalculator
{
    public partial class ImportOptionsWindow : Window
    {
        public bool ImportFertilizers { get; private set; }
        public bool ImportMixes { get; private set; }

        public ImportOptionsWindow()
        {
            InitializeComponent();

            // Default both options to true
            ImportFertilizersCheckBox.IsChecked = true;
            ImportMixesCheckBox.IsChecked = true;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected options
            ImportFertilizers = ImportFertilizersCheckBox.IsChecked ?? false;
            ImportMixes = ImportMixesCheckBox.IsChecked ?? false;

            // Validate that at least one option is selected
            if (!ImportFertilizers && !ImportMixes)
            {
                MessageBox.Show("Please select at least one option to import.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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