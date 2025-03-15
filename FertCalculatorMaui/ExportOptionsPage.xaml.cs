using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

public partial class ExportOptionsPage : ContentPage
{
    private ExportOptionsViewModel? _viewModel;

    public ExportOptionsPage(FileService fileService, List<Fertilizer> fertilizers, ObservableCollection<FertilizerMix> mixes)
    {
        InitializeComponent();
        _viewModel = new ExportOptionsViewModel(fileService, fertilizers, mixes)
        {
            CancelCommand = new Command(async () => await Navigation.PopAsync())
        };
        
        _viewModel.ExportCompleted += async (sender, e) => 
        {
            if (e.Success)
            {
                await Navigation.PopAsync();
            }
        };
        
        BindingContext = _viewModel;
    }
    
    // Event to provide results back to the caller
    public event EventHandler<ExportResult> ExportCompleted
    {
        add => _viewModel.ExportCompleted += value;
        remove => _viewModel.ExportCompleted -= value;
    }
}

public class ExportOptionsViewModel : INotifyPropertyChanged
{
    private readonly FileService _fileService;
    private readonly List<Fertilizer> _fertilizers;
    private readonly ObservableCollection<FertilizerMix> _mixes;
    private bool _exportFertilizers = true;
    private bool _exportMixes = true;
    private string _fileName = "FertCalculator_Export";
    private string _statusMessage = string.Empty;
    private string _statusColor = "Gray";
    private bool _hasStatus;
    private string _errorMessage = string.Empty;

    public ExportOptionsViewModel(FileService fileService, List<Fertilizer> fertilizers, ObservableCollection<FertilizerMix> mixes)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _fertilizers = fertilizers ?? new List<Fertilizer>();
        _mixes = mixes ?? new ObservableCollection<FertilizerMix>();
        
        ExportCommand = new Command(ExecuteExport);
        
        // Set initial status
        UpdateStatus($"Ready to export {_fertilizers.Count} fertilizer(s) and {_mixes.Count} mix(es) to XML.", "Gray");
    }
    
    // Event for reporting back export results
    public event EventHandler<ExportResult>? ExportCompleted;

    public bool ExportFertilizers
    {
        get => _exportFertilizers;
        set
        {
            if (_exportFertilizers != value)
            {
                _exportFertilizers = value;
                OnPropertyChanged(nameof(ExportFertilizers));
            }
        }
    }

    public bool ExportMixes
    {
        get => _exportMixes;
        set
        {
            if (_exportMixes != value)
            {
                _exportMixes = value;
                OnPropertyChanged(nameof(ExportMixes));
            }
        }
    }

    public string FileName
    {
        get => _fileName;
        set
        {
            if (_fileName != value)
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
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

    public ICommand ExportCommand { get; }
    public ICommand? CancelCommand { get; set; }

    private async void ExecuteExport()
    {
        // Clear any previous errors
        ErrorMessage = string.Empty;
        
        try
        {
            // Validate that at least one option is selected
            if (!ExportFertilizers && !ExportMixes)
            {
                ErrorMessage = "Please select at least one option to export.";
                return;
            }
            
            // Create export data
            var exportData = new Services.ExportData();
            
            if (ExportFertilizers)
            {
                exportData.Fertilizers.AddRange(_fertilizers);
                UpdateStatus("Preparing fertilizers for export...", "Blue");
            }
            
            if (ExportMixes)
            {
                exportData.Mixes.AddRange(_mixes);
                UpdateStatus("Preparing mixes for export...", "Blue");
            }
            
            // Ensure we have a valid filename
            if (string.IsNullOrWhiteSpace(FileName))
            {
                FileName = "FertCalculator_Export";
            }
            
            // Add .xml extension if not present
            string fileName = FileName;
            if (!fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".xml";
            }
            
            // Get temporary file path for export before sharing
            string tempFilePath = Path.Combine(_fileService.GetAppDataDirectory(), $"temp_{fileName}");
            
            // Save to XML
            UpdateStatus("Saving data to XML...", "Blue");
            bool saveSuccess = await _fileService.SaveToXmlAsync(exportData, Path.GetFileName(tempFilePath));
            
            if (!saveSuccess)
            {
                ErrorMessage = "Failed to save export data.";
                UpdateStatus("Export failed.", "Red");
                ExportCompleted?.Invoke(this, new ExportResult { Success = false, Error = "Failed to save export data." });
                return;
            }
            
            // Share the file
            UpdateStatus("Opening share dialog...", "Blue");
            bool shareSuccess = await _fileService.ShareFileAsync(tempFilePath, "Share Fertilizer Calculator Data");
            
            if (shareSuccess)
            {
                UpdateStatus("Export completed successfully!", "Green");
                ExportCompleted?.Invoke(this, new ExportResult { Success = true });
            }
            else
            {
                UpdateStatus("Export was canceled or failed.", "Red");
                ExportCompleted?.Invoke(this, new ExportResult { Success = false, Error = "Sharing was canceled or failed." });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
            UpdateStatus("Export failed.", "Red");
            ExportCompleted?.Invoke(this, new ExportResult { Success = false, Error = ex.Message });
        }
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

public class ExportResult : EventArgs
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
}
