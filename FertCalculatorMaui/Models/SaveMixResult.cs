using System;

namespace FertCalculatorMaui.Models
{
    public class SaveMixResult : EventArgs
    {
        public bool Success { get; set; }
        public string MixName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
