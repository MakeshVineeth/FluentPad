using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace FluentPad
{
    public sealed partial class MainPage : Page
    {
        StorageFile openedFile = null;
        string previousSearched = string.Empty;
        string lastSavedText = string.Empty;
        const string pattern = " *";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void FileMenuButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void OperationsBtn_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void MiscButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void DarkThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (darkThemeToggle.IsChecked)
            {
                RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                RequestedTheme = ElementTheme.Light;
                var appView = ApplicationView.GetForCurrentView();
                var titleBar = appView.TitleBar;
                titleBar.BackgroundColor = Colors.White;
            }
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.FileTypeFilter.Add(".txt");

                StorageFile file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    openedFile = file;
                    ApplicationView appView = ApplicationView.GetForCurrentView();
                    appView.Title = file.DisplayName;
                    string text = await FileIO.ReadTextAsync(file);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        lastSavedText = text;
                        textBoxMain.Text = text;
                        FixAutoSelect();
                    }
                }
            }
            catch (Exception)
            {
                var messageBox = new MessageDialog("Unable to load file", "ERROR");
                await messageBox.ShowAsync();
            }
        }

        private void FixAutoSelect()
        {
            textBoxMain.SelectionStart = 0;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            lastSavedText = string.Empty;
            textBoxMain.Text = string.Empty;
            openedFile = null;

            ApplicationView view = ApplicationView.GetForCurrentView();
            view.Title = string.Empty;
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            textBoxMain.CutSelectionToClipboard();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            textBoxMain.CopySelectionToClipboard();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            textBoxMain.PasteFromClipboard();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            textBoxMain.SelectedText = string.Empty;
        }

        private void UppercaseBtn_Click(object sender, RoutedEventArgs e)
        {
            textBoxMain.SelectedText = textBoxMain.SelectedText.ToUpper();
        }

        private void LowercaseBtn_Click(object sender, RoutedEventArgs e)
        {
            textBoxMain.SelectedText = textBoxMain.SelectedText.ToLower();
        }

        private void TitlecaseBtn_Click(object sender, RoutedEventArgs e)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            textBoxMain.SelectedText = ti.ToTitleCase(textBoxMain.SelectedText.ToLower());
        }

        private async void SearchGoogleBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = textBoxMain.SelectedText.Trim();
                if (string.IsNullOrWhiteSpace(text)) return;
                string google = "https://www.google.com/search?q=";
                string url = google + WebUtility.UrlEncode(text);
                var urlObject = new Uri(url);
                bool success = await Windows.System.Launcher.LaunchUriAsync(urlObject);

                if (!success)
                {
                    var messageBox = new MessageDialog("Oops, unable to search", "FAILED TO OPEN URL");
                    _ = messageBox.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var messageBox = new MessageDialog("Oops, an error has occurred: " + ex.Message, "ERROR");
                _ = messageBox.ShowAsync();
            }
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxMain.Text))
            {
                textBoxMain.Focus(FocusState.Programmatic);
                textBoxMain.SelectAll();
            }
        }

        private void InsertDateTimeBtn_Click(object sender, RoutedEventArgs e)
        {
            textBoxMain.Focus(FocusState.Programmatic);
            DateTime now = DateTime.Now;
            string date_time = now.ToString();
            textBoxMain.Text += " " + date_time;
            textBoxMain.SelectionStart = textBoxMain.Text.Length;
            textBoxMain.SelectionLength = 0;
        }

        private void PasteClipboardBtn_Click(object sender, RoutedEventArgs e)
        {
            textBoxMain.Focus(FocusState.Programmatic);
            textBoxMain.Text += Environment.NewLine;
            textBoxMain.SelectionStart = textBoxMain.Text.Length;
            textBoxMain.SelectionLength = 0;
            textBoxMain.PasteFromClipboard();
        }

        private void TrimSpaceBtn_Click(object sender, RoutedEventArgs e)
        {
            textBoxMain.Text = textBoxMain.Text.Trim();
            textBoxMain.Focus(FocusState.Programmatic);
        }

        private async void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            var messageBox = new MessageDialog(@"Developed by Makesh Vineeth
Version 1.0
Copyright © 2022
All Rights Reserved.", "About Notepad");

            await messageBox.ShowAsync();
        }

        private async void ShorcutsMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            var messageBox = new MessageDialog(@"Alt - Show/Hide Menu
Ctrl + O to open file
Ctrl + S to save current file
Ctrl + F to search for text
Ctrl + H for replacing text
Ctrl + G to search in Google
Ctrl + I to insert date/time
Ctrl + U for Upper Case
Ctrl + L for Lower Case", "Shortcuts Guide");

            await messageBox.ShowAsync();
        }

        private async void SaveCurrentBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBoxMain.Text)) return;

                if (openedFile == null)
                {
                    var saveDialog = new Windows.Storage.Pickers.FileSavePicker();
                    saveDialog.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                    saveDialog.SuggestedFileName = "New Text File";
                    StorageFile file = await saveDialog.PickSaveFileAsync();

                    if (file != null)
                    {
                        openedFile = file;
                    }
                }

                if (openedFile == null) return;
                lastSavedText = textBoxMain.Text;
                ApplicationView view = ApplicationView.GetForCurrentView();
                view.Title = openedFile.DisplayName;
                await FileIO.WriteTextAsync(openedFile, textBoxMain.Text);
            }
            catch (Exception)
            {
                var messageBox = new MessageDialog("Error has occurred", "ERROR");
                await messageBox.ShowAsync();
            }
        }

        private async void SaveNewBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBoxMain.Text)) return;
                var saveDialog = new Windows.Storage.Pickers.FileSavePicker();
                saveDialog.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                saveDialog.DefaultFileExtension = ".txt";
                saveDialog.SuggestedFileName = "New Text File";
                StorageFile file = await saveDialog.PickSaveFileAsync();

                if (file != null)
                {
                    await FileIO.WriteTextAsync(openedFile, textBoxMain.Text);
                    openedFile = file;
                    ApplicationView view = ApplicationView.GetForCurrentView();
                    view.Title = openedFile.DisplayName;
                    var messageBox = new MessageDialog("Saved!", "Success");
                    await messageBox.ShowAsync();
                }
            }
            catch (Exception)
            {
                var messageBox = new MessageDialog("Error has occurred", "ERROR");
                await messageBox.ShowAsync();
            }
        }

        private async void SearchTextBtn_Click(object sender, RoutedEventArgs e)
        {
            string text = textBoxMain.Text;
            if (string.IsNullOrWhiteSpace(text)) return;
            text = text.ToLower();

            string val = await ShowAddDialogAsync("Search for any text");
            if (string.IsNullOrWhiteSpace(val)) return;
            previousSearched = val;
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

        public static async Task<string> ShowAddDialogAsync(string title)
        {
            var inputTextBox = new TextBox { AcceptsReturn = false };
            (inputTextBox as FrameworkElement).VerticalAlignment = VerticalAlignment.Bottom;
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
    }
}
