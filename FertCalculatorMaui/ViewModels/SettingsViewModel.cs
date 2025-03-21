using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FertCalculatorMaui.Services;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FertCalculatorMaui.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IDialogService dialogService;
        private readonly FileService fileService;
        private readonly AppSettings appSettings;

        [ObservableProperty]
        private Color buttonBackgroundColor = Colors.DarkGreen;

        [ObservableProperty]
        private Color buttonTextColor = Colors.White;

        public SettingsViewModel(FileService fileService, IDialogService dialogService, AppSettings appSettings)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            
            // Load saved settings
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (!string.IsNullOrEmpty(appSettings.ButtonColor))
                {
                    ButtonBackgroundColor = Color.FromArgb(appSettings.ButtonColor);
                }
                
                if (!string.IsNullOrEmpty(appSettings.ButtonTextColor))
                {
                    ButtonTextColor = Color.FromArgb(appSettings.ButtonTextColor);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                // Use default colors if there's an error
            }
        }

        [RelayCommand]
        private void ResetToDefaults()
        {
            try
            {
                ButtonBackgroundColor = Colors.DarkGreen;
                ButtonTextColor = Colors.White;
                SaveSettings();
            }
            catch (Exception ex)
            {
                dialogService.DisplayAlert("Error", $"Failed to reset colors: {ex.Message}", "OK");
            }
        }

        public void SaveSettings()
        {
            try
            {
                appSettings.ButtonColor = ButtonBackgroundColor.ToArgbHex();
                appSettings.ButtonTextColor = ButtonTextColor.ToArgbHex();
                appSettings.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
                dialogService.DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
            }
        }
    }
}
