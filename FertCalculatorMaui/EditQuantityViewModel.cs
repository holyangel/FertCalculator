using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FertCalculatorMaui
{
    public partial class EditQuantityViewModel : ObservableObject
    {
        [ObservableProperty]
        private string fertilizerName;

        [ObservableProperty]
        private double quantity;

        [ObservableProperty]
        private string unitLabel;

        [ObservableProperty]
        private bool useImperialUnits;

        public EditQuantityViewModel(string fertilizerName, double quantity, bool useImperialUnits)
        {
            FertilizerName = fertilizerName;
            Quantity = quantity;
            UseImperialUnits = useImperialUnits;
            UnitLabel = useImperialUnits ? "g/gal" : "g/L";
        }
    }
}
