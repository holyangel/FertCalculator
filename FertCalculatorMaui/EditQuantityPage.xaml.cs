using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FertCalculatorMaui
{
    public partial class EditQuantityPage : ContentPage
    {
        private EditQuantityViewModel viewModel;
        
        public event EventHandler<QuantityChangedEventArgs> QuantityChanged;

        public EditQuantityPage(string fertilizerName, double quantity, bool useImperialUnits)
        {
            InitializeComponent();
            
            viewModel = new EditQuantityViewModel(fertilizerName, quantity, useImperialUnits);
            BindingContext = viewModel;
        }

        private void OnIncrementGramClicked(object sender, EventArgs e)
        {
            var textValue = QuantityEntry.GetValue(Entry.TextProperty) as string;
            if (double.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                value += 1.0;
                viewModel.Quantity = value;
                QuantityEntry.SetValue(Entry.TextProperty, value.ToString("F1", CultureInfo.InvariantCulture));
            }
        }

        private void OnIncrementSmallClicked(object sender, EventArgs e)
        {
            var textValue = QuantityEntry.GetValue(Entry.TextProperty) as string;
            if (double.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                value += 0.1;
                viewModel.Quantity = value;
                QuantityEntry.SetValue(Entry.TextProperty, value.ToString("F1", CultureInfo.InvariantCulture));
            }
        }

        private void OnDecrementSmallClicked(object sender, EventArgs e)
        {
            var textValue = QuantityEntry.GetValue(Entry.TextProperty) as string;
            if (double.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                value -= 0.1;
                if (value < 0) value = 0;
                viewModel.Quantity = value;
                QuantityEntry.SetValue(Entry.TextProperty, value.ToString("F1", CultureInfo.InvariantCulture));
            }
        }

        private void OnDecrementGramClicked(object sender, EventArgs e)
        {
            var textValue = QuantityEntry.GetValue(Entry.TextProperty) as string;
            if (double.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                value -= 1.0;
                if (value < 0) value = 0;
                viewModel.Quantity = value;
                QuantityEntry.SetValue(Entry.TextProperty, value.ToString("F1", CultureInfo.InvariantCulture));
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var textValue = QuantityEntry.GetValue(Entry.TextProperty) as string;
            if (double.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double newQuantity))
            {
                QuantityChanged?.Invoke(this, new QuantityChangedEventArgs(viewModel.FertilizerName, newQuantity));
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Invalid Input", "Please enter a valid number", "OK");
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
        public double NewQuantity { get; }

        public QuantityChangedEventArgs(string fertilizerName, double newQuantity)
        {
            FertilizerName = fertilizerName;
            NewQuantity = newQuantity;
        }
    }
}
