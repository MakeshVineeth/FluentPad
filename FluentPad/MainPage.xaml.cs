using System;
using System.IO;
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
                var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
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
                    appView.Title = "Fluent Pad - " + file.DisplayName;
                    string text = await Windows.Storage.FileIO.ReadTextAsync(file);
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
    }
}
