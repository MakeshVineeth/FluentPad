using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentPad
{
    internal class Operations
    {
        private readonly TextBox textBoxMain;
        private readonly HelpMenu helpMenu;

        public Operations(TextBox textBoxMain)
        {
            helpMenu = new HelpMenu();
            this.textBoxMain = textBoxMain;
        }

        public async void Replace()
        {
            if (string.IsNullOrWhiteSpace(textBoxMain.Text))
            {
                var msgBox = new MessageDialog("No text found!", "Replace failed");
                await msgBox.ShowAsync();
                return;
            }

            var inputTextBoxFrom = new TextBox
            {
                AcceptsReturn = false,
                VerticalAlignment = VerticalAlignment.Bottom,
                PlaceholderText = "Enter old text...",
                Margin = new Thickness(0, 0, 0, 10),
            };

            var inputTextBoxTo = new TextBox
            {
                AcceptsReturn = false,
                VerticalAlignment = VerticalAlignment.Bottom,
                PlaceholderText = "Enter new text...",
                Margin = new Thickness(0, 0, 0, 10),
            };

            var caseInsensitive = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Content = "Case Insensitive",
                IsChecked = false,
                IsThreeState = false,
            };

            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(inputTextBoxFrom);
            stackPanel.Children.Add(inputTextBoxTo);
            stackPanel.Children.Add(caseInsensitive);

            var dialog = new ContentDialog
            {
                Content = stackPanel,
                Title = "Enter text to replace for",
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel"
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                bool caseFlag = caseInsensitive.IsChecked ?? false;
                string from = inputTextBoxFrom.Text;
                string to = inputTextBoxTo.Text;

                if (string.IsNullOrWhiteSpace(to) && string.IsNullOrWhiteSpace(from)) return;

                string text = textBoxMain.Text;
                StringComparison comparison = caseFlag ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

                if (text.Contains(from, comparison))
                {
                    while (text.Contains(from, comparison))
                    {
                        text = text.Replace(from, to, comparison);
                        textBoxMain.Text = text;
                        var messageBox = new MessageDialog("Replaced!", "Success");
                        await messageBox.ShowAsync();
                        textBoxMain.Focus(FocusState.Programmatic);
                        int index = text.IndexOf(to);
                        textBoxMain.SelectionStart = index;
                        textBoxMain.SelectionLength = 0;
                    }
                }
                else
                {
                    textBoxMain.SelectionLength = 0;
                    textBoxMain.SelectionStart = 0;
                    var messageBox = new MessageDialog("Not found any instances to replace with!", "Error");
                    await messageBox.ShowAsync();
                }
            }
        }

        public async void CloseApplicationPrompt()
        {
            MessageDialog messageDialog = new MessageDialog("File is not saved. Do you still want to close this file?", "Prompt");
            messageDialog.Commands.Add(new UICommand("Yes", null));
            messageDialog.Commands.Add(new UICommand("No", null));
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;

            if ((await messageDialog.ShowAsync()).Label == "Yes")
            {
                helpMenu.ExitApp();
            }
        }

        public async void SearchText()
        {
            string text = textBoxMain.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                var msgBox = new MessageDialog("No text found!", "Unable to search");
                await msgBox.ShowAsync();
                return;
            }

            text = text.ToLower();

            string val = await CommonUtils.ShowAddDialogAsync("Search for any text");
            if (string.IsNullOrWhiteSpace(val)) return;
            val = val.ToLower();
            textBoxMain.Focus(FocusState.Programmatic);

            if (text.Contains(val))
            {
                int indexStart = text.IndexOf(val);
                textBoxMain.SelectionStart = indexStart;
                textBoxMain.SelectionLength = val.Length;
            }
            else
            {
                textBoxMain.SelectionLength = 0;
                textBoxMain.SelectionStart = 0;
                var messageBox = new MessageDialog("Not found!", "Unable to find");
                await messageBox.ShowAsync();
            }
        }

        public void SelectAll()
        {
            if (!string.IsNullOrWhiteSpace(textBoxMain.Text))
            {
                textBoxMain.Focus(FocusState.Programmatic);
                textBoxMain.SelectAll();
            }
        }
    }


}
