using System.Diagnostics;
using FertCalculatorMaui.Services;

namespace FertCalculatorMaui;

public partial class App : Application
{
    public App()
    {
        try
        {
            Debug.WriteLine("App constructor starting");
            InitializeComponent();
            Debug.WriteLine("InitializeComponent completed");

            // Set up unhandled exception handlers
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                Debug.WriteLine($"Unhandled exception: {ex.Message}\n{ex.StackTrace}");
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Debug.WriteLine($"Unobserved task exception: {args.Exception.Message}\n{args.Exception.StackTrace}");
                args.SetObserved();
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in App constructor: {ex.Message}\n{ex.StackTrace}");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            Debug.WriteLine("CreateWindow called");
            
            // Add Android-specific exception handling
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                Debug.WriteLine("Running on Android platform");
                
                // Register global exception handler for Android
                Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
                {
                    Exception e = args.Exception;
                    Debug.WriteLine($"Android UnhandledExceptionRaiser: {e.GetType()}: {e.Message}\n{e.StackTrace}");
                    
                    // Write the exception to a file in app data directory
                    try 
                    {
                        string errorPath = Path.Combine(FileSystem.AppDataDirectory, "crash_log.txt");
                        File.WriteAllText(errorPath, $"Crash at {DateTime.Now}:\n{e.GetType()}: {e.Message}\n{e.StackTrace}");
                    }
                    catch {}
                };
            }
            
            var window = new Window(new AppShell());
            Debug.WriteLine("Window created with AppShell");
            return window;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in CreateWindow: {ex.Message}\n{ex.StackTrace}");
            
            // Try to save exception details to a file
            try 
            {
                string errorPath = Path.Combine(FileSystem.AppDataDirectory, "create_window_error.txt");
                File.WriteAllText(errorPath, $"Crash at {DateTime.Now}:\n{ex.GetType()}: {ex.Message}\n{ex.StackTrace}");
            }
            catch {}
            
            // Create a fallback window with an error message
            var errorPage = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = "An error occurred during initialization", HorizontalOptions = LayoutOptions.Center },
                        new Label { Text = ex.Message, HorizontalOptions = LayoutOptions.Center }
                    }
                }
            };
            return new Window(errorPage);
        }
    }
}
