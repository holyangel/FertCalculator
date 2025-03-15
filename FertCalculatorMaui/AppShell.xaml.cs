using System.Diagnostics;

namespace FertCalculatorMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            Debug.WriteLine("AppShell constructor starting");
            InitializeComponent();
            Debug.WriteLine("AppShell InitializeComponent completed");
            
            // Register routes for navigation
            Routing.RegisterRoute(nameof(AddFertilizerPage), typeof(AddFertilizerPage));
            Routing.RegisterRoute(nameof(SaveMixPage), typeof(SaveMixPage));
            Debug.WriteLine("AppShell routes registered");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in AppShell constructor: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
