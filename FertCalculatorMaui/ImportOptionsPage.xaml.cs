using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

public partial class ImportOptionsPage : ContentPage
{
    private ImportOptionsViewModel _viewModel;
    private readonly FileService _fileService;
    private readonly ObservableCollection<Fertilizer> _availableFertilizers;
    private readonly ObservableCollection<FertilizerMix> _savedMixes;
    
    // Static readonly collections for file picker types - XML only
    private static readonly Dictionary<DevicePlatform, IEnumerable<string>> _filePickerTypes = new()
    {
        { DevicePlatform.Android, new[] { "application/xml", "*/*" } },
        { DevicePlatform.iOS, new[] { "public.xml", "public.data" } },
        { DevicePlatform.WinUI, new[] { ".xml" } },
        { DevicePlatform.MacCatalyst, new[] { "public.xml", "public.data" } }
    };

    public ImportOptionsPage(FileService fileService, ObservableCollection<Fertilizer> fertilizers, ObservableCollection<FertilizerMix> mixes)
    {
        InitializeComponent();
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _availableFertilizers = fertilizers ?? new ObservableCollection<Fertilizer>();
        _savedMixes = mixes ?? new ObservableCollection<FertilizerMix>();
        
        // Initialize the view model
        _viewModel = new ImportOptionsViewModel()
        {
            FileService = _fileService,
            ImportCommand = new Command(async () => await PickAndImportFileAsync()),
            CancelCommand = new Command(async () => await Navigation.PopAsync())
        };
        
        BindingContext = _viewModel;
    }

