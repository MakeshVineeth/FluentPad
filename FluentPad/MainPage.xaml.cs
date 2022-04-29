using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Core;
using System.IO;
using Windows.UI.Xaml.Navigation;

namespace FluentPad
{
    public sealed partial class MainPage : Page
    {
        private StorageFile openedFile = null;
        private string lastSavedText = string.Empty;
        private const string pattern = " *";
        private readonly DispatcherTimer timer;
        private readonly Operations operations;
        private readonly Miscellaneous miscellaneous;
        private readonly HelpMenu helpMenu;
        private readonly ContextOptions contextOptions;


        public MainPage()
        {
            InitializeComponent();

            operations = new Operations(textBoxMain);
            miscellaneous = new Miscellaneous(textBoxMain);
            helpMenu = new HelpMenu();
            contextOptions = new ContextOptions(textBoxMain);

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 3);

            ApplicationDataContainer dataContainer = ApplicationData.Current.LocalSettings;
            string showMenu = dataContainer.Values["menuVisibility"]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(showMenu) && showMenu.Contains("Visible"))
            {
                gridTopMenu.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrWhiteSpace(showMenu) && showMenu.Contains("Hidden"))
            {
                gridTopMenu.Visibility = Visibility.Collapsed;
            }

            string autoSaveCheck = dataContainer.Values["AutoSaveEnabled"]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(autoSaveCheck))
            {
                if (autoSaveCheck.Contains("True"))
                {
                    autoSaveToggle.IsChecked = true;
                    timer.Start();
                }
                else if (autoSaveCheck.Contains("False"))
                {
                    autoSaveToggle.IsChecked = false;
                }
            }

            UndoBtn.IsEnabled = false;
            RedoBtn.IsEnabled = false;
        }

        private void Timer_Tick(object sender, object e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (autoSaveToggle.IsChecked && view.Title.EndsWith(pattern) && openedFile != null)
                SaveCurrentBtn_Click(null, null);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (e.Parameter is Windows.ApplicationModel.Activation.IActivatedEventArgs args)
                {
                    if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                    {
                        var fileArgs = args as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                        var file = (StorageFile)fileArgs.Files[0];
                        string strFilePath = file.Path;

                        if (!string.IsNullOrWhiteSpace(strFilePath))
                        {
                            openedFile = file;
                            var appView = ApplicationView.GetForCurrentView();
                            appView.Title = Path.GetFileName(strFilePath);
                            LoadTextFromFile();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msgBox = new MessageDialog("Error: " + ex.Message, "ERROR");
                _ = msgBox.ShowAsync();
            }
        }

        private async void LoadTextFromFile()
        {
            string text = await FileIO.ReadTextAsync(openedFile);

            if (!string.IsNullOrWhiteSpace(text))
            {
                lastSavedText = text.Replace("\n", string.Empty);
                textBoxMain.Text = text;
                FixAutoSelect();
            }
        }

        private void FileMenuButton_Click(object sender, RoutedEventArgs e) => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private void OperationsBtn_Click(object sender, RoutedEventArgs e) => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private void MiscButton_Click(object sender, RoutedEventArgs e) => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private void HelpButton_Click(object sender, RoutedEventArgs e) => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

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
                        lastSavedText = text.Replace("\n", string.Empty); // TextBox using \r as newlines instead of \n.
                        textBoxMain.Text = text;
                        FixAutoSelect();
                    }
                }
            }
            catch (Exception)
            {
                var messageBox = new MessageDialog("Unable to load file.", "ERROR");
                await messageBox.ShowAsync();
            }
        }

        private void FixAutoSelect() => textBoxMain.SelectionStart = 0;

        private void ExitButton_Click(object sender, RoutedEventArgs e) => Application.Current.Exit();

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            lastSavedText = string.Empty;
            textBoxMain.Text = string.Empty;
            openedFile = null;

            ApplicationView view = ApplicationView.GetForCurrentView();
            view.Title = string.Empty;
        }

        private void CutButton_Click(object sender, RoutedEventArgs e) => contextOptions.CutText();
        private void CopyButton_Click(object sender, RoutedEventArgs e) => contextOptions.CopyText();

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxMain.CanPasteClipboardContent)
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

        private void SearchGoogleBtn_Click(object sender, RoutedEventArgs e) => contextOptions.GoogleSearch();

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e) => operations.SelectAll();

        private void InsertDateTimeBtn_Click(object sender, RoutedEventArgs e) => miscellaneous.InsertDateTime(textBoxMain);

        private void PasteClipboardBtn_Click(object sender, RoutedEventArgs e) => miscellaneous.PasteFromClipboard();

        private void TrimSpaceBtn_Click(object sender, RoutedEventArgs e) => miscellaneous.RemoveSpaces(textBoxMain);

        private void AboutBtn_Click(object sender, RoutedEventArgs e) => helpMenu.ShowAbout();

        private void ShorcutsMenuBtn_Click(object sender, RoutedEventArgs e) => helpMenu.ShowHelp();

        private async void SaveCurrentBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

                string text = textBoxMain.Text;
                lastSavedText = text.Replace("\n", string.Empty); // TextBox using \r as newlines instead of \n.
                ApplicationView view = ApplicationView.GetForCurrentView();

                if (view.Title.EndsWith(pattern))
                    view.Title = view.Title.Replace(pattern, string.Empty);

                view.Title = openedFile.DisplayName;
                await FileIO.WriteTextAsync(openedFile, text);
            }
            catch (Exception)
            {
                var messageBox = new MessageDialog("Error has occurred.", "ERROR");
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
                var messageBox = new MessageDialog("Error has occurred.", "ERROR");
                await messageBox.ShowAsync();
            }
        }

        private void SearchTextBtn_Click(object sender, RoutedEventArgs e) => operations.SearchText();
        private void ReplaceBtn_Click(object sender, RoutedEventArgs e) => operations.Replace();

        private void TextBoxMain_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            bool isCtrlPressed = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            if (e.Key == VirtualKey.Menu)
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (gridTopMenu.Visibility == Visibility.Visible)
                {
                    gridTopMenu.Visibility = Visibility.Collapsed;
                    localSettings.Values["menuVisibility"] = "Hidden";
                    e.Handled = true;
                }
                else
                {
                    gridTopMenu.Visibility = Visibility.Visible;
                    localSettings.Values["menuVisibility"] = "Visible";
                    e.Handled = true;
                }
            }
            else if (isCtrlPressed && e.Key == VirtualKey.O)
            {
                OpenButton_Click(sender, e);
            }
            else if (isCtrlPressed && e.Key == VirtualKey.S)
            {
                SaveCurrentBtn_Click(sender, e);
            }
            else if (isCtrlPressed && e.Key == VirtualKey.F)
            {
                SearchTextBtn_Click(sender, e);
            }
            else if (isCtrlPressed && e.Key == VirtualKey.H)
            {
                ReplaceBtn_Click(sender, e);
            }
            else if (isCtrlPressed && e.Key == VirtualKey.G)
            {
                SearchGoogleBtn_Click(sender, e);
            }
            else if (isCtrlPressed && e.Key == VirtualKey.I)
            {
                InsertDateTimeBtn_Click(sender, e);
            }
            else if (isCtrlPressed && e.Key == VirtualKey.U)
            {
                UppercaseBtn_Click(sender, e);
            }
            else if (isCtrlPressed && e.Key == VirtualKey.L)
            {
                LowercaseBtn_Click(sender, e);
            }
            else if (isCtrlPressed && e.Key == VirtualKey.P)
            {
                CalculateBtn_Click(sender, e);
            }
            else if (isCtrlPressed && e.Key == VirtualKey.K)
            {
                StatisticsBtn_Click(sender, e);
            }
        }

        private void TextBoxMain_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            string text = textBoxMain.Text;
            int compareInt = string.CompareOrdinal(lastSavedText, text);

            if (compareInt != 0)
            {
                if (!view.Title.EndsWith(pattern))
                    view.Title += pattern;
            }
            else if (compareInt == 0 && view.Title.EndsWith(pattern))
            {
                view.Title = view.Title.Replace(pattern, string.Empty);
            }

            if (textBoxMain.CanUndo)
            {
                UndoBtn.IsEnabled = true;
            }
            else
            {
                UndoBtn.IsEnabled = false;
            }

            if (textBoxMain.CanRedo)
            {
                RedoBtn.IsEnabled = true;
            }
            else
            {
                RedoBtn.IsEnabled = false;
            }
        }

        private void AutoSaveToggle_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer dataContainer = ApplicationData.Current.LocalSettings;

            if (autoSaveToggle.IsChecked)
            {
                timer.Start();
                dataContainer.Values["AutoSaveEnabled"] = "True";
            }
            else
            {
                timer.Stop();
                dataContainer.Values["AutoSaveEnabled"] = "False";
            }
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxMain.CanUndo)
                textBoxMain.Undo();
        }

        private void RedoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxMain.CanRedo)
                textBoxMain.Redo();
        }

        private void CalculateBtn_Click(object sender, RoutedEventArgs e) => contextOptions.Calculate();
        private void StatisticsBtn_Click(object sender, RoutedEventArgs e) => miscellaneous.GetStatistics();
    }
}
