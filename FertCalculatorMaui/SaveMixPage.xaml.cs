using System.Collections.ObjectModel;
using System.ComponentModel;
using FertCalculatorMaui.Services;
using FertCalculatorMaui.ViewModels;
using FertCalculatorMaui.Models;

namespace FertCalculatorMaui;

public partial class SaveMixPage : ContentPage
{
    private SaveMixViewModel _viewModel;
    
    public SaveMixViewModel ViewModel => _viewModel;
    
    public SaveMixPage(FileService fileService, List<FertilizerQuantity> ingredients, ObservableCollection<FertilizerMix> existingMixes = null)
    {
        InitializeComponent();
        _viewModel = new SaveMixViewModel(fileService, ingredients, existingMixes);
        
        // Set up commands
        _viewModel.CancelCommand = new Command(async () => await Navigation.PopAsync());
        _viewModel.SaveCompletedEvent += async (sender, e) => 
        {
            if (e.Success)
            {
                await DisplayAlert("Success", "Mix saved successfully.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", e.ErrorMessage ?? "Failed to save mix.", "OK");
            }
        };
        
        _viewModel.DisplayAlertAsync = DisplayAlertAsync;
        
        BindingContext = _viewModel;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // If using pre-loaded mixes, we don't need to reload them here
        if (!_viewModel.HasExistingMixes)
        {
            System.Diagnostics.Debug.WriteLine("No pre-loaded mixes found, loading from file...");
            _viewModel.LoadExistingMixesAsync();
        }
        
        System.Diagnostics.Debug.WriteLine($"OnAppearing: HasExistingMixes = {_viewModel.HasExistingMixes}, ExistingMixes count = {_viewModel.ExistingMixes?.Count}");
    }
    
    private async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
    {
        return await DisplayAlert(title, message, accept, cancel);
    }
    
    private void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.MixName))
        {
            DisplayAlert("Error", "Please enter a name for this mix.", "OK");
            return;
        }
        
        if (_viewModel.SaveCommand.CanExecute(null))
        {
            _viewModel.SaveCommand.Execute(null);
        }
    }
    
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (_viewModel.CancelCommand != null && _viewModel.CancelCommand.CanExecute(null))
        {
            _viewModel.CancelCommand.Execute(null);
        }
        else
        {
            // Fallback if command not set
            await Navigation.PopAsync();
        }
    }
    
    // Event to provide results back to the caller
    public event EventHandler<Models.SaveMixResult> SaveCompleted
    {
        add => _viewModel.SaveCompletedEvent += value;
        remove => _viewModel.SaveCompletedEvent -= value;
    }
}
