using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using FertCalculatorMaui.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace FertCalculatorMaui.ViewModels
{
    public partial class ManageFertilizersViewModel : ObservableObject
    {
        private readonly FileService fileService;
        private readonly IDialogService dialogService;

        [ObservableProperty]
        private ObservableCollection<Fertilizer> availableFertilizers;

        [ObservableProperty]
        private Fertilizer selectedFertilizer;

        public ManageFertilizersViewModel(FileService fileService, IDialogService dialogService, ObservableCollection<Fertilizer> fertilizers)
        {
            this.fileService = fileService;
            this.dialogService = dialogService;
            AvailableFertilizers = new ObservableCollection<Fertilizer>(fertilizers);
            
            _ = ReloadFertilizersAsync();
        }

        [RelayCommand]
        private async Task AddFertilizer()
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new AddFertilizerPage(fileService, dialogService));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to AddFertilizerPage: {ex.Message}");
                await dialogService.DisplayAlertAsync("Error", "Could not open Add Fertilizer page", "OK");
            }
        }

        [RelayCommand]
        private async Task EditFertilizer(Fertilizer fertilizer = null)
        {
            // Use the passed fertilizer parameter if provided, otherwise use SelectedFertilizer
            fertilizer = fertilizer ?? SelectedFertilizer;
            
            if (fertilizer != null)
            {
                try
                {
                    await Shell.Current.Navigation.PushAsync(new AddFertilizerPage(fileService, dialogService, fertilizer));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error navigating to AddFertilizerPage: {ex.Message}");
                    await dialogService.DisplayAlertAsync("Error", "Could not open Edit Fertilizer page", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task DeleteFertilizer(Fertilizer fertilizer)
        {
            if (fertilizer == null)
                return;

            try
            {
                // Check whether the fertilizer is used by any saved mix
                var mixes = await fileService.LoadMixesAsync();
                bool isUsedInMixes = mixes.Any(m => m.Ingredients.Any(i =>
                    string.Equals(i.FertilizerName, fertilizer.Name, StringComparison.OrdinalIgnoreCase)));

                string message = isUsedInMixes
                    ? $"{fertilizer.Name} is used in one or more mixes. Deleting it will remove it from all mixes. Continue?"
                    : $"Are you sure you want to delete {fertilizer.Name}?";

                bool confirm = await dialogService.DisplayConfirmationAsync("Confirm Delete", message, "Yes", "No");
                if (!confirm) return;

                // Remove from the persisted list by name (loaded objects are different instances)
                var fertilizers = await fileService.LoadFertilizersAsync();
                fertilizers.RemoveAll(f => string.Equals(f.Name, fertilizer.Name, StringComparison.OrdinalIgnoreCase));
                await fileService.SaveFertilizersAsync(fertilizers);

                // Cascade-remove it from any saved mixes
                if (isUsedInMixes)
                {
                    foreach (var mix in mixes)
                    {
                        mix.Ingredients.RemoveAll(i =>
                            string.Equals(i.FertilizerName, fertilizer.Name, StringComparison.OrdinalIgnoreCase));
                    }
                    await fileService.SaveMixesAsync(mixes);
                }

                AvailableFertilizers.Remove(fertilizer);

                // Notify other pages that fertilizers have been updated
                WeakReferenceMessenger.Default.Send(new FertilizersUpdatedMessage(null));
                WeakReferenceMessenger.Default.Send(new FertilizerDeletedMessage(fertilizer.Name));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting fertilizer: {ex.Message}");
                await dialogService.DisplayAlertAsync("Error", "Could not delete fertilizer", "OK");
            }
        }
        
        [RelayCommand]
        private async Task RemoveFertilizer()
        {
            if (SelectedFertilizer == null)
                return;
                
            await DeleteFertilizer(SelectedFertilizer);
        }
        
        [RelayCommand]
        private async Task BackToMix()
        {
            // Navigate back to the previous page
            var navigation = GetCurrentNavigation();
            if (navigation != null)
            {
                await navigation.PopAsync();
            }
        }
        
        [RelayCommand]
        private void AddToMix(Fertilizer fertilizer)
        {
            if (fertilizer == null)
                return;
                
            try
            {
                // Send a message to add the fertilizer to the mix
                WeakReferenceMessenger.Default.Send(new AddFertilizerToMixMessage(fertilizer));
                
                // Navigate back to the main page
                _ = BackToMix();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding fertilizer to mix: {ex.Message}");
                _ = dialogService.DisplayAlertAsync("Error", "Could not add fertilizer to mix", "OK");
            }
        }
        
        [RelayCommand]
        private async Task ShowFertilizerDetails(Fertilizer fertilizer)
        {
            if (fertilizer == null)
                return;

            try
            {
                // Create a formatted string with fertilizer details
                string details = $"Name: {fertilizer.Name}\n\n";
                
                // Add nutrient information
                var nutrientDetails = new List<(string Name, double Value)>();
                
                // Add Nitrogen
                nutrientDetails.Add(("Nitrogen (N)", fertilizer.NitrogenPercent));
                
                // Add Phosphorus with oxide form if applicable
                string phosphorusLabel = "Phosphorus (P)";
                if (fertilizer.IsPhosphorusInOxideForm && fertilizer.PhosphorusPercent > 0)
                {
                    phosphorusLabel = $"Phosphorus (P) [P₂O₅: {fertilizer.OriginalPhosphorusValue:F2}%]";
                }
                nutrientDetails.Add((phosphorusLabel, fertilizer.PhosphorusPercent));
                
                // Add Potassium with oxide form if applicable
                string potassiumLabel = "Potassium (K)";
                if (fertilizer.IsPotassiumInOxideForm && fertilizer.PotassiumPercent > 0)
                {
                    potassiumLabel = $"Potassium (K) [K₂O: {fertilizer.OriginalPotassiumValue:F2}%]";
                }
                nutrientDetails.Add((potassiumLabel, fertilizer.PotassiumPercent));
                
                // Add remaining nutrients
                nutrientDetails.AddRange(new List<(string Name, double Value)>
                {
                    ("Calcium (Ca)", fertilizer.CalciumPercent),
                    ("Magnesium (Mg)", fertilizer.MagnesiumPercent),
                    ("Sulfur (S)", fertilizer.SulfurPercent),
                    ("Boron (B)", fertilizer.BoronPercent),
                    ("Copper (Cu)", fertilizer.CopperPercent),
                    ("Iron (Fe)", fertilizer.IronPercent),
                    ("Manganese (Mn)", fertilizer.ManganesePercent),
                    ("Molybdenum (Mo)", fertilizer.MolybdenumPercent),
                    ("Zinc (Zn)", fertilizer.ZincPercent),
                    ("Chlorine (Cl)", fertilizer.ChlorinePercent),
                    ("Silica (Si)", fertilizer.SilicaPercent),
                    ("Humic Acid", fertilizer.HumicAcidPercent),
                    ("Fulvic Acid", fertilizer.FulvicAcidPercent),
                    ("Kelp", fertilizer.KelpPercent)
                });
                
                var nonZeroNutrients = nutrientDetails
                    .Where(n => n.Value > 0);
                
                if (nonZeroNutrients.Any())
                {
                    details += "Nutrients:\n";
                    foreach (var nutrient in nonZeroNutrients)
                    {
                        details += $"{nutrient.Name}: {nutrient.Value:F3}%\n";
                    }
                }
                else
                {
                    details += "No nutrients specified.";
                }
                
                await dialogService.DisplayAlertAsync("Fertilizer Details", details, "Close");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing fertilizer details: {ex.Message}");
                await dialogService.DisplayAlertAsync("Error", "Could not display fertilizer details", "OK");
            }
        }
        
        public async Task DeleteFertilizerAsync(Fertilizer fertilizer)
        {
            // This method is called from the code-behind event handler
            await DeleteFertilizer(fertilizer);
        }
        
        private INavigation GetCurrentNavigation()
        {
            // For all platforms, use the recommended approach for .NET MAUI
            if (Application.Current?.Windows != null && Application.Current.Windows.Count > 0)
            {
                // Get the first window (most apps only have one window)
                var window = Application.Current.Windows[0];
                
                if (window?.Page != null)
                {
                    return GetVisiblePageNavigation(window.Page);
                }
            }
            
            throw new InvalidOperationException("Could not determine current navigation for page navigation");
        }
        
        private INavigation GetVisiblePageNavigation(Page page)
        {
            if (page is Shell shell)
            {
                return shell.CurrentPage?.Navigation ?? shell.Navigation;
            }
            
            if (page is NavigationPage navPage)
            {
                return navPage.CurrentPage?.Navigation ?? navPage.Navigation;
            }
            
            if (page is TabbedPage tabbedPage)
            {
                return tabbedPage.CurrentPage?.Navigation ?? tabbedPage.Navigation;
            }
            
            if (page is FlyoutPage flyoutPage)
            {
                return flyoutPage.Detail?.Navigation ?? flyoutPage.Navigation;
            }
            
            return page.Navigation;
        }

        public async Task ReloadFertilizersAsync()
        {
            try
            {
                var updatedFertilizers = await fileService.LoadFertilizersAsync();
                
                // Use a custom sorting logic that handles numerical values in fertilizer names
                var sortedFertilizers = updatedFertilizers.OrderBy(f => f.Name, new FertilizerNameComparer()).ToList();
                
                AvailableFertilizers.Clear();
                foreach (var fertilizer in sortedFertilizers)
                {
                    AvailableFertilizers.Add(fertilizer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reloading fertilizers: {ex.Message}");
                await dialogService.DisplayAlertAsync("Error", "Could not reload fertilizers", "OK");
            }
        }
        
    }
}
