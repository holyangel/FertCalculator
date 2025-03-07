using System.Windows;

namespace FertilizerCalculator
{
    public partial class SaveMixWindow : Window
    {
        public string MixName { get; private set; }

        public SaveMixWindow(string currentName = "")
        {
            InitializeComponent();

            // If editing an existing mix, pre-fill the name
            if (!string.IsNullOrEmpty(currentName))
            {
                MixNameTextBox.Text = currentName;
            }

            // Set focus to the text box
            MixNameTextBox.Focus();
            MixNameTextBox.SelectAll();
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