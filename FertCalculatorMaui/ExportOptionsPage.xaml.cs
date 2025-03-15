using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using System.Xml.Serialization;

namespace FertCalculatorMaui;

public partial class ExportOptionsPage : ContentPage
{
    private ExportOptionsViewModel _viewModel;

    public ExportOptionsPage(List<Fertilizer> fertilizers, ObservableCollection<FertilizerMix> mixes)
    {
        InitializeComponent();
        _viewModel = new ExportOptionsViewModel(fertilizers, mixes);
        _viewModel.CancelCommand = new Command(async () => await Navigation.PopModalAsync());
        _viewModel.ExportCompleted += async (sender, result) => 
        {
            if (result.Success)
            {
                await Navigation.PopModalAsync();
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
    private List<Fertilizer> _fertilizers;
    private ObservableCollection<FertilizerMix> _mixes;
    private bool _exportFertilizers = true;
    private bool _exportMixes = true;
    private bool _exportToXml = true;
    private bool _exportToJson;
    private string _fileName = "FertilizerCalculatorExport";
    private string _statusMessage;
    private string _statusColor = "Gray";
    private bool _hasStatus;

    public ExportOptionsViewModel(List<Fertilizer> fertilizers, ObservableCollection<FertilizerMix> mixes)
    {
        _fertilizers = fertilizers;
        _mixes = mixes;
        
        ExportCommand = new Command(ExecuteExport, CanExecuteExport);
        
        // Set initial status
        UpdateStatus($"Ready to export {_fertilizers.Count} fertilizer(s) and {_mixes.Count} mix(es).", "Gray");
    }

    // Event for reporting back export results
    public event EventHandler<ExportResult> ExportCompleted;

    // Properties for UI binding
    public bool ExportFertilizers
    {
        get => _exportFertilizers;
        set
        {
            if (_exportFertilizers != value)
            {
                _exportFertilizers = value;
                OnPropertyChanged(nameof(ExportFertilizers));
                OnPropertyChanged(nameof(CanExport));
                ((Command)ExportCommand).ChangeCanExecute();
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
                OnPropertyChanged(nameof(CanExport));
                ((Command)ExportCommand).ChangeCanExecute();
            }
        }
    }

    public bool ExportToXml
    {
        get => _exportToXml;
        set
        {
            if (_exportToXml != value)
            {
                _exportToXml = value;
                OnPropertyChanged(nameof(ExportToXml));
            }
        }
    }

    public bool ExportToJson
    {
        get => _exportToJson;
        set
        {
            if (_exportToJson != value)
            {
                _exportToJson = value;
                OnPropertyChanged(nameof(ExportToJson));
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
                OnPropertyChanged(nameof(CanExport));
                ((Command)ExportCommand).ChangeCanExecute();
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

    public bool CanExport => (ExportFertilizers || ExportMixes) && !string.IsNullOrWhiteSpace(FileName);

    // Commands
    public ICommand ExportCommand { get; }
    public ICommand CancelCommand { get; set; }

    private bool CanExecuteExport() => CanExport;

    private async void ExecuteExport()
    {
        try
        {
            var result = new ExportResult { Success = true };
            
            // Prepare the export data
            var exportData = new ExportData();
            
            if (ExportFertilizers)
            {
                exportData.Fertilizers.AddRange(_fertilizers);
            }
            
            if (ExportMixes)
            {
                exportData.Mixes.AddRange(_mixes);
            }

            // Determine file path and extension
            string extension = ExportToXml ? ".xml" : ".json";
            string fileName = $"{FileName}{extension}";
            
            // Get the app's documents directory
            string appDataDir = FileSystem.AppDataDirectory;
            string filePath = Path.Combine(appDataDir, fileName);

            // Save the file
            if (ExportToXml)
            {
                // Export to XML
                using var stream = File.Create(filePath);
                var serializer = new XmlSerializer(typeof(ExportData));
                serializer.Serialize(stream, exportData);
            }
            else
            {
                // Export to JSON
                string jsonData = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonData);
            }

            // For sharing the file
            try
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Export Fertilizer Data",
                    File = new ShareFile(filePath)
                });
                
                result.FilePath = filePath;
                UpdateStatus($"Export completed successfully! File saved to {fileName}", "Green");
            }
            catch (Exception ex)
            {
                // If sharing fails, just notify about file location
                result.FilePath = filePath;
                UpdateStatus($"Export completed. File saved to: {filePath}", "Green");
            }

            // Notify that export is completed
            ExportCompleted?.Invoke(this, result);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Export failed: {ex.Message}", "Red");
            ExportCompleted?.Invoke(this, new ExportResult { Success = false, ErrorMessage = ex.Message });
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

public class ExportResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public string FilePath { get; set; }
}
