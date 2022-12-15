using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentPad
{
    internal class CommonUtils
    {
        public static async Task<string> ShowTextPromptAsync(string title)
        {
            var inputTextBox = new TextBox
            {
                AcceptsReturn = false,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            var dialog = new ContentDialog
            {
                Content = inputTextBox,
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel"
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }

        public static void ShowDialog(object message, string title)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Ok"
            };

            _ = contentDialog.ShowAsync();
        }

        public static bool CheckIfUnSaved(TabViewItem item)
        {
            if (item?.IconSource == null)
            {
                return false;
            }

            Microsoft.UI.Xaml.Controls.SymbolIconSource symbolIcon = item.IconSource as Microsoft.UI.Xaml.Controls.SymbolIconSource;
            return symbolIcon.Symbol == Symbol.Edit;
        }

        public static async Task<bool> ShowPromptAsync(object content, string title)
        {
            try
            {
                ContentDialog confirmFileDialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    PrimaryButtonText = "Yes",
                    IsSecondaryButtonEnabled = true,
                    CloseButtonText = "No"
                };

                if ((await confirmFileDialog.ShowAsync()) == ContentDialogResult.Primary)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
