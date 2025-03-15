using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Serialization;

namespace FertCalculatorMaui.Services;

public class FileService
{
    private const string FERTILIZERS_FILENAME = "Fertilizers.xml";
    private const string MIXES_FILENAME = "Mixes.xml";

    public async Task<List<Fertilizer>> LoadFertilizersAsync()
    {
        try
        {
            var fertilizers = await LoadFromXmlAsync<List<Fertilizer>>(FERTILIZERS_FILENAME);
            return fertilizers ?? new List<Fertilizer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading fertilizers: {ex.Message}");
            return new List<Fertilizer>();
        }
    }

    public async Task<bool> SaveFertilizersAsync(List<Fertilizer> fertilizers)
    {
        if (fertilizers == null)
            return false;

        try
        {
            return await SaveToXmlAsync(fertilizers, FERTILIZERS_FILENAME);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving fertilizers: {ex.Message}");
            return false;
        }
    }

    public async Task<ObservableCollection<FertilizerMix>> LoadMixesAsync()
    {
        try
        {
            var mixes = await LoadFromXmlAsync<List<FertilizerMix>>(MIXES_FILENAME);
            return mixes != null 
                ? new ObservableCollection<FertilizerMix>(mixes) 
                : new ObservableCollection<FertilizerMix>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading mixes: {ex.Message}");
            return new ObservableCollection<FertilizerMix>();
        }
    }

    public async Task<bool> SaveMixesAsync(ObservableCollection<FertilizerMix> mixes)
    {
        if (mixes == null)
            return false;

        try
        {
            return await SaveToXmlAsync(mixes.ToList(), MIXES_FILENAME);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving mixes: {ex.Message}");
            return false;
        }
    }

    public async Task<T?> LoadFromXmlAsync<T>(string fileName) where T : class
    {
        try
        {
            string filePath = Path.Combine(GetAppDataDirectory(), fileName);
            
            if (!File.Exists(filePath))
                return null;

            using FileStream stream = File.OpenRead(filePath);
            return await LoadFromXmlAsync<T>(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading XML file: {ex.Message}");
            return null;
        }
    }

    public async Task<T?> LoadFromXmlAsync<T>(Stream stream) where T : class
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));

            using StreamReader reader = new StreamReader(stream);
            var result = serializer.Deserialize(reader) as T;
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing XML: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SaveToXmlAsync<T>(T data, string fileName) where T : class
    {
        try
        {
            string filePath = Path.Combine(GetAppDataDirectory(), fileName);
            
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using FileStream stream = File.Create(filePath);
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, data);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving XML file: {ex.Message}");
            return false;
        }
    }

    public string GetAppDataDirectory()
    {
        return FileSystem.AppDataDirectory;
    }

    public async Task<bool> ShareFileAsync(string filePath, string title)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = title,
                File = new ShareFile(filePath)
            });
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sharing file: {ex.Message}");
            return false;
        }
    }

    public async Task<ExportResult> ImportDataAsync(Stream stream)
    {
        try
        {
            var importData = await LoadFromXmlAsync<ExportData>(stream);
            
            if (importData == null)
                return new ExportResult { Success = false, Error = "Failed to deserialize data" };

            // If import is successful, return the imported data
            return new ExportResult 
            { 
                Success = true, 
                ImportedData = importData 
            };
        }
        catch (Exception ex)
        {
            return new ExportResult { Success = false, Error = ex.Message };
        }
    }
}

public class ExportResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
    public ExportData? ImportedData { get; set; }
}

public class ExportData
{
    public List<Fertilizer> Fertilizers { get; set; } = new List<Fertilizer>();
    public List<FertilizerMix> Mixes { get; set; } = new List<FertilizerMix>();
}
