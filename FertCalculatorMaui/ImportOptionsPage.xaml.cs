using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

public partial class ImportOptionsPage : ContentPage
{
    private ImportOptionsViewModel? _viewModel;
    private readonly FileService _fileService;
    private readonly ObservableCollection<Fertilizer> _availableFertilizers;
    private readonly ObservableCollection<FertilizerMix> _savedMixes;
    
    // Static readonly collections for file picker types - XML only
    private static readonly Dictionary<DevicePlatform, IEnumerable<string>> _filePickerTypes = new()
    {
        { DevicePlatform.Android, new[] { "application/xml" } },
        { DevicePlatform.iOS, new[] { "public.xml" } },
        { DevicePlatform.WinUI, new[] { ".xml" } },
        { DevicePlatform.MacCatalyst, new[] { "public.xml" } }
    };

    public ImportOptionsPage(FileService fileService, ObservableCollection<Fertilizer> fertilizers, ObservableCollection<FertilizerMix> mixes)
    {
        InitializeComponent();
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _availableFertilizers = fertilizers ?? new ObservableCollection<Fertilizer>();
        _savedMixes = mixes ?? new ObservableCollection<FertilizerMix>();
        
        // Show file picker to select the import file
        _ = PickAndImportFileAsync();
    }

    private async Task PickAndImportFileAsync()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = new FilePickerFileType(_filePickerTypes),
                PickerTitle = "Select an XML file to import"
            });

            if (result != null)
            {
                string extension = Path.GetExtension(result.FileName).ToLowerInvariant();
                
                try
                {
                    // Only handle XML files
                    if (extension == ".xml")
                    {
                        // Import from XML
                        using var stream = await result.OpenReadAsync();
                        var importResult = await _fileService.ImportDataAsync(stream);
                        
                        if (importResult.Success && importResult.ImportedData != null)
                        {
                            // Create the view model with the loaded data
                            _viewModel = new ImportOptionsViewModel(importResult.ImportedData, _availableFertilizers.ToList(), _savedMixes.ToList())
                            {
                                FileService = _fileService,
                                CancelCommand = new Command(async () => await Navigation.PopAsync())
                            };
                            
                            _viewModel.ImportCompleted += async (s, e) => 
                            {
                                if (e.Success)
                                {
                                    await Navigation.PopAsync();
                                }
                            };
                            BindingContext = _viewModel;
                        }
                        else
                        {
                            string errorMessage = !string.IsNullOrEmpty(importResult.Error) 
                                ? $"Failed to load import data: {importResult.Error}" 
                                : "Failed to load import data from the selected file.";
                            
                            await DisplayAlert("Error", errorMessage, "OK");
                            await Navigation.PopAsync();
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Unsupported file format. Please select an XML file.", "OK");
                        await Navigation.PopAsync();
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred while importing: {ex.Message}", "OK");
                    await Navigation.PopAsync();
                }
            }
            else
            {
                // User canceled the file picking
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred while picking a file: {ex.Message}", "OK");
            await Navigation.PopAsync();
        }
    }
}

public class ImportOptionsViewModel : INotifyPropertyChanged
{
    private readonly ExportData _importData;
    private readonly List<Fertilizer> _existingFertilizers;
    private readonly List<FertilizerMix> _existingMixes;
    private bool _importFertilizers = true;
    private bool _importMixes = true;
    private string _statusMessage = string.Empty;
    private string _statusColor = "Gray";
    private bool _hasStatus;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public ImportOptionsViewModel(ExportData importData, List<Fertilizer> existingFertilizers, List<FertilizerMix> existingMixes)
    {
        _importData = importData ?? throw new ArgumentNullException(nameof(importData));
        _existingFertilizers = existingFertilizers ?? new List<Fertilizer>();
        _existingMixes = existingMixes ?? new List<FertilizerMix>();
        
        ImportCommand = new Command(ExecuteImport);
        
        // Count available data
        int newFertilizersCount = CountNewFertilizers();
        int newMixesCount = CountNewMixes();
        
        // Set initial status
        UpdateStatus($"Found {newFertilizersCount} new fertilizer(s) and {newMixesCount} new mix(es) to import.", "Gray");
    }
    
    public FileService FileService { get; set; }
    
    // Event for reporting back import results
    public event EventHandler<ImportResult> ImportCompleted;

    public bool ImportFertilizers
    {
        get => _importFertilizers;
        set
        {
            if (_importFertilizers != value)
            {
                _importFertilizers = value;
                OnPropertyChanged(nameof(ImportFertilizers));
            }
        }
    }

