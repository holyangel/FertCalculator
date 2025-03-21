using FertCalculatorMaui.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.DependencyInjection;
using FertCalculatorMaui.ViewModels;
#if ANDROID
using Android.Runtime;
#endif

namespace FertCalculatorMaui;

public partial class App : Application
{
    private readonly IServiceProvider serviceProvider;
    private MainViewModel mainViewModel;
    private FileService fileService;
    private IDialogService dialogService;
    private AppSettings appSettings;
    
    public App(IServiceProvider serviceProvider)
    {
        try
        {
            Debug.WriteLine("App constructor starting");
            
            // Initialize resources
            try
            {
                InitializeComponent();
                Debug.WriteLine("App InitializeComponent completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeComponent: {ex.Message}");
                // Continue without initializing components
                // We'll set up a fallback UI in CreateWindow
            }
            
            this.serviceProvider = serviceProvider;
            
            // Get required services
            if (serviceProvider != null)
            {
                try
                {
                    mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                    fileService = serviceProvider.GetRequiredService<FileService>();
                    dialogService = serviceProvider.GetRequiredService<IDialogService>();
                    appSettings = serviceProvider.GetRequiredService<AppSettings>();
                    Debug.WriteLine("Successfully retrieved core services");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting core services: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("WARNING: serviceProvider is null in App constructor");
            }

            // Apply custom button style based on settings
            try
            {
                ApplyCustomButtonStyle();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying custom button style: {ex.Message}");
            }
            
            Debug.WriteLine("App initialization completed");

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
#if ANDROID
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
#endif
            }
            
            // Create a page for the window
            Page mainPage = CreateMainPage();
            Debug.WriteLine($"Created main page of type: {mainPage.GetType().Name}");
            
            // Create a window with the page
            var window = new Window(mainPage);
            Debug.WriteLine("Window created with MainPage");
            
            // If the main page is a Shell, ensure the flyout behavior is set
            if (mainPage is Shell shell)
            {
                shell.FlyoutBehavior = FlyoutBehavior.Flyout;
                Debug.WriteLine($"Set Shell.FlyoutBehavior to {shell.FlyoutBehavior} in CreateWindow");
            }
            
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
            
            // Create a fallback error page
            var errorPage = CreateErrorPage($"Application error: {ex.Message}");
            return new Window(errorPage);
        }
    }
    
    private Page CreateMainPage()
    {
        try
        {
            Debug.WriteLine("CreateMainPage called");
            
            // Try to create AppShell first
            if (serviceProvider != null)
            {
                try
                {
                    // Try to get AppShell from the service provider
                    var appShell = serviceProvider.GetService<AppShell>();
                    if (appShell != null)
                    {
                        Debug.WriteLine("Created AppShell from service provider");
                        
                        // Ensure the flyout behavior is set
                        appShell.FlyoutBehavior = FlyoutBehavior.Flyout;
                        Debug.WriteLine($"AppShell FlyoutBehavior set to: {appShell.FlyoutBehavior}");
                        
                        return appShell;
                    }
                    else
                    {
                        Debug.WriteLine("WARNING: AppShell is null from service provider");
                        
                        // Try to create AppShell directly
                        try
                        {
                            var newAppShell = new AppShell(serviceProvider);
                            Debug.WriteLine("Created AppShell directly");
                            return newAppShell;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to create AppShell directly: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to create AppShell: {ex.Message}");
                }
            }
            
            // Fallback: Create a basic MainPage
            if (mainViewModel != null && fileService != null && dialogService != null)
            {
                try
                {
                    var mainPage = new MainPage(fileService, dialogService)
                    {
                        BindingContext = mainViewModel
                    };
                    Debug.WriteLine("Created MainPage directly");
                    return mainPage;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to create MainPage: {ex.Message}");
                }
            }
            
            // Last resort: Create a simple error page
            return CreateErrorPage("Failed to initialize application. Please restart the app.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in CreateMainPage: {ex.Message}");
            return CreateErrorPage($"Application error: {ex.Message}");
        }
    }
    
    private ContentPage CreateErrorPage(string errorMessage)
    {
        return new ContentPage
        {
            BackgroundColor = Colors.Black,
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = new Thickness(20),
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "Fertilizer Calculator",
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = errorMessage,
                        FontSize = 16,
                        TextColor = Colors.Red,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Button
                    {
                        Text = "Restart App",
                        Command = new Command(() => Application.Current?.Quit()),
                        HorizontalOptions = LayoutOptions.Center,
                        BackgroundColor = Color.FromArgb("#512BD4"),
                        TextColor = Colors.White,
                        Padding = new Thickness(10, 5)
                    }
                }
            }
        };
    }

