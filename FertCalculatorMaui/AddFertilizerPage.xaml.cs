using System.Diagnostics;
using FertCalculatorMaui.Services;
using FertCalculatorMaui.ViewModels;

namespace FertCalculatorMaui;

public partial class AddFertilizerPage : ContentPage
{
    private readonly AddFertilizerViewModel viewModel;

    public AddFertilizerPage(FileService fileService, IDialogService dialogService)
    {
        try
        {
            Debug.WriteLine("AddFertilizerPage constructor starting");
            InitializeComponent();
            
            // Initialize ViewModel
            viewModel = new AddFertilizerViewModel(fileService, dialogService);
            BindingContext = viewModel;
            Debug.WriteLine("ViewModel initialized and set as BindingContext");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in AddFertilizerPage constructor: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    public AddFertilizerPage(FileService fileService, IDialogService dialogService, Fertilizer existingFertilizer)
    {
        try
        {
            Debug.WriteLine("AddFertilizerPage constructor (with existing fertilizer) starting");
            InitializeComponent();
            
            // Initialize ViewModel and load existing fertilizer
            viewModel = new AddFertilizerViewModel(fileService, dialogService);
            BindingContext = viewModel;
            viewModel.LoadExistingFertilizer(existingFertilizer);
            Debug.WriteLine("ViewModel initialized with existing fertilizer data");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in AddFertilizerPage constructor (with existing fertilizer): {ex.Message}\n{ex.StackTrace}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = viewModel.LoadFertilizersAsync();
    }
}
