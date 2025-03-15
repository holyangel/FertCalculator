using System.Diagnostics;
using System.Text.Json;
using System.Xml.Serialization;

namespace FertCalculatorMaui.Services
{
    public class FileService
    {
        private readonly string _appDataPath;

        public FileService()
        {
            _appDataPath = FileSystem.AppDataDirectory;
        }

        public string GetFilePath(string fileName)
        {
            return Path.Combine(_appDataPath, fileName);
        }

        public async Task<T?> LoadFromXmlAsync<T>(string fileName) where T : class
        {
            try
            {
                string filePath = GetFilePath(fileName);
                Debug.WriteLine($"Attempting to load file: {filePath}");

                if (!File.Exists(filePath))
                {
                    Debug.WriteLine($"File does not exist: {filePath}");
                    return null;
                }

                using var stream = File.OpenRead(filePath);
                var serializer = new XmlSerializer(typeof(T));
                return await Task.Run(() => (T)serializer.Deserialize(stream));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading file {fileName}: {ex.Message}\n{ex.StackTrace}");
                // Try to display an alert, but only if Shell.Current is available
                if (Shell.Current != null)
                {
                    try
                    {
                        await Shell.Current.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
                    }
                    catch (Exception)
                    {
                        // Ignore if we can't show the alert
                    }
                }
                return null;
            }
        }

        public async Task SaveToXmlAsync<T>(T data, string fileName) where T : class
        {
            try
            {
                string filePath = GetFilePath(fileName);
                Debug.WriteLine($"Attempting to save file: {filePath}");
                
                // Ensure directory exists
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using var stream = File.Create(filePath);
                var serializer = new XmlSerializer(typeof(T));
                await Task.Run(() => serializer.Serialize(stream, data));
                Debug.WriteLine($"Successfully saved file: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving file {fileName}: {ex.Message}\n{ex.StackTrace}");
                // Try to display an alert, but only if Shell.Current is available
                if (Shell.Current != null)
                {
                    try
                    {
                        await Shell.Current.DisplayAlert("Error", $"Failed to save data: {ex.Message}", "OK");
                    }
                    catch (Exception)
                    {
                        // Ignore if we can't show the alert
                    }
                }
            }
        }
        
        public async Task<T?> LoadFromJsonAsync<T>(string fileName) where T : class
        {
            try
            {
                string filePath = GetFilePath(fileName);
                Debug.WriteLine($"Attempting to load JSON file: {filePath}");

                if (!File.Exists(filePath))
                {
                    Debug.WriteLine($"File does not exist: {filePath}");
                    return null;
                }

                using var stream = File.OpenRead(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                return await JsonSerializer.DeserializeAsync<T>(stream, options);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading JSON file {fileName}: {ex.Message}\n{ex.StackTrace}");
                // Try to display an alert, but only if Shell.Current is available
                if (Shell.Current != null)
                {
                    try
                    {
                        await Shell.Current.DisplayAlert("Error", $"Failed to load JSON data: {ex.Message}", "OK");
                    }
                    catch (Exception)
                    {
                        // Ignore if we can't show the alert
                    }
                }
                return null;
            }
        }

        public async Task SaveToJsonAsync<T>(T data, string fileName) where T : class
        {
            try
            {
                string filePath = GetFilePath(fileName);
                Debug.WriteLine($"Attempting to save JSON file: {filePath}");
                
                // Ensure directory exists
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using var stream = File.Create(filePath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                await JsonSerializer.SerializeAsync(stream, data, options);
                Debug.WriteLine($"Successfully saved JSON file: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving JSON file {fileName}: {ex.Message}\n{ex.StackTrace}");
                // Try to display an alert, but only if Shell.Current is available
                if (Shell.Current != null)
                {
                    try
                    {
                        await Shell.Current.DisplayAlert("Error", $"Failed to save JSON data: {ex.Message}", "OK");
                    }
                    catch (Exception)
                    {
                        // Ignore if we can't show the alert
                    }
                }
            }
        }

        public async Task ImportFileAsync(string sourceFilePath, string targetFileName)
        {
            try
            {
                string targetPath = GetFilePath(targetFileName);
                Debug.WriteLine($"Attempting to import file from {sourceFilePath} to {targetPath}");
                
                // Ensure directory exists
                string directoryPath = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                // Use a safer approach with streams
                using (var sourceStream = File.OpenRead(sourceFilePath))
                using (var targetStream = File.Create(targetPath))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }
                
                Debug.WriteLine($"Successfully imported file to {targetPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error importing file: {ex.Message}\n{ex.StackTrace}");
                // Try to display an alert, but only if Shell.Current is available
                if (Shell.Current != null)
                {
                    try
                    {
                        await Shell.Current.DisplayAlert("Error", $"Failed to import file: {ex.Message}", "OK");
                    }
                    catch (Exception)
                    {
                        // Ignore if we can't show the alert
                    }
                }
            }
        }
        
        public async Task<bool> ShareFileAsync(string fileName, string title = "Share File")
        {
            try
            {
                string filePath = GetFilePath(fileName);
                if (!File.Exists(filePath))
                {
                    Debug.WriteLine($"File does not exist for sharing: {filePath}");
                    return false;
                }
                
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = title,
                    File = new ShareFile(filePath)
                });
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sharing file: {ex.Message}\n{ex.StackTrace}");
                if (Shell.Current != null)
                {
                    try
                    {
                        await Shell.Current.DisplayAlert("Error", $"Failed to share file: {ex.Message}", "OK");
                    }
                    catch
                    {
                        // Ignore if we can't show the alert
                    }
                }
                return false;
            }
        }
    }
}
