using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using FertCalculatorMaui.ViewModels;

namespace FertCalculatorMaui
{
    public partial class EditQuantityPage : ContentPage
    {
        private EditQuantityViewModel viewModel;
        
        public event EventHandler<QuantityChangedEventArgs> QuantityChanged;

        public EditQuantityPage(string fertilizerName, double quantity, string unitLabel, bool useImperialUnits)
        {
            InitializeComponent();
            
            viewModel = new EditQuantityViewModel(fertilizerName, quantity, unitLabel, useImperialUnits);
            BindingContext = viewModel;
        }

        // Parses the entry text using the device culture first, then invariant,
        // so both "." and locale decimal separators (e.g. ",") work.
        private static bool TryParseQuantity(string? text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
                || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private void AdjustQuantity(double delta)
        {
            var textValue = QuantityEntry.GetValue(Entry.TextProperty) as string;
            if (TryParseQuantity(textValue, out double value))
            {
                value = Math.Max(0, value + delta);
                viewModel.Quantity = value;
                QuantityEntry.SetValue(Entry.TextProperty, value.ToString("F3", CultureInfo.CurrentCulture));
            }
        }

        private void OnIncrement10GramClicked(object sender, EventArgs e) => AdjustQuantity(10.0);
        private void OnIncrement5GramClicked(object sender, EventArgs e) => AdjustQuantity(5.0);
        private void OnIncrementGramClicked(object sender, EventArgs e) => AdjustQuantity(1.0);
        private void OnIncrementSmallClicked(object sender, EventArgs e) => AdjustQuantity(0.1);
        private void OnIncrementExtraSmallClicked(object sender, EventArgs e) => AdjustQuantity(0.01);
        private void OnDecrement10GramClicked(object sender, EventArgs e) => AdjustQuantity(-10.0);
        private void OnDecrement5GramClicked(object sender, EventArgs e) => AdjustQuantity(-5.0);
        private void OnDecrementGramClicked(object sender, EventArgs e) => AdjustQuantity(-1.0);
        private void OnDecrementSmallClicked(object sender, EventArgs e) => AdjustQuantity(-0.1);
        private void OnDecrementExtraSmallClicked(object sender, EventArgs e) => AdjustQuantity(-0.01);

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var textValue = QuantityEntry.GetValue(Entry.TextProperty) as string;
            if (TryParseQuantity(textValue, out double newQuantity))
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
