using System.Collections.ObjectModel;
using System.ComponentModel;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

public partial class SaveMixPage : ContentPage
{
    private SaveMixViewModel _viewModel;
    
    public SaveMixViewModel ViewModel => _viewModel;
    
    public SaveMixPage(FileService fileService, List<FertilizerQuantity> ingredients)
    {
        InitializeComponent();
        _viewModel = new SaveMixViewModel(fileService, ingredients);
        
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
    public event EventHandler<SaveMixResult> SaveCompleted
    {
        add => _viewModel.SaveCompletedEvent += value;
        remove => _viewModel.SaveCompletedEvent -= value;
    }
}

public partial class SaveMixViewModel : INotifyPropertyChanged
{
    private readonly FileService _fileService;
    private readonly List<FertilizerQuantity> _ingredients;
    private string _mixName = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSaving;
    
    public delegate Task<bool> DisplayAlertDelegate(string title, string message, string accept, string cancel);
    public DisplayAlertDelegate DisplayAlertAsync { get; set; }
    
    public SaveMixViewModel(FileService fileService, List<FertilizerQuantity> ingredients)
    {
        _fileService = fileService;
        _ingredients = ingredients;
        SaveCommand = new Command(ExecuteSave, CanExecuteSave);
    }
    
    public string MixName
    {
        get => _mixName;
        set
        {
            if (_mixName != value)
            {
                _mixName = value;
                OnPropertyChanged(nameof(MixName));
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }
    }
    
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }
    }
    
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    
    public bool IsSaving
    {
        get => _isSaving;
        set
        {
            if (_isSaving != value)
            {
                _isSaving = value;
                OnPropertyChanged(nameof(IsSaving));
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }
    }
    
    public System.Windows.Input.ICommand SaveCommand { get; }
    public System.Windows.Input.ICommand CancelCommand { get; set; }
    
    public event EventHandler<SaveMixResult> SaveCompletedEvent;
    
    private bool CanExecuteSave()
    {
        return !string.IsNullOrWhiteSpace(MixName) && !IsSaving;
    }
    
    private async void ExecuteSave()
    {
        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;
            
            // Validate mix name
            if (string.IsNullOrWhiteSpace(MixName))
            {
                ErrorMessage = "Please enter a name for this mix.";
                return;
            }
            
            // Validate ingredients
            if (_ingredients == null || _ingredients.Count == 0)
            {
                ErrorMessage = "Cannot save an empty mix. Please add at least one ingredient.";
                return;
            }
            
            // Create a new mix from the ingredients
            var mix = new FertilizerMix
            {
                Name = MixName,
                Ingredients = new List<FertilizerQuantity>(_ingredients)
            };
            
            // Load existing mixes
            var existingMixes = await _fileService.LoadMixesAsync();
            if (existingMixes == null)
            {
                existingMixes = new ObservableCollection<FertilizerMix>();
            }
            
            // Check if a mix with this name already exists
            var existingMix = existingMixes.FirstOrDefault(m => m.Name.Equals(mix.Name, StringComparison.OrdinalIgnoreCase));
            if (existingMix != null)
            {
                // Ask user if they want to replace the existing mix
                bool replace = await DisplayAlertAsync(
                    "Mix Already Exists", 
                    $"A mix named '{mix.Name}' already exists. Do you want to replace it?", 
                    "Replace", "Cancel");
                
                if (!replace)
                {
                    IsSaving = false;
                    return;
                }
                
                // Replace the existing mix
                int index = existingMixes.IndexOf(existingMix);
                existingMixes[index] = mix;
            }
            else
            {
                // Add new mix
                existingMixes.Add(mix);
            }
            
            // Save all mixes to XML
            bool success = await _fileService.SaveMixesAsync(existingMixes);
            
            if (success)
            {
                SaveCompletedEvent?.Invoke(this, new SaveMixResult { Success = true, SavedMix = mix });
            }
            else
            {
                ErrorMessage = "Failed to save mix.";
                SaveCompletedEvent?.Invoke(this, new SaveMixResult { Success = false, ErrorMessage = "Failed to save mix." });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving mix: {ex.Message}";
            SaveCompletedEvent?.Invoke(this, new SaveMixResult { Success = false, ErrorMessage = ex.Message });
        }
        finally
        {
            IsSaving = false;
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class SaveMixResult : EventArgs
{
    public bool Success { get; set; }
    public FertilizerMix? SavedMix { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
