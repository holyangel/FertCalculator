using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Serialization;
using System.IO;
#if ANDROID
using Android.Content;
using Android.Provider;
using Android.OS;
using AndroidX.Core.Content;
using JavaFile = Java.IO.File;
using JavaConsole = Java.IO.Console;
#endif
#if WINDOWS
using Microsoft.UI.Xaml;
using WinRT.Interop;
#endif

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
            System.Console.WriteLine($"Error loading fertilizers: {ex.Message}");
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
            System.Console.WriteLine($"Error saving fertilizers: {ex.Message}");
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
            System.Console.WriteLine($"Error loading mixes: {ex.Message}");
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
            System.Console.WriteLine($"Error saving mixes: {ex.Message}");
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
            System.Console.WriteLine($"Error loading XML file: {ex.Message}");
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
            System.Console.WriteLine($"Error deserializing XML: {ex.Message}");
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
            System.Console.WriteLine($"Error saving XML file: {ex.Message}");
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
            System.Console.WriteLine($"Error sharing file: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> SaveToUserSelectedDirectoryAsync<T>(T data, string suggestedFileName) where T : class
    {
        try
        {
            // Ensure the filename has the correct extension
            if (!suggestedFileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                suggestedFileName += ".xml";
            
            // Create a temporary file first
            string tempFilePath = Path.Combine(GetAppDataDirectory(), $"temp_{suggestedFileName}");
            
            // Serialize and save the data to the temp file
            using (FileStream stream = File.Create(tempFilePath))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, data);
            }
            
            // Now use the Share API to let the user save it where they want
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Save Fertilizer Calculator Data",
                File = new ShareFile(tempFilePath)
            });
            
            return true;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> SaveFileToDirectoryAsync(string sourceFilePath, string suggestedFileName)
    {
        try
        {
            if (!File.Exists(sourceFilePath))
                return false;
                
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // For Android, we need to use the platform-specific API
                return await SaveFileToAndroidDocumentsAsync(sourceFilePath, suggestedFileName);
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                // For Windows, we can use the FileSavePicker
                return await SaveFileToWindowsDocumentsAsync(sourceFilePath, suggestedFileName);
            }
            else
            {
                // For other platforms, use the Share API as fallback
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Save Fertilizer Calculator Data",
                    File = new ShareFile(sourceFilePath)
                });
                
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> SaveFileToAndroidDocumentsAsync(string sourceFilePath, string fileName)
    {
#if ANDROID
        try
        {
            // Request permissions first
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                return false;
            }
            
            // Get the Android context
            var context = Android.App.Application.Context;
            
            // Create a file in the Downloads directory
            var downloadsDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
            var destinationFile = new JavaFile(downloadsDir, fileName);
            
            // Copy the source file to the destination
            using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            using (var destinationStream = new FileStream(destinationFile.AbsolutePath, FileMode.Create, FileAccess.Write))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
            
            // Create a content intent to notify the system
            var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
            
            // Get the file URI using FileProvider
            var fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                context,
                context.PackageName + ".fileprovider",
                destinationFile);
                
            // Set the data and type
            intent.SetDataAndType(fileUri, "application/xml");
            intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
            
            // Show a toast notification
            Android.Widget.Toast.MakeText(
                context, 
                $"File saved to Downloads/{fileName}", 
                Android.Widget.ToastLength.Long).Show();
                
            return true;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error saving file to Android Documents: {ex.Message}");
            return false;
        }
#else
        // For non-Android platforms, just return false and let the caller handle it
        return false;
#endif
    }

    private async Task<bool> SaveFileToWindowsDocumentsAsync(string sourceFilePath, string fileName)
    {
#if WINDOWS
        try
        {
            // Use the Windows file save picker
            var fileSavePicker = new Windows.Storage.Pickers.FileSavePicker();
            
            // Initialize the picker with the window handle
            var window = new Microsoft.UI.Xaml.Window();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(fileSavePicker, hwnd);
            
            // Set properties
            fileSavePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            fileSavePicker.FileTypeChoices.Add("XML Document", new List<string>() { ".xml" });
            fileSavePicker.SuggestedFileName = fileName;
            
            // Show the picker and get the selected file
            var file = await fileSavePicker.PickSaveFileAsync();
            
            if (file != null)
            {
                // Copy the source file to the selected location
                using var sourceStream = File.OpenRead(sourceFilePath);
                using var destinationStream = await file.OpenStreamForWriteAsync();
                await sourceStream.CopyToAsync(destinationStream);
                
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error saving file to Windows Documents: {ex.Message}");
            return false;
        }
#else
        // For non-Windows platforms, just return false and let the caller handle it
        return false;
#endif
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
