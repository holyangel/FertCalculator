using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FertCalculatorMaui.Models;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui.ViewModels
{
    public partial class SaveMixViewModel : ObservableObject
    {
        private readonly FileService _fileService;
        private readonly List<FertilizerQuantity> _ingredients;
        
        [ObservableProperty]
        private string _mixName = string.Empty;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private bool _isSaving;
        
        [ObservableProperty]
        private ObservableCollection<FertilizerMix> _existingMixes = new();
        
        [ObservableProperty]
        private ObservableCollection<string> _existingMixNames = new();
        
        [ObservableProperty]
        private FertilizerMix _selectedMix;
        
        [ObservableProperty]
        private string _selectedMixName;
        
        [ObservableProperty]
        private bool _overwriteExisting;
        
        [ObservableProperty]
        private bool _hasExistingMixes;
        
        public delegate Task<bool> DisplayAlertDelegate(string title, string message, string accept, string cancel);
        public DisplayAlertDelegate DisplayAlertAsync { get; set; }
        
        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
        public bool IsOverwriteVisible => SelectedMix != null;
        
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; set; }
        
        public event EventHandler<SaveMixResult> SaveCompletedEvent;
        
        public SaveMixViewModel(FileService fileService, List<FertilizerQuantity> ingredients, ObservableCollection<FertilizerMix> existingMixes = null)
        {
            _fileService = fileService;
            _ingredients = ingredients;
            SaveCommand = new Command(ExecuteSave, CanExecuteSave);
            
            // If we already have mixes from MainPage, use them instead of loading from file
            if (existingMixes != null && existingMixes.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Using {existingMixes.Count} pre-loaded mixes");
                ExistingMixes = existingMixes;
                UpdateMixNames();
            }
        }
        
        private void UpdateMixNames()
        {
            ExistingMixNames = new ObservableCollection<string>(
                ExistingMixes.Select(m => m.Name).ToList()
            );
            
            HasExistingMixes = ExistingMixes.Count > 0;
            System.Diagnostics.Debug.WriteLine($"HasExistingMixes set to {HasExistingMixes}, ExistingMixNames count: {ExistingMixNames.Count}");
        }
        
        partial void OnMixNameChanged(string value)
        {
            ((Command)SaveCommand).ChangeCanExecute();
        }
        
        partial void OnIsSavingChanged(bool value)
        {
            ((Command)SaveCommand).ChangeCanExecute();
        }
        
        partial void OnExistingMixesChanged(ObservableCollection<FertilizerMix> value)
        {
            HasExistingMixes = value != null && value.Count > 0;
            System.Diagnostics.Debug.WriteLine($"ExistingMixes changed: HasExistingMixes = {HasExistingMixes}, Count = {value?.Count ?? 0}");
        }
        
        partial void OnSelectedMixNameChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Find the mix with the given name
                SelectedMix = ExistingMixes.FirstOrDefault(m => m.Name == value);
                System.Diagnostics.Debug.WriteLine($"Selected mix: {value}, Found: {SelectedMix != null}");
            }
            else
            {
                SelectedMix = null;
            }
        }
        
        partial void OnSelectedMixChanged(FertilizerMix value)
        {
            // Auto-populate the mix name when selecting a mix
            if (value != null)
            {
                MixName = value.Name;
                OverwriteExisting = true;
                System.Diagnostics.Debug.WriteLine($"SelectedMix changed: {value.Name}");
            }
            
            OnPropertyChanged(nameof(IsOverwriteVisible));
        }
        
        public async Task LoadExistingMixesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading existing mixes from file...");
                var mixes = await _fileService.LoadMixesAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {mixes.Count} mixes from file");
                
                ExistingMixes = new ObservableCollection<FertilizerMix>(mixes);
                UpdateMixNames();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load existing mixes: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading mixes: {ex}");
            }
        }
        
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
                
                // Check if we're overwriting an existing mix or creating a new one
                FertilizerMix mixToSave;
                bool isNewMix = true;
                
                if (SelectedMix != null && OverwriteExisting)
                {
                    // Overwrite existing mix
                    mixToSave = SelectedMix;
                    mixToSave.Ingredients.Clear();
                    isNewMix = false;
                    System.Diagnostics.Debug.WriteLine($"Overwriting existing mix: {mixToSave.Name}");
                }
                else
                {
                    // Create a new mix
                    mixToSave = new FertilizerMix
                    {
                        Name = MixName,
                        Ingredients = new List<FertilizerQuantity>()
                    };
                    System.Diagnostics.Debug.WriteLine($"Creating new mix: {mixToSave.Name}");
                }
                
                // Add all the ingredients to the mix
                foreach (var ingredient in _ingredients)
                {
                    mixToSave.Ingredients.Add(new FertilizerQuantity
                    {
                        FertilizerName = ingredient.FertilizerName,
                        Quantity = ingredient.Quantity
                    });
                }
                
                // Load current mixes
                var mixes = await _fileService.LoadMixesAsync();
                
                // If this is a new mix, add it to the collection
                if (isNewMix)
                {
                    mixes.Add(mixToSave);
                    System.Diagnostics.Debug.WriteLine($"Added new mix to collection");
                }
                else
                {
                    // If we're overwriting, we need to update by name since there's no Id property
                    var existingMixIndex = mixes.IndexOf(mixes.FirstOrDefault(m => m.Name == mixToSave.Name));
                    if (existingMixIndex >= 0)
                    {
                        mixes[existingMixIndex] = mixToSave;
                        System.Diagnostics.Debug.WriteLine($"Updated existing mix at index {existingMixIndex}");
                    }
                    else
                    {
                        // Mix wasn't found, add it
                        mixes.Add(mixToSave);
                        System.Diagnostics.Debug.WriteLine($"Mix not found for update, adding as new");
                    }
                }
                
                // Save the mixes
                bool success = await _fileService.SaveMixesAsync(mixes);
                System.Diagnostics.Debug.WriteLine($"Save result: {success}");
                
                SaveCompletedEvent?.Invoke(this, new SaveMixResult
                {
                    Success = success,
                    MixName = mixToSave.Name,
                    ErrorMessage = success ? null : "Failed to save mix to file"
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving mix: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error saving mix: {ex}");
                SaveCompletedEvent?.Invoke(this, new SaveMixResult
                {
                    Success = false,
                    ErrorMessage = ErrorMessage
                });
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}
