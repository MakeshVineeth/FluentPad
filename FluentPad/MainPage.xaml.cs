using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
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
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;

namespace FluentPad
{
    public sealed partial class MainPage : Page
    {
        StorageFile openedFile = null;
        string lastSavedText = string.Empty;
        const string pattern = " *";
        readonly DispatcherTimer timer;

        public MainPage()
        {
            InitializeComponent();
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

        private async void SearchGoogleBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = textBoxMain.SelectedText.Trim();
                if (string.IsNullOrWhiteSpace(text)) return;
                string google = "https://www.google.com/search?q=";
                string url = google + WebUtility.UrlEncode(text);
                var urlObject = new Uri(url);
                bool success = await Launcher.LaunchUriAsync(urlObject);

                if (!success)
                {
                    var messageBox = new MessageDialog("Oops, unable to search.", "FAILED TO OPEN URL");
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
            if (textBoxMain.CanPasteClipboardContent)
            {
                textBoxMain.Focus(FocusState.Programmatic);
                textBoxMain.Text += Environment.NewLine;
                textBoxMain.SelectionStart = textBoxMain.Text.Length;
                textBoxMain.SelectionLength = 0;
                textBoxMain.PasteFromClipboard();
            }
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

        private async void SearchTextBtn_Click(object sender, RoutedEventArgs e)
        {
            string text = textBoxMain.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                var msgBox = new MessageDialog("No text found!", "Unable to search");
                await msgBox.ShowAsync();
                return;
            }

            text = text.ToLower();

            string val = await ShowAddDialogAsync("Search for any text");
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

        public static async Task<string> ShowAddDialogAsync(string title)
        {
            var inputTextBox = new TextBox { AcceptsReturn = false };
            inputTextBox.VerticalAlignment = VerticalAlignment.Bottom;
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

        private async void ReplaceBtn_Click(object sender, RoutedEventArgs e)
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
                        textBoxMain.SelectionLength = 0;
                        textBoxMain.SelectionStart = 0;
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

        private async void CalculateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBoxMain.Text)) return;
                string expression = textBoxMain.SelectedText;

                if (!string.IsNullOrWhiteSpace(expression))
                {
                    var result = new DataTable().Compute(expression, null);
                    if (result == DBNull.Value)
                    {
                        var msgBox = new MessageDialog("Unable to calculate the expression!", "ERROR");
                        await msgBox.ShowAsync();
                    }
                    else
                    {
                        var msgBox = new MessageDialog(result.ToString(), "ANSWER");
                        await msgBox.ShowAsync();
                    }
                }
            }
            catch (Exception)
            {
                var msgBox = new MessageDialog("Error has occurred.", "ERROR");
                await msgBox.ShowAsync();
            }
        }

        private async void StatisticsBtn_Click(object sender, RoutedEventArgs e)
        {
            string selectedText = textBoxMain.Text;
            if (!string.IsNullOrWhiteSpace(selectedText))
            {
                int lines = Regex.Matches(selectedText, "\r", RegexOptions.Multiline).Count + 1;
                char[] delimiters = new char[] { ' ', '\r', '\n' };
                int words = selectedText.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
                int charsNoSpaces = selectedText.Count(c => !char.IsWhiteSpace(c));
                int paragraphs = Regex.Matches(selectedText, "[^\r\n]+((\r|\n|\r\n)[^\r\n]+)*").Count;
                int specialCharacters = Regex.Matches(selectedText, "[~!@#$%^&*()_+{}:\"<>?]").Count;
                int numbers = selectedText.Count(c => char.IsDigit(c));

                string message = "Lines: " + lines + "\r" + "Words: " + words + "\r" + "Characters without spaces: " + charsNoSpaces + "\r" + "Characters with spaces: " + selectedText.Length
                    + "\r" + "Paragraphs: " + paragraphs + "\r" + "Special Characters: " + specialCharacters
                    + "\r" + "Digits: " + numbers + "\r\r\r" + "(Not all information that is shown above may be accurate)";

                var msgBox = new MessageDialog(message, "Statistics");
                await msgBox.ShowAsync();
            }
            else
            {
                var msgBox = new MessageDialog("No text found!", "Statistics");
                await msgBox.ShowAsync();
            }
        }
    }
}