    private async Task PickAndImportFileAsync()
    {
        try
        {
            // Validate that at least one option is selected
            if (!_viewModel.ImportFertilizers && !_viewModel.ImportMixes)
            {
                await DisplayAlert("Error", "Please select at least one option to import.", "OK");
                return;
            }
            
            // Request storage permissions first
            var status = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Required", "Storage permission is required to import files.", "OK");
                return;
            }
            
            UpdateStatus("Selecting file...", "Blue");
            
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
                        UpdateStatus("Loading file...", "Blue");
                        
                        // Import from XML
                        using var stream = await result.OpenReadAsync();
                        var importResult = await _fileService.ImportDataAsync(stream);
                        
                        if (importResult.Success && importResult.ImportedData != null)
                        {
                            UpdateStatus("File loaded successfully", "Green");
                            
                            // Process the import with the selected options
                            await ProcessImportAsync(importResult.ImportedData);
                        }
                        else
                        {
                            string errorMessage = !string.IsNullOrEmpty(importResult.Error) 
                                ? $"Failed to load import data: {importResult.Error}" 
                                : "Failed to load import data from the selected file.";
                            
                            await DisplayAlert("Error", errorMessage, "OK");
                            UpdateStatus("Import failed", "Red");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Unsupported file format. Please select an XML file.", "OK");
                        UpdateStatus("Unsupported file format", "Red");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred while importing: {ex.Message}", "OK");
                    UpdateStatus("Import failed", "Red");
                }
            }
            else
            {
                // User canceled the file picking
                UpdateStatus("File selection canceled", "Gray");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred while picking a file: {ex.Message}", "OK");
            UpdateStatus("Import failed", "Red");
        }
    }
    
    private async Task ProcessImportAsync(ExportData importData)
    {
        try
        {
            UpdateStatus("Processing import...", "Blue");
            _viewModel.IsBusy = true;
            
            List<Fertilizer> fertilizersToSave = new List<Fertilizer>(_availableFertilizers);
            List<FertilizerMix> mixesToSave = new List<FertilizerMix>(_savedMixes);
            
            int importedFertilizersCount = 0;
            int importedMixesCount = 0;
            
            // Import fertilizers if selected
            if (_viewModel.ImportFertilizers && importData.Fertilizers.Count > 0)
            {
                UpdateStatus("Importing fertilizers...", "Blue");
                
                // Find new fertilizers (ones that don't exist in the current list)
                foreach (var fertilizer in importData.Fertilizers)
                {
                    // Check if fertilizer already exists (by name)
                    bool exists = fertilizersToSave.Any(f => f.Name.Equals(fertilizer.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (!exists)
                    {
                        // Add new fertilizer
                        fertilizersToSave.Add(fertilizer);
                        importedFertilizersCount++;
                    }
                    else
                    {
                        switch (_viewModel.DuplicateHandling)
                        {
                            case DuplicateHandlingOption.Skip:
                                break;
                            case DuplicateHandlingOption.Replace:
                                // Replace existing fertilizer
                                var index = fertilizersToSave.FindIndex(f => f.Name.Equals(fertilizer.Name, StringComparison.OrdinalIgnoreCase));
                                if (index >= 0)
                                {
                                    fertilizersToSave[index] = fertilizer;
                                    importedFertilizersCount++;
                                }
                                break;
                            case DuplicateHandlingOption.Rename:
                                // Rename and add fertilizer
                                var newName = GetUniqueNameForFertilizer(fertilizer.Name, fertilizersToSave);
                                var renamedFertilizer = new Fertilizer
                                {
                                    Name = newName,
                                    NitrogenPercent = fertilizer.NitrogenPercent,
                                    PhosphorusPercent = fertilizer.PhosphorusPercent,
                                    PotassiumPercent = fertilizer.PotassiumPercent,
                                    CalciumPercent = fertilizer.CalciumPercent,
                                    MagnesiumPercent = fertilizer.MagnesiumPercent,
                                    SulfurPercent = fertilizer.SulfurPercent,
                                    BoronPercent = fertilizer.BoronPercent,
                                    CopperPercent = fertilizer.CopperPercent,
                                    IronPercent = fertilizer.IronPercent,
                                    ManganesePercent = fertilizer.ManganesePercent,
                                    MolybdenumPercent = fertilizer.MolybdenumPercent,
                                    ZincPercent = fertilizer.ZincPercent,
                                    ChlorinePercent = fertilizer.ChlorinePercent,
                                    SilicaPercent = fertilizer.SilicaPercent,
                                    HumicAcidPercent = fertilizer.HumicAcidPercent,
                                    FulvicAcidPercent = fertilizer.FulvicAcidPercent,
                                    IsPhosphorusInOxideForm = fertilizer.IsPhosphorusInOxideForm,
                                    IsPotassiumInOxideForm = fertilizer.IsPotassiumInOxideForm,
                                    OriginalPhosphorusValue = fertilizer.OriginalPhosphorusValue,
                                    OriginalPotassiumValue = fertilizer.OriginalPotassiumValue
                                };
                                fertilizersToSave.Add(renamedFertilizer);
                                importedFertilizersCount++;
                                break;
                        }
                    }
                }
                
                // Sort fertilizers alphabetically
                fertilizersToSave = fertilizersToSave.OrderBy(f => f.Name).ToList();
                
                // Save updated fertilizers list
                bool fertilizersSaved = await _fileService.SaveFertilizersAsync(fertilizersToSave);
                if (!fertilizersSaved)
                {
                    throw new Exception("Failed to save imported fertilizers.");
                }
            }
            
            // Import mixes if selected
            if (_viewModel.ImportMixes && importData.Mixes.Count > 0)
            {
                UpdateStatus("Importing mixes...", "Blue");
                
                // Add all fertilizers first to ensure mix ingredients exist
                if (!_viewModel.ImportFertilizers && importData.Fertilizers.Count > 0)
                {
                    // If fertilizers aren't selected for import, we still need to make sure
                    // any required fertilizers for mixes exist
                    foreach (var mix in importData.Mixes)
                    {
                        foreach (var ingredient in mix.Ingredients)
                        {
                            // Check if this ingredient fertilizer exists
                            bool fertilizerExists = fertilizersToSave.Any(f => 
                                f.Name.Equals(ingredient.FertilizerName, StringComparison.OrdinalIgnoreCase));
                                
                            if (!fertilizerExists)
                            {
                                // Try to find it in the import data
                                var requiredFertilizer = importData.Fertilizers.FirstOrDefault(f => 
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
                    await _fileService.SaveFertilizersAsync(fertilizersToSave);
                }
                
                // Process mixes based on duplicate handling strategy
                foreach (var mix in importData.Mixes)
                {
                    bool exists = mixesToSave.Any(m => m.Name.Equals(mix.Name, StringComparison.OrdinalIgnoreCase));
                    
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
                    
                    if (!allIngredientsExist)
                    {
                        UpdateStatus($"Skipped mix '{mix.Name}' because some ingredients are missing.", "Orange");
                        continue;
                    }
                    
                    if (!exists)
                    {
                        // Add new mix
                        mixesToSave.Add(mix);
                        importedMixesCount++;
                    }
                    else
                    {
                        switch (_viewModel.DuplicateHandling)
                        {
                            case DuplicateHandlingOption.Skip:
                                break;
                            case DuplicateHandlingOption.Replace:
                                // Replace existing mix
                                var index = mixesToSave.FindIndex(m => m.Name.Equals(mix.Name, StringComparison.OrdinalIgnoreCase));
                                if (index >= 0)
                                {
                                    mixesToSave[index] = mix;
                                    importedMixesCount++;
                                }
                                break;
                            case DuplicateHandlingOption.Rename:
                                // Rename and add mix
                                var newName = GetUniqueNameForMix(mix.Name, mixesToSave);
                                var renamedMix = new FertilizerMix
                                {
                                    Name = newName,
                                    Ingredients = mix.Ingredients.ToList()
                                };
                                mixesToSave.Add(renamedMix);
                                importedMixesCount++;
                                break;
                        }
                    }
                }
                
                // Save updated mixes list
                bool mixesSaved = await _fileService.SaveMixesAsync(new ObservableCollection<FertilizerMix>(mixesToSave));
                if (!mixesSaved)
                {
                    throw new Exception("Failed to save imported mixes.");
                }
            }
            
            // Update UI with results
            string result = $"Imported {importedFertilizersCount} fertilizer(s) and {importedMixesCount} mix(es).";
            UpdateStatus(result, "Green");
            
            // Show success message and close page if anything was imported
            if (importedFertilizersCount > 0 || importedMixesCount > 0)
            {
                await DisplayAlert("Import Complete", result, "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Import Complete", "No new items were imported.", "OK");
            }
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = $"Import failed: {ex.Message}";
            UpdateStatus("Import failed.", "Red");
            await DisplayAlert("Error", $"Import failed: {ex.Message}", "OK");
        }
        finally
        {
            _viewModel.IsBusy = false;
        }
    }
    
    private string GetUniqueNameForFertilizer(string baseName, List<Fertilizer> existingFertilizers)
    {
        string newName = baseName;
        int counter = 1;
        
        while (existingFertilizers.Any(f => f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            newName = $"{baseName} ({counter})";
            counter++;
        }
        
        return newName;
    }
    
    private string GetUniqueNameForMix(string baseName, List<FertilizerMix> existingMixes)
    {
        string newName = baseName;
        int counter = 1;
        
        while (existingMixes.Any(m => m.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            newName = $"{baseName} ({counter})";
            counter++;
        }
        
        return newName;
    }
    
    private void UpdateStatus(string message, string color)
    {
        _viewModel.StatusMessage = message;
        _viewModel.StatusColor = color;
        _viewModel.HasStatus = !string.IsNullOrEmpty(message);
    }
}

public class ImportOptionsViewModel : INotifyPropertyChanged
{
    private bool _importFertilizers = true;
    private bool _importMixes = true;
    private DuplicateHandlingOption _duplicateHandling = DuplicateHandlingOption.Skip;
    private string _statusMessage = string.Empty;
    private string _statusColor = "Gray";
    private bool _hasStatus;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public ImportOptionsViewModel()
    {
        // Initialize with default values
    }
    
    public FileService FileService { get; set; }

    public bool ImportFertilizers
    {
        get => _importFertilizers;
        set
        {
            if (_importFertilizers != value)
            {
                _importFertilizers = value;
                OnPropertyChanged(nameof(ImportFertilizers));
                OnPropertyChanged(nameof(CanImport));
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
                OnPropertyChanged(nameof(CanImport));
            }
        }
    }
    
    public DuplicateHandlingOption DuplicateHandling
    {
        get => _duplicateHandling;
        set
        {
            if (_duplicateHandling != value)
            {
                _duplicateHandling = value;
                OnPropertyChanged(nameof(DuplicateHandling));
                OnPropertyChanged(nameof(SkipDuplicates));
                OnPropertyChanged(nameof(ReplaceDuplicates));
                OnPropertyChanged(nameof(RenameDuplicates));
            }
        }
    }
    
    // These properties are for backward compatibility with existing code
    public bool SkipDuplicates
    {
        get => _duplicateHandling == DuplicateHandlingOption.Skip;
        set
        {
            if (value && _duplicateHandling != DuplicateHandlingOption.Skip)
            {
                _duplicateHandling = DuplicateHandlingOption.Skip;
                OnPropertyChanged(nameof(DuplicateHandling));
                OnPropertyChanged(nameof(SkipDuplicates));
                OnPropertyChanged(nameof(ReplaceDuplicates));
                OnPropertyChanged(nameof(RenameDuplicates));
            }
        }
    }
    
    public bool ReplaceDuplicates
    {
        get => _duplicateHandling == DuplicateHandlingOption.Replace;
        set
        {
            if (value && _duplicateHandling != DuplicateHandlingOption.Replace)
            {
                _duplicateHandling = DuplicateHandlingOption.Replace;
                OnPropertyChanged(nameof(DuplicateHandling));
                OnPropertyChanged(nameof(SkipDuplicates));
                OnPropertyChanged(nameof(ReplaceDuplicates));
                OnPropertyChanged(nameof(RenameDuplicates));
            }
        }
    }
    
    public bool RenameDuplicates
    {
        get => _duplicateHandling == DuplicateHandlingOption.Rename;
        set
        {
            if (value && _duplicateHandling != DuplicateHandlingOption.Rename)
            {
                _duplicateHandling = DuplicateHandlingOption.Rename;
                OnPropertyChanged(nameof(DuplicateHandling));
                OnPropertyChanged(nameof(SkipDuplicates));
                OnPropertyChanged(nameof(ReplaceDuplicates));
                OnPropertyChanged(nameof(RenameDuplicates));
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
                OnPropertyChanged(nameof(CanImport));
            }
        }
    }
    
    public bool CanImport => (ImportFertilizers || ImportMixes) && !IsBusy;

    public ICommand ImportCommand { get; set; }
    public ICommand CancelCommand { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum DuplicateHandlingOption
{
    Skip,
    Replace,
    Rename
}
