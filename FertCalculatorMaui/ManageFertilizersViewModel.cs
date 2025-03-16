using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FertCalculatorMaui.Messages;
using FertCalculatorMaui.Services;
using System.Linq;

namespace FertCalculatorMaui
{
    public partial class ManageFertilizersViewModel : ObservableObject
    {
        private readonly FileService _fileService;

        [ObservableProperty]
        private ObservableCollection<Fertilizer> availableFertilizers;

        [ObservableProperty]
        private Fertilizer selectedFertilizer;

        public ManageFertilizersViewModel(FileService fileService, ObservableCollection<Fertilizer> fertilizers)
        {
            _fileService = fileService;
            AvailableFertilizers = fertilizers;
        }

        [RelayCommand]
        private async Task AddFertilizer()
        {
            // This will be handled by the page's navigation
        }

        [RelayCommand]
        private async Task EditFertilizer()
        {
            // This will be handled by the page's navigation
        }

        [RelayCommand]
        private async Task DeleteFertilizer(Fertilizer fertilizer)
        {
            if (fertilizer == null)
                return;

            bool confirmed = await Application.Current.MainPage.DisplayAlert(
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

        public async Task ReloadFertilizersAsync()
        {
            // Load the latest fertilizers from the file
            var updatedFertilizers = await _fileService.LoadFertilizersAsync();
            
            // Sort fertilizers alphabetically by name
            var sortedFertilizers = updatedFertilizers.OrderBy(f => f.Name).ToList();
            
            // Clear and repopulate the collection
            AvailableFertilizers.Clear();
            foreach (var fertilizer in sortedFertilizers)
            {
                AvailableFertilizers.Add(fertilizer);
            }
        }
    }
}