    public bool ImportMixes
    {
        get => _importMixes;
        set
        {
            if (_importMixes != value)
            {
                _importMixes = value;
                OnPropertyChanged(nameof(ImportMixes));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    public string StatusColor
    {
        get => _statusColor;
        set
        {
            if (_statusColor != value)
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }
    }

    public bool HasStatus
    {
        get => _hasStatus;
        set
        {
            if (_hasStatus != value)
            {
                _hasStatus = value;
                OnPropertyChanged(nameof(HasStatus));
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
    
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }

    public ICommand ImportCommand { get; }
    public ICommand CancelCommand { get; set; }

    private async void ExecuteImport()
    {
        if (FileService == null)
        {
            ErrorMessage = "File service is not available.";
            return;
        }
        
        // Clear any previous errors
        ErrorMessage = string.Empty;
        IsBusy = true;
        
        try
        {
            // Validate that at least one option is selected
            if (!ImportFertilizers && !ImportMixes)
            {
                ErrorMessage = "Please select at least one option to import.";
                IsBusy = false;
                return;
            }
            
            List<Fertilizer> fertilizersToSave = new List<Fertilizer>(_existingFertilizers);
            List<FertilizerMix> mixesToSave = new List<FertilizerMix>(_existingMixes);
            
            int importedFertilizersCount = 0;
            int importedMixesCount = 0;
            
            // Import fertilizers if selected
            if (ImportFertilizers && _importData.Fertilizers.Count > 0)
            {
                UpdateStatus("Importing fertilizers...", "Blue");
                
                // Find new fertilizers (ones that don't exist in the current list)
                foreach (var fertilizer in _importData.Fertilizers)
                {
                    // Check if fertilizer already exists (by name)
                    if (!fertilizersToSave.Any(f => f.Name.Equals(fertilizer.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        fertilizersToSave.Add(fertilizer);
                        importedFertilizersCount++;
                    }
                }
                
                // Sort fertilizers alphabetically
                fertilizersToSave = fertilizersToSave.OrderBy(f => f.Name).ToList();
                
                // Save updated fertilizers list
                bool fertilizersSaved = await FileService.SaveFertilizersAsync(fertilizersToSave);
                if (!fertilizersSaved)
                {
                    throw new Exception("Failed to save imported fertilizers.");
                }
            }
            
            // Import mixes if selected
            if (ImportMixes && _importData.Mixes.Count > 0)
            {
                UpdateStatus("Importing mixes...", "Blue");
                
                // Add all fertilizers first to ensure mix ingredients exist
                if (!ImportFertilizers && _importData.Fertilizers.Count > 0)
                {
                    // If fertilizers aren't selected for import, we still need to make sure
                    // any required fertilizers for mixes exist
                    foreach (var mix in _importData.Mixes)
                    {
                        foreach (var ingredient in mix.Ingredients)
                        {
                            // Check if this ingredient fertilizer exists
                            bool fertilizerExists = fertilizersToSave.Any(f => 
                                f.Name.Equals(ingredient.FertilizerName, StringComparison.OrdinalIgnoreCase));
                                
                            if (!fertilizerExists)
                            {
                                // Try to find it in the import data
                                var requiredFertilizer = _importData.Fertilizers.FirstOrDefault(f => 
                                    f.Name.Equals(ingredient.FertilizerName, StringComparison.OrdinalIgnoreCase));
                                    
                                if (requiredFertilizer != null)
                                {
                                    // Add the required fertilizer
                                    fertilizersToSave.Add(requiredFertilizer);
                                }
                            }
                        }
                    }
                    
                    // Sort and save fertilizers if we added any required ones
                    fertilizersToSave = fertilizersToSave.OrderBy(f => f.Name).ToList();
                    await FileService.SaveFertilizersAsync(fertilizersToSave);
                }
                
                // Find new mixes (ones that don't exist in the current list)
                foreach (var mix in _importData.Mixes)
                {
                    // Check if this mix already exists (by name)
                    if (!mixesToSave.Any(m => m.Name.Equals(mix.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        // Verify that all ingredients exist in the available fertilizers
                        bool allIngredientsExist = true;
                        
                        foreach (var ingredient in mix.Ingredients)
                        {
                            if (!fertilizersToSave.Any(f => f.Name.Equals(ingredient.FertilizerName, StringComparison.OrdinalIgnoreCase)))
                            {
                                allIngredientsExist = false;
                                break;
                            }
                        }
                        
                        if (allIngredientsExist)
                        {
                            mixesToSave.Add(mix);
                            importedMixesCount++;
                        }
                        else
                        {
                            UpdateStatus($"Skipped mix '{mix.Name}' because some ingredients are missing.", "Orange");
                        }
                    }
                }
                
                // Save updated mixes list
                bool mixesSaved = await FileService.SaveMixesAsync(new ObservableCollection<FertilizerMix>(mixesToSave));
                if (!mixesSaved)
                {
                    throw new Exception("Failed to save imported mixes.");
                }
            }
            
            // Update status with results
            string result = $"Imported {importedFertilizersCount} fertilizer(s) and {importedMixesCount} mix(es).";
            UpdateStatus(result, "Green");
            
            // Notify of completion
            ImportCompleted?.Invoke(this, new ImportResult { 
                Success = true, 
                ImportedFertilizersCount = importedFertilizersCount,
                ImportedMixesCount = importedMixesCount 
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Import failed: {ex.Message}";
            UpdateStatus("Import failed.", "Red");
            ImportCompleted?.Invoke(this, new ImportResult { Success = false, Error = ex.Message });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private int CountNewFertilizers()
    {
        int count = 0;
        foreach (var fertilizer in _importData.Fertilizers)
        {
            if (!_existingFertilizers.Any(f => f.Name.Equals(fertilizer.Name, StringComparison.OrdinalIgnoreCase)))
            {
                count++;
            }
        }
        return count;
    }
    
    private int CountNewMixes()
    {
        int count = 0;
        foreach (var mix in _importData.Mixes)
        {
            if (!_existingMixes.Any(m => m.Name.Equals(mix.Name, StringComparison.OrdinalIgnoreCase)))
            {
                count++;
            }
        }
        return count;
    }

    private void UpdateStatus(string message, string color)
    {
        StatusMessage = message;
        StatusColor = color;
        HasStatus = !string.IsNullOrEmpty(message);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ImportResult : EventArgs
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
    public int ImportedFertilizersCount { get; set; }
    public int ImportedMixesCount { get; set; }
}