    protected override void OnResume()
    {
        base.OnResume();
        Debug.WriteLine("App.OnResume called");
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        Debug.WriteLine("App.OnSleep called");
    }

    private void ApplyCustomButtonStyle()
    {
        try
        {
            // Get app settings
            if (appSettings == null)
            {
                Debug.WriteLine("WARNING: AppSettings not available for button styling");
                return;
            }

            // Ensure resources are loaded
            if (Resources == null)
            {
                Debug.WriteLine("WARNING: Resources dictionary is null");
                return;
            }

            // Get the button style from resources
            if (Resources.TryGetValue("ButtonStyle", out var buttonStyle) && buttonStyle is Style style)
            {
                // Apply custom button color if set
                if (!string.IsNullOrEmpty(appSettings.ButtonColor))
                {
                    try
                    {
                        var color = Color.FromArgb(appSettings.ButtonColor);
                        
                        // Remove existing BackgroundColor setter if any
                        var existingSetter = style.Setters.FirstOrDefault(s => 
                            s is Setter setter && setter.Property == Button.BackgroundColorProperty);
                        
                        if (existingSetter != null)
                        {
                            style.Setters.Remove(existingSetter);
                        }
                        
                        // Add new setter
                        style.Setters.Add(new Setter { Property = Button.BackgroundColorProperty, Value = color });
                        Debug.WriteLine($"Applied custom button color: {appSettings.ButtonColor}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error parsing button color: {ex.Message}");
                    }
                }
                
                // Apply custom text color if set
                if (!string.IsNullOrEmpty(appSettings.ButtonTextColor))
                {
                    try
                    {
                        var textColor = Color.FromArgb(appSettings.ButtonTextColor);
                        
                        // Remove existing TextColor setter if any
                        var existingSetter = style.Setters.FirstOrDefault(s => 
                            s is Setter setter && setter.Property == Button.TextColorProperty);
                        
                        if (existingSetter != null)
                        {
                            style.Setters.Remove(existingSetter);
                        }
                        
                        // Add new setter
                        style.Setters.Add(new Setter { Property = Button.TextColorProperty, Value = textColor });
                        Debug.WriteLine($"Applied custom button text color: {appSettings.ButtonTextColor}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error parsing button text color: {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.WriteLine("WARNING: ButtonStyle not found in resources");
                
                // Create a new style if it doesn't exist
                var newStyle = new Style(typeof(Button));
                
                // Add basic properties
                newStyle.Setters.Add(new Setter { Property = Button.FontFamilyProperty, Value = "OpenSansRegular" });
                newStyle.Setters.Add(new Setter { Property = Button.FontSizeProperty, Value = 14 });
                newStyle.Setters.Add(new Setter { Property = Button.BorderWidthProperty, Value = 0 });
                newStyle.Setters.Add(new Setter { Property = Button.CornerRadiusProperty, Value = 8 });
                newStyle.Setters.Add(new Setter { Property = Button.PaddingProperty, Value = new Thickness(14, 10) });
                newStyle.Setters.Add(new Setter { Property = Button.MinimumHeightRequestProperty, Value = 44 });
                newStyle.Setters.Add(new Setter { Property = Button.MinimumWidthRequestProperty, Value = 44 });
                
                // Add colors from settings
                try
                {
                    if (!string.IsNullOrEmpty(appSettings.ButtonColor))
                    {
                        var color = Color.FromArgb(appSettings.ButtonColor);
                        newStyle.Setters.Add(new Setter { Property = Button.BackgroundColorProperty, Value = color });
                    }
                    else
                    {
                        newStyle.Setters.Add(new Setter { Property = Button.BackgroundColorProperty, Value = Colors.DarkGreen });
                    }
                    
                    if (!string.IsNullOrEmpty(appSettings.ButtonTextColor))
                    {
                        var textColor = Color.FromArgb(appSettings.ButtonTextColor);
                        newStyle.Setters.Add(new Setter { Property = Button.TextColorProperty, Value = textColor });
                    }
                    else
                    {
                        newStyle.Setters.Add(new Setter { Property = Button.TextColorProperty, Value = Colors.White });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting colors for new style: {ex.Message}");
                    newStyle.Setters.Add(new Setter { Property = Button.BackgroundColorProperty, Value = Colors.DarkGreen });
                    newStyle.Setters.Add(new Setter { Property = Button.TextColorProperty, Value = Colors.White });
                }
                
                // Add visual states
                var visualStateGroupList = new VisualStateGroupList();
                var commonStatesGroup = new VisualStateGroup { Name = "CommonStates" };
                
                var normalState = new VisualState { Name = "Normal" };
                commonStatesGroup.States.Add(normalState);
                
                var disabledState = new VisualState { Name = "Disabled" };
                disabledState.Setters.Add(new Setter { Property = Button.TextColorProperty, Value = Colors.Gray });
                disabledState.Setters.Add(new Setter { Property = Button.BackgroundColorProperty, Value = Color.FromArgb("#666666") });
                commonStatesGroup.States.Add(disabledState);
                
                visualStateGroupList.Add(commonStatesGroup);
                newStyle.Setters.Add(new Setter { Property = VisualStateManager.VisualStateGroupsProperty, Value = visualStateGroupList });
                
                // Add the style to resources
                Resources.Add("ButtonStyle", newStyle);
                Debug.WriteLine("Created and added new ButtonStyle to Resources");
            }
            
            // Also check and apply RadioButtonStyle
            if (!Resources.TryGetValue("RadioButtonStyle", out var _))
            {
                Debug.WriteLine("WARNING: RadioButtonStyle not found in resources");
                
                // Create a new RadioButton style
                var radioStyle = new Style(typeof(RadioButton));
                
                // Add basic properties
                radioStyle.Setters.Add(new Setter { Property = RadioButton.BackgroundColorProperty, Value = Colors.Transparent });
                radioStyle.Setters.Add(new Setter { Property = RadioButton.TextColorProperty, Value = Colors.White });
                radioStyle.Setters.Add(new Setter { Property = RadioButton.FontFamilyProperty, Value = "OpenSansRegular" });
                radioStyle.Setters.Add(new Setter { Property = RadioButton.FontSizeProperty, Value = 14 });
                radioStyle.Setters.Add(new Setter { Property = RadioButton.MinimumHeightRequestProperty, Value = 44 });
                radioStyle.Setters.Add(new Setter { Property = RadioButton.MinimumWidthRequestProperty, Value = 44 });
                
                // Add visual states
                var visualStateGroupList = new VisualStateGroupList();
                var commonStatesGroup = new VisualStateGroup { Name = "CommonStates" };
                
                var normalState = new VisualState { Name = "Normal" };
                commonStatesGroup.States.Add(normalState);
                
                var disabledState = new VisualState { Name = "Disabled" };
                disabledState.Setters.Add(new Setter { Property = RadioButton.TextColorProperty, Value = Colors.Gray });
                commonStatesGroup.States.Add(disabledState);
                
                visualStateGroupList.Add(commonStatesGroup);
                radioStyle.Setters.Add(new Setter { Property = VisualStateManager.VisualStateGroupsProperty, Value = visualStateGroupList });
                
                // Add the style to resources
                Resources.Add("RadioButtonStyle", radioStyle);
                Debug.WriteLine("Created and added new RadioButtonStyle to Resources");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ApplyCustomButtonStyle: {ex.Message}");
        }
    }
}
