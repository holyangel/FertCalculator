using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FertCalculatorMaui;

public partial class ImportOptionsPage : ContentPage
{
    private ImportOptionsViewModel _viewModel;

    public ImportOptionsPage(ExportData importData, List<Fertilizer> existingFertilizers, ObservableCollection<FertilizerMix> existingMixes)
    {
        InitializeComponent();
        _viewModel = new ImportOptionsViewModel(importData, existingFertilizers, existingMixes);
        _viewModel.CancelCommand = new Command(async () => await Navigation.PopModalAsync());
        _viewModel.ImportCompleted += async (sender, result) => 
        {
            if (result.Success)
            {
                await Navigation.PopModalAsync();
            }
        };
        BindingContext = _viewModel;
    }

    // Event to provide results back to the caller
    public event EventHandler<ImportResult> ImportCompleted
    {
        add => _viewModel.ImportCompleted += value;
        remove => _viewModel.ImportCompleted -= value;
    }
}

public class ImportOptionsViewModel : INotifyPropertyChanged
{
    private ExportData _importData;
    private List<Fertilizer> _existingFertilizers;
    private ObservableCollection<FertilizerMix> _existingMixes;
    private bool _importFertilizers = true;
    private bool _importMixes = true;
    private bool _skipDuplicates = true;
    private bool _replaceDuplicates;
    private bool _renameDuplicates;
    private string _statusMessage;
    private string _statusColor = "Gray";
    private bool _hasStatus;

    public ImportOptionsViewModel(ExportData importData, List<Fertilizer> existingFertilizers, ObservableCollection<FertilizerMix> existingMixes)
    {
        _importData = importData;
        _existingFertilizers = existingFertilizers;
        _existingMixes = existingMixes;
        
        ImportCommand = new Command(ExecuteImport, CanExecuteImport);
        
        // Set initial status
        UpdateStatus($"Ready to import {_importData.Fertilizers.Count} fertilizer(s) and {_importData.Mixes.Count} mix(es).", "Gray");
    }

    // Event for reporting back import results
    public event EventHandler<ImportResult> ImportCompleted;

