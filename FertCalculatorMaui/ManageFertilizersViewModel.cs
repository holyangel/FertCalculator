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
            
            var sortedFertilizers = updatedFertilizers.OrderBy(f => f.Name).ToList();
            
            AvailableFertilizers.Clear();
            foreach (var fertilizer in sortedFertilizers)
            {
                AvailableFertilizers.Add(fertilizer);
            }
        }
    }
}
