using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace FertCalculatorMaui.Services
{
    public class DialogService : IDialogService
    {
        public async Task<bool> DisplayConfirmationAsync(string title, string message, string acceptText, string cancelText)
        {
            return await GetCurrentPage().DisplayAlert(title, message, acceptText, cancelText);
        }

        public async Task DisplayAlertAsync(string title, string message, string cancelText)
        {
            await GetCurrentPage().DisplayAlert(title, message, cancelText);
        }
        
        public async Task DisplayAlert(string title, string message, string cancelText)
        {
            await GetCurrentPage().DisplayAlert(title, message, cancelText);
        }
        
        private Page GetCurrentPage()
        {
            // For all platforms, use the recommended approach for .NET MAUI
            if (Application.Current?.Windows != null && Application.Current.Windows.Count > 0)
            {
                // Get the first window (most apps only have one window)
                var window = Application.Current.Windows[0];
                
                if (window?.Page != null)
                {
                    return GetVisiblePage(window.Page);
                }
            }
            
            throw new InvalidOperationException("Could not determine current page for dialog display");
        }
        
        private Page GetVisiblePage(Page page)
        {
            // Handle different navigation patterns
            if (page is Shell shell)
            {
                return shell.CurrentPage;
            }
            
            if (page is NavigationPage navPage)
            {
                return navPage.CurrentPage;
            }
            
            if (page is TabbedPage tabbedPage)
            {
                return tabbedPage.CurrentPage;
            }
            
            if (page is FlyoutPage flyoutPage)
            {
                return flyoutPage.Detail;
            }
            
            // If it's a regular page, just return it
            return page;
        }
    }
}
