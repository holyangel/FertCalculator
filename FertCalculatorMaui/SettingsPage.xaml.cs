using FertCalculatorMaui.Services;
using FertCalculatorMaui.ViewModels;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace FertCalculatorMaui;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel viewModel;
    
    public SettingsPage(FileService fileService, IDialogService dialogService, AppSettings appSettings)
    {
        try
        {
            Debug.WriteLine("SettingsPage constructor starting");
            InitializeComponent();
            
            // Initialize ViewModel
            viewModel = new SettingsViewModel(fileService, dialogService, appSettings);
            BindingContext = viewModel;
            
            Debug.WriteLine("SettingsPage constructor completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in SettingsPage constructor: {ex.Message}\n{ex.StackTrace}");
            throw; // Rethrow to let the DI container handle it
        }
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("SettingsPage OnAppearing");
        
        // Set initial colors on the pickers
        SetInitialColors();
    }
    
    private void SetInitialColors()
    {
        try
        {
            Debug.WriteLine("Setting initial colors for pickers");
            
            // We need to set the initial colors programmatically since we can't bind to them
            if (BackgroundColorPicker != null)
            {
                // Set the initial color for the background color picker
                // The color pickers don't have a direct way to set the color, 
                // so we'll just wait for the user to interact with them
                Debug.WriteLine($"Current background color: {viewModel.ButtonBackgroundColor}");
            }
            
            if (TextColorPicker != null)
            {
                // Set the initial color for the text color picker
                Debug.WriteLine($"Current text color: {viewModel.ButtonTextColor}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SetInitialColors: {ex.Message}");
        }
    }
    
    private void BackgroundColorPicker_PickedColorChanged(object sender, Color colorPicked)
    {
        try
        {
            viewModel.ButtonBackgroundColor = colorPicked;
            viewModel.SaveSettings();
            Debug.WriteLine($"Background color changed to: {colorPicked.ToArgbHex()}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in BackgroundColorPicker_PickedColorChanged: {ex.Message}");
        }
    }
    
    private void TextColorPicker_PickedColorChanged(object sender, Color colorPicked)
    {
        try
        {
            viewModel.ButtonTextColor = colorPicked;
            viewModel.SaveSettings();
            Debug.WriteLine($"Text color changed to: {colorPicked.ToArgbHex()}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in TextColorPicker_PickedColorChanged: {ex.Message}");
        }
    }
}
