using System;
using System.ComponentModel;
using System.Diagnostics;

namespace FertCalculatorMaui.Services
{
    public class AppSettings : INotifyPropertyChanged
    {
        private const string UNIT_PREFERENCE_KEY = "UseImperialUnits";
        private const string BUTTON_COLOR_KEY = "ButtonColor";
        private const string BUTTON_TEXT_COLOR_KEY = "ButtonTextColor";
        
        private bool useImperialUnits;
        private string buttonColor = "#FF512BD4"; // Purple
        private string buttonTextColor = "#FFFFFFFF"; // White
        
        public AppSettings()
        {
            // Load settings from preferences
            try
            {
                UseImperialUnits = Preferences.Default.Get(UNIT_PREFERENCE_KEY, false);
                ButtonColor = Preferences.Default.Get(BUTTON_COLOR_KEY, "#FF512BD4");
                ButtonTextColor = Preferences.Default.Get(BUTTON_TEXT_COLOR_KEY, "#FFFFFFFF");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                // Use default values if there's an error
                UseImperialUnits = false;
                ButtonColor = "#FF512BD4";
                ButtonTextColor = "#FFFFFFFF";
            }
        }
        
        public bool UseImperialUnits
        {
            get => useImperialUnits;
            set 
            { 
                if (useImperialUnits != value)
                {
                    useImperialUnits = value; 
                    OnPropertyChanged(nameof(UseImperialUnits));
                    OnPropertyChanged(nameof(UnitLabel));
                    
                    // Save the setting when it changes
                    try
                    {
                        Preferences.Default.Set(UNIT_PREFERENCE_KEY, value);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error saving settings: {ex.Message}");
                    }
                }
            }
        }
        
        public string ButtonColor
        {
            get => buttonColor;
            set
            {
                if (buttonColor != value)
                {
                    buttonColor = value;
                    OnPropertyChanged(nameof(ButtonColor));
                    
                    // Save the setting when it changes
                    try
                    {
                        Preferences.Default.Set(BUTTON_COLOR_KEY, value);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error saving button color: {ex.Message}");
                    }
                }
            }
        }
        
        public string ButtonTextColor
        {
            get => buttonTextColor;
            set
            {
                if (buttonTextColor != value)
                {
                    buttonTextColor = value;
                    OnPropertyChanged(nameof(ButtonTextColor));
                    
                    // Save the setting when it changes
                    try
                    {
                        Preferences.Default.Set(BUTTON_TEXT_COLOR_KEY, value);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error saving button text color: {ex.Message}");
                    }
                }
            }
        }
        
        public string UnitLabel => UseImperialUnits ? "PPM per gallon" : "PPM per liter";
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void Save()
        {
            try
            {
                Preferences.Default.Set(UNIT_PREFERENCE_KEY, UseImperialUnits);
                Preferences.Default.Set(BUTTON_COLOR_KEY, ButtonColor);
                Preferences.Default.Set(BUTTON_TEXT_COLOR_KEY, ButtonTextColor);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}
