using System;
using System.Globalization;
using System.Net;
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
    }
}
