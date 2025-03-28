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

    public FileService()
    {
        // Initialize the app data directory with default files if needed
        InitializeDefaultFilesAsync().ConfigureAwait(false);
    }

    private async Task InitializeDefaultFilesAsync()
    {
        try
        {
            string fertilizersPath = Path.Combine(GetAppDataDirectory(), FERTILIZERS_FILENAME);
            
            // Check if Fertilizers.xml exists in the app data directory
            if (!File.Exists(fertilizersPath))
            {
                System.Console.WriteLine($"Fertilizers.xml not found in app data directory. Attempting to import from embedded resource.");
                
                // List all embedded resources to debug
                var resourceNames = GetType().Assembly.GetManifestResourceNames();
                foreach (var resourceName in resourceNames)
                {
                    System.Console.WriteLine($"Found embedded resource: {resourceName}");
                }
                
                // Get the embedded Fertilizers.xml resource
                Stream? resourceStream = GetType().Assembly.GetManifestResourceStream("FertCalculatorMaui.Fertilizers.xml");
                if (resourceStream != null)
                {
                    // Import the data using the existing import functionality
                    var importResult = await ImportDataAsync(resourceStream);
                    
                    if (importResult.Success && importResult.ImportedData != null)
                    {
                        System.Console.WriteLine($"Successfully imported default fertilizers from embedded resource");
                        
                        // Save the imported fertilizers
                        if (importResult.ImportedData.Fertilizers != null && importResult.ImportedData.Fertilizers.Count > 0)
                        {
                            await SaveFertilizersAsync(importResult.ImportedData.Fertilizers);
                            System.Console.WriteLine($"Saved {importResult.ImportedData.Fertilizers.Count} default fertilizers");
                        }
                        
                        // Save any imported mixes if they exist
                        if (importResult.ImportedData.Mixes != null && importResult.ImportedData.Mixes.Count > 0)
                        {
                            await SaveMixesAsync(new ObservableCollection<FertilizerMix>(importResult.ImportedData.Mixes));
                            System.Console.WriteLine($"Saved {importResult.ImportedData.Mixes.Count} default mixes");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine($"Failed to import default fertilizers from embedded resource");
                    }
                }
                else
                {
                    System.Console.WriteLine($"Embedded resource FertCalculatorMaui.Fertilizers.xml not found. Make sure it's properly configured in the project file.");
                }
            }
            else
            {
                System.Console.WriteLine($"Fertilizers.xml already exists at {fertilizersPath}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error initializing default files: {ex.Message}");
            System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

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

            // Use Task.Run to perform file operations on a background thread
            return await Task.Run(async () => 
            {
                using FileStream stream = File.OpenRead(filePath);
                return await LoadFromXmlAsync<T>(stream);
            });
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
            return await Task.Run(() => 
            {
                var serializer = new XmlSerializer(typeof(T));
                using StreamReader reader = new(stream);
                var result = serializer.Deserialize(reader) as T;
                return result;
            });
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
            
            // Ensure the directory exists
            string? directoryName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            
            // Use Task.Run to perform the serialization on a background thread
            return await Task.Run(() =>
            {
                try
                {
                    // Create a temporary file path
                    string tempFilePath = Path.Combine(GetAppDataDirectory(), $"temp_{Guid.NewGuid()}.xml");

                    // Serialize to the temporary file first
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                        serializer.Serialize(fileStream, data);
                    }

                    // If the target file exists, delete it
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    // Move the temporary file to the target file
                    File.Move(tempFilePath, filePath);

                    return true;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error in SaveToXmlAsync: {ex.Message}");
                    return false;
                }
            });
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error in SaveToXmlAsync: {ex.Message}");
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

#if IOS || MACCATALYST
            // Use Share API on iOS/macCatalyst which supports these features
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = title,
                File = new ShareFile(filePath)
            });
#else
            // For Android, Windows, and other platforms
            // Use a platform-agnostic approach or platform-specific implementations
#if ANDROID
            // Android-specific sharing implementation
            var context = Android.App.Application.Context;
            var file = new JavaFile(filePath);
            var uri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                context,
                context.PackageName + ".fileprovider",
                file);
                
            var intent = new Intent(Intent.ActionSend);
            intent.SetType("application/xml");
            intent.PutExtra(Intent.ExtraStream, uri);
            intent.PutExtra(Intent.ExtraText, title);
            intent.PutExtra(Intent.ExtraTitle, title);
            intent.AddFlags(ActivityFlags.NewTask);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);
            
            var chooserIntent = Intent.CreateChooser(intent, title);
            chooserIntent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(chooserIntent);
#elif WINDOWS
            // Windows-specific implementation
            var fileSavePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                SuggestedFileName = Path.GetFileName(filePath)
            };
            
            fileSavePicker.FileTypeChoices.Add("XML Document", new List<string>() { ".xml" });
            
            // Get the current window handle
            var window = Microsoft.Maui.Controls.Application.Current?.Windows[0];
            if (window != null)
            {
                // Associate the picker with the window
                var hwnd = WindowNative.GetWindowHandle(window);
                InitializeWithWindow.Initialize(fileSavePicker, hwnd);
                
                var file = await fileSavePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Copy the file to the selected location
                    await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath)
                        .AsTask()
                        .ContinueWith(async (t) => 
                        {
                            var sourceFile = await t;
                            await sourceFile.CopyAndReplaceAsync(file);
                        });
                }
            }
#else
            // Fallback for other platforms - just copy the file to a known location
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string destinationPath = Path.Combine(documentsPath, Path.GetFileName(filePath));
            File.Copy(filePath, destinationPath, true);
            
            // Show a message to the user about where the file was saved
            await Application.Current.MainPage.DisplayAlert(
                "File Saved", 
                $"The file has been saved to: {destinationPath}", 
                "OK");
#endif
#endif
            
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
            
            // Now use the platform-specific approach to let the user save it
            return await ShareFileAsync(tempFilePath, "Save Fertilizer Calculator Data");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error saving to user selected directory: {ex.Message}");
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

            // Create content values for the new file
            var contentValues = new Android.Content.ContentValues();
            contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
            contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "application/xml");
            contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads);

            // Get the content resolver
            var contentResolver = context.ContentResolver;

            // Insert the new file
            var uri = contentResolver.Insert(Android.Provider.MediaStore.Downloads.ExternalContentUri, contentValues);

            if (uri != null)
            {
                // Open output stream to the new file
                using (var outputStream = contentResolver.OpenOutputStream(uri))
                using (var inputStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                {
                    if (outputStream != null)
                    {
                        await inputStream.CopyToAsync(outputStream);

                        // Show a toast notification
                        Android.Widget.Toast.MakeText(
                            context,
                            $"File saved to Downloads/{fileName}",
                            Android.Widget.ToastLength.Long).Show();

                        return true;
                    }
                }
            }

            return false;
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
            
            // Get the current window handle
            var window = Microsoft.Maui.Controls.Application.Current.Windows[0].Handler.PlatformView;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(fileSavePicker, hwnd);
            
            // Set properties
            fileSavePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            fileSavePicker.FileTypeChoices.Add("XML Document", new[] { ".xml" });
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
