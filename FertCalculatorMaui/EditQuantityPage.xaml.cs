using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FertCalculatorMaui
{
    public partial class EditQuantityPage : ContentPage
    {
        private string fertilizerName;
        private double currentQuantity;
        private bool useImperialUnits;

        public event EventHandler<QuantityChangedEventArgs> QuantityChanged;

        public EditQuantityPage(string fertilizerName, double quantity, bool useImperialUnits)
        {
            InitializeComponent();
            
            this.fertilizerName = fertilizerName;
            this.currentQuantity = quantity;
            this.useImperialUnits = useImperialUnits;
            
            FertilizerNameLabel.Text = fertilizerName;
            QuantityEntry.Text = quantity.ToString("F1", CultureInfo.InvariantCulture);
            UnitLabel.Text = useImperialUnits ? "g/gal" : "g/L";
        }

        private void OnIncrementGramClicked(object sender, EventArgs e)
        {
            if (double.TryParse(QuantityEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                value += 1.0;
                QuantityEntry.Text = value.ToString("F1", CultureInfo.InvariantCulture);
            }
        }
        
        private void OnDecrementGramClicked(object sender, EventArgs e)
        {
            if (double.TryParse(QuantityEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value) && value >= 1.0)
            {
                value -= 1.0;
                QuantityEntry.Text = value.ToString("F1", CultureInfo.InvariantCulture);
            }
        }
        
        private void OnIncrementSmallClicked(object sender, EventArgs e)
        {
            if (double.TryParse(QuantityEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                value += 0.1;
                value = Math.Round(value, 1);
                QuantityEntry.Text = value.ToString("F1", CultureInfo.InvariantCulture);
            }
        }
        
        private void OnDecrementSmallClicked(object sender, EventArgs e)
        {
            if (double.TryParse(QuantityEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value) && value >= 0.1)
            {
                value -= 0.1;
                value = Math.Round(value, 1);
                QuantityEntry.Text = value.ToString("F1", CultureInfo.InvariantCulture);
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (double.TryParse(QuantityEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double newQuantity))
            {
                // Ensure quantity is not negative
                if (newQuantity < 0)
                {
                    newQuantity = 0;
                }
                
                // Raise event to notify the main page
                QuantityChanged?.Invoke(this, new QuantityChangedEventArgs(fertilizerName, newQuantity));
                
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Invalid Input", "Please enter a valid number for quantity.", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }

    public class QuantityChangedEventArgs : EventArgs
    {
        public string FertilizerName { get; }
        public double Quantity { get; }

        public QuantityChangedEventArgs(string fertilizerName, double quantity)
        {
            FertilizerName = fertilizerName;
            Quantity = quantity;
        }
    }
}