    // Properties for UI binding
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
                ((Command)ImportCommand).ChangeCanExecute();
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
                ((Command)ImportCommand).ChangeCanExecute();
            }
        }
    }

    public bool SkipDuplicates
    {
        get => _skipDuplicates;
        set
        {
            if (_skipDuplicates != value)
            {
                _skipDuplicates = value;
                OnPropertyChanged(nameof(SkipDuplicates));
            }
        }
    }

    public bool ReplaceDuplicates
    {
        get => _replaceDuplicates;
        set
        {
            if (_replaceDuplicates != value)
            {
                _replaceDuplicates = value;
                OnPropertyChanged(nameof(ReplaceDuplicates));
            }
        }
    }

    public bool RenameDuplicates
    {
        get => _renameDuplicates;
        set
        {
            if (_renameDuplicates != value)
            {
                _renameDuplicates = value;
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

    public bool CanImport => ImportFertilizers || ImportMixes;

    // Commands
    public ICommand ImportCommand { get; }
    public ICommand CancelCommand { get; set; }

    private bool CanExecuteImport() => CanImport;

    private void ExecuteImport()
    {
        try
        {
            var result = new ImportResult { Success = true };
            
            // Track counts for status reporting
            int fertilizersAdded = 0;
            int fertilizersSkipped = 0;
            int fertilizersReplaced = 0;
            int mixesAdded = 0;
            int mixesSkipped = 0;
            int mixesReplaced = 0;

            // Import fertilizers if selected
            if (ImportFertilizers)
            {
                foreach (var importFertilizer in _importData.Fertilizers)
                {
                    // Check if fertilizer with same name exists
                    var existingFertilizer = _existingFertilizers.FirstOrDefault(f => f.Name == importFertilizer.Name);
                    
                    if (existingFertilizer != null)
                    {
                        // Handle duplicates according to selected option
                        if (SkipDuplicates)
                        {
                            fertilizersSkipped++;
                            continue;
                        }
                        else if (ReplaceDuplicates)
                        {
                            // Remove existing fertilizer
                            _existingFertilizers.Remove(existingFertilizer);
                            fertilizersReplaced++;
                        }
                        else if (RenameDuplicates)
                        {
                            // Rename the imported fertilizer
                            int counter = 1;
                            string newName;
                            do
                            {
                                newName = $"{importFertilizer.Name} ({counter})";
                                counter++;
                            } while (_existingFertilizers.Any(f => f.Name == newName));
                            
                            importFertilizer.Name = newName;
                        }
                    }
                    
                    // Add the fertilizer
                    _existingFertilizers.Add(importFertilizer);
                    fertilizersAdded++;
                    result.ImportedFertilizers.Add(importFertilizer);
                }
            }

            // Import mixes if selected
            if (ImportMixes)
            {
                foreach (var importMix in _importData.Mixes)
                {
                    // Verify all ingredients exist
                    bool allIngredientsExist = importMix.Ingredients.All(ingredient => 
                        _existingFertilizers.Any(f => f.Name == ingredient.FertilizerName));
                    
                    if (!allIngredientsExist)
                    {
                        mixesSkipped++;
                        continue; // Skip mixes with missing ingredients
                    }

                    // Check if mix with same name exists
                    var existingMix = _existingMixes.FirstOrDefault(m => m.Name == importMix.Name);
                    
                    if (existingMix != null)
                    {
                        // Handle duplicates according to selected option
                        if (SkipDuplicates)
                        {
                            mixesSkipped++;
                            continue;
                        }
                        else if (ReplaceDuplicates)
                        {
                            // Remove existing mix
                            _existingMixes.Remove(existingMix);
                            mixesReplaced++;
                        }
                        else if (RenameDuplicates)
                        {
                            // Rename the imported mix
                            int counter = 1;
                            string newName;
                            do
                            {
                                newName = $"{importMix.Name} ({counter})";
                                counter++;
                            } while (_existingMixes.Any(m => m.Name == newName));
                            
                            importMix.Name = newName;
                        }
                    }
                    
                    // Add the mix
                    _existingMixes.Add(importMix);
                    mixesAdded++;
                    result.ImportedMixes.Add(importMix);
                }
            }

            // Sort fertilizers alphabetically
            List<Fertilizer> sortedFertilizers = _existingFertilizers.OrderBy(f => f.Name).ToList();
            _existingFertilizers.Clear();
            foreach (var fertilizer in sortedFertilizers)
            {
                _existingFertilizers.Add(fertilizer);
            }

            // Update result counts
            result.FertilizersAdded = fertilizersAdded;
            result.FertilizersSkipped = fertilizersSkipped;
            result.FertilizersReplaced = fertilizersReplaced;
            result.MixesAdded = mixesAdded;
            result.MixesSkipped = mixesSkipped;
            result.MixesReplaced = mixesReplaced;

            // Show success status
            UpdateStatus($"Import completed successfully!", "Green");

            // Notify that import is completed
            ImportCompleted?.Invoke(this, result);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Import failed: {ex.Message}", "Red");
            ImportCompleted?.Invoke(this, new ImportResult { Success = false, ErrorMessage = ex.Message });
        }
    }

    private void UpdateStatus(string message, string color)
    {
        StatusMessage = message;
        StatusColor = color;
        HasStatus = !string.IsNullOrEmpty(message);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ImportResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public List<Fertilizer> ImportedFertilizers { get; set; } = new List<Fertilizer>();
    public List<FertilizerMix> ImportedMixes { get; set; } = new List<FertilizerMix>();
    public int FertilizersAdded { get; set; }
    public int FertilizersSkipped { get; set; }
    public int FertilizersReplaced { get; set; }
    public int MixesAdded { get; set; }
    public int MixesSkipped { get; set; }
    public int MixesReplaced { get; set; }
}
