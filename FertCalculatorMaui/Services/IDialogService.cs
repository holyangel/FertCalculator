using System;
using System.Threading.Tasks;

namespace FertCalculatorMaui.Services
{
    public interface IDialogService
    {
        Task<bool> DisplayConfirmationAsync(string title, string message, string acceptText, string cancelText);
        Task DisplayAlertAsync(string title, string message, string cancelText);
        Task DisplayAlert(string title, string message, string cancelText);
        Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons);
    }
}
