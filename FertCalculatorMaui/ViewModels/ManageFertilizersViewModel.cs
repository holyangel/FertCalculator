using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using FertCalculatorMaui.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace FertCalculatorMaui
{
    public partial class ManageFertilizersViewModel : ObservableObject
    {
        private readonly FileService _fileService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<Fertilizer> availableFertilizers;

        [ObservableProperty]
        private Fertilizer selectedFertilizer;

        public ManageFertilizersViewModel(FileService fileService, IDialogService dialogService, ObservableCollection<Fertilizer> fertilizers)
        {
            _fileService = fileService;
            _dialogService = dialogService;
            AvailableFertilizers = fertilizers;
            
            _ = ReloadFertilizersAsync();
        }

        [RelayCommand]
        private async Task AddFertilizer()
        {
            // This will be handled by the page's navigation
            // The page will handle navigation to AddFertilizerPage
        }

        [RelayCommand]
        private async Task EditFertilizer()
        {
            if (SelectedFertilizer != null)
            {
                // This will be handled by the page's navigation
                // The page will handle navigation to AddFertilizerPage with the selected fertilizer
            }
        }

        [RelayCommand]
        private async Task DeleteFertilizer(Fertilizer fertilizer)
        {
            if (fertilizer == null)
                return;

            bool confirmed = await _dialogService.DisplayConfirmationAsync(
                "Confirm Delete",
                $"Are you sure you want to delete {fertilizer.Name}?",
                "Yes", "No");

            if (confirmed)
            {
                AvailableFertilizers.Remove(fertilizer);
                await _fileService.SaveFertilizersAsync(AvailableFertilizers.ToList());
                
                // Notify other pages that fertilizers have been updated
                // Pass the deleted fertilizer to the message
                WeakReferenceMessenger.Default.Send(new FertilizersUpdatedMessage(fertilizer));
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
            var updatedFertilizers = await _fileService.LoadFertilizersAsync();
            
            // Use a custom sorting logic that handles numerical values in fertilizer names
            var sortedFertilizers = updatedFertilizers.OrderBy(f => f.Name, new FertilizerNameComparer()).ToList();
            
            AvailableFertilizers.Clear();
            foreach (var fertilizer in sortedFertilizers)
            {
                AvailableFertilizers.Add(fertilizer);
            }
        }
        
        /// <summary>
        /// Custom comparer for fertilizer names that handles numerical values within the names.
        /// Ensures that names like "Jacks 5-15-26" come before "Jacks 10-30-20".
        /// </summary>
        private class FertilizerNameComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == null) return y == null ? 0 : -1;
                if (y == null) return 1;
                
                // Split the names into segments by whitespace, punctuation, and other delimiters
                string[] segmentsX = SplitIntoSegments(x);
                string[] segmentsY = SplitIntoSegments(y);
                
                // Compare each segment
                int minSegments = Math.Min(segmentsX.Length, segmentsY.Length);
                for (int i = 0; i < minSegments; i++)
                {
                    string segX = segmentsX[i];
                    string segY = segmentsY[i];
                    
                    // Try to parse segments as numbers
                    bool isNumX = double.TryParse(segX, out double numX);
                    bool isNumY = double.TryParse(segY, out double numY);
                    
                    // If both are numbers, compare numerically
                    if (isNumX && isNumY)
                    {
                        int numCompare = numX.CompareTo(numY);
                        if (numCompare != 0) return numCompare;
                    }
                    // If only one is a number, numbers come before text
                    else if (isNumX) return -1;
                    else if (isNumY) return 1;
                    // If both are text, compare alphabetically
                    else
                    {
                        int textCompare = string.Compare(segX, segY, StringComparison.OrdinalIgnoreCase);
                        if (textCompare != 0) return textCompare;
                    }
                }
                
                // If all compared segments are equal, the one with more segments comes later
                return segmentsX.Length.CompareTo(segmentsY.Length);
            }
            
            private string[] SplitIntoSegments(string input)
            {
                // Use regex to split the string into segments of numbers and non-numbers
                // This will handle patterns like "Jacks 5-15-26" appropriately
                return System.Text.RegularExpressions.Regex.Split(input, 
                    @"(?<=\D)(?=\d)|(?<=\d)(?=\D)|[^a-zA-Z0-9]")
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();
            }
        }
    }
}
