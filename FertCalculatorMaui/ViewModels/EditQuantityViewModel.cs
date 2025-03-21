using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FertCalculatorMaui.ViewModels
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

        public EditQuantityViewModel(string fertilizerName, double quantity, string unitLabel, bool useImperialUnits)
        {
            FertilizerName = fertilizerName;
            Quantity = quantity;
            UnitLabel = unitLabel;
            UseImperialUnits = useImperialUnits;
        }
    }
}
