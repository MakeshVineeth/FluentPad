using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Core;
using System.IO;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.UI.Core.Preview;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Hosting;
using Windows.UI;
using Windows.ApplicationModel.Core;

namespace FluentPad
{
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        private StorageFile openedFile = null;
        private string lastSavedText = string.Empty;
        public static string pattern = " *";
        private readonly DispatcherTimer timer;
        private readonly Operations operations;
        private readonly Miscellaneous miscellaneous;
        private readonly HelpMenu helpMenu;
        private readonly ContextOptions contextOptions;
        private const string autoSavePref = "AutoSaveEnabled";
        private const string menuVisibilityPref = "menuVisibility";


        public MainPage()
        {
            InitializeComponent();

            try
            {
                operations = new Operations(textBoxMain);
                miscellaneous = new Miscellaneous(textBoxMain);
                helpMenu = new HelpMenu();
                contextOptions = new ContextOptions(textBoxMain);

                timer = new DispatcherTimer();
                timer.Tick += Timer_Tick;
                timer.Interval = new TimeSpan(0, 0, 3);

                ToggleMenuVisibility();
                AutoSaveToggle();

                UndoBtn.IsEnabled = false;
                RedoBtn.IsEnabled = false;

                var listener = new ThemeListener();
                listener.ThemeChanged += Listener_ThemeChanged;
                if (ActualTheme == ElementTheme.Light)
                {
                    FixTitleBar();
                }

                // Force dark theme until light theme fixed.
                var root = (FrameworkElement)Window.Current.Content;
                if (root != null)
                    root.RequestedTheme = ElementTheme.Dark;

                SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
            }
            catch (Exception)
            {
            }
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            OnCloseEvents(e);
        }

        private void OnCloseEvents(SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.Title.EndsWith(pattern))
            {
                e.Handled = true;
                operations.CloseApplicationPrompt();
            }
        }

        private void Listener_ThemeChanged(ThemeListener sender)
        {
            FixTitleBar();
        }

        private void FixTitleBar()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            Brush brush = gridTopMenu.Background;
            AcrylicBrush acrylicBrush = (AcrylicBrush)brush;
            titleBar.BackgroundColor = acrylicBrush.TintColor;
            titleBar.ButtonBackgroundColor = acrylicBrush.TintColor;
        }

        private void Timer_Tick(object sender, object e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (autoSaveToggle.IsChecked && view.Title.EndsWith(pattern) && openedFile != null)
                SaveCurrentBtn_Click(null, null);
        }

        private void ToggleMenuVisibility()
        {
            ApplicationDataContainer dataContainer = ApplicationData.Current.LocalSettings;
            string showMenu = dataContainer.Values[menuVisibilityPref]?.ToString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(showMenu) && showMenu.Contains("Visible"))
            {
                gridTopMenu.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrWhiteSpace(showMenu) && showMenu.Contains("Hidden"))
            {
                gridTopMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void AutoSaveToggle()
        {
            ApplicationDataContainer dataContainer = ApplicationData.Current.LocalSettings;
            string autoSaveCheck = dataContainer.Values[autoSavePref]?.ToString() ?? string.Empty;
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (e.Parameter is Windows.ApplicationModel.Activation.IActivatedEventArgs args)
                {
                    if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                    {
                        if (args is Windows.ApplicationModel.Activation.FileActivatedEventArgs fileArgs && fileArgs.Files.Count > 0)
                        {
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
            }
            catch (Exception ex)
            {
                CommonUtils.ShowDialog("Error: " + ex.Message, "ERROR");
            }
        }

        private async void LoadTextFromFile()
        {
            try
            {
                string text = await FileIO.ReadTextAsync(openedFile);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    textBoxMain.Text = text;
                    lastSavedText = textBoxMain.Text;

                    FixAutoSelect();
                }
            }
            catch (Exception)
            {
                CommonUtils.ShowDialog("Error reading the file.", "ERROR");
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
                        textBoxMain.Text = text;
                        lastSavedText = textBoxMain.Text;
                        FixAutoSelect();
                    }
                }
            }
            catch (Exception)
            {
                CommonUtils.ShowDialog("Unable to load file.", "ERROR");
            }
        }

        private void FixAutoSelect() => textBoxMain.SelectionStart = 0;
        private void ExitButton_Click(object sender, RoutedEventArgs e) => helpMenu.ExitApp();

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
        private void PasteButton_Click(object sender, RoutedEventArgs e) => contextOptions.PasteContent();
        private void DeleteButton_Click(object sender, RoutedEventArgs e) => contextOptions.Delete();
        private void UppercaseBtn_Click(object sender, RoutedEventArgs e) => contextOptions.ToUpperCase();
        private void LowercaseBtn_Click(object sender, RoutedEventArgs e) => contextOptions.ToLowerCase();
        private void TitlecaseBtn_Click(object sender, RoutedEventArgs e) => contextOptions.ToSentenceCase();
        private void SearchGoogleBtn_Click(object sender, RoutedEventArgs e) => contextOptions.GoogleSearch();
        private void SelectAllBtn_Click(object sender, RoutedEventArgs e) => operations.SelectAll();
        private void InsertDateTimeBtn_Click(object sender, RoutedEventArgs e) => miscellaneous.InsertDateTime(textBoxMain);
        private void PasteClipboardBtn_Click(object sender, RoutedEventArgs e) => miscellaneous.PasteFromClipboard();
        private void TrimSpaceBtn_Click(object sender, RoutedEventArgs e) => miscellaneous.RemoveSpacesFromTextBox(textBoxMain);
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
                lastSavedText = text;
                ApplicationView view = ApplicationView.GetForCurrentView();

                if (view.Title.EndsWith(pattern))
                    view.Title = view.Title.Replace(pattern, string.Empty);

                view.Title = openedFile.DisplayName;
                await FileIO.WriteTextAsync(openedFile, text);
            }
            catch (Exception)
            {
                CommonUtils.ShowDialog("Error has occurred.", "ERROR");
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

                    CommonUtils.ShowDialog("Saved!", "Success");
                }
            }
            catch (Exception)
            {
                CommonUtils.ShowDialog("Error has occurred.", "ERROR");
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
                    localSettings.Values[menuVisibilityPref] = "Hidden";
                    e.Handled = true;
                }
                else
                {
                    gridTopMenu.Visibility = Visibility.Visible;
                    localSettings.Values[menuVisibilityPref] = "Visible";
                    e.Handled = true;
                }
            }
            else if (isCtrlPressed)
            {
                switch (e.Key)
                {
                    case VirtualKey.O: OpenButton_Click(sender, e); break;
                    case VirtualKey.S: SaveCurrentBtn_Click(sender, e); break;
                    case VirtualKey.F: SearchTextBtn_Click(sender, e); break;
                    case VirtualKey.H: ReplaceBtn_Click(sender, e); break;
                    case VirtualKey.G: SearchGoogleBtn_Click(sender, e); break;
                    case VirtualKey.I: InsertDateTimeBtn_Click(sender, e); break;
                    case VirtualKey.U: UppercaseBtn_Click(sender, e); break;
                    case VirtualKey.L: LowercaseBtn_Click(sender, e); break;
                    case VirtualKey.P: CalculateBtn_Click(sender, e); break;
                    case VirtualKey.K: StatisticsBtn_Click(sender, e); break;
                    case VirtualKey.C:
                        {
                            // Trimming spaces before copying.
                            DataPackage dataPackage = new DataPackage
                            {
                                RequestedOperation = DataPackageOperation.Copy
                            };

                            dataPackage.SetText(miscellaneous.ClearSpacesImplementation(textBoxMain.Text));
                            Clipboard.SetContent(dataPackage);
                            e.Handled = true;
                            break;
                        }
                    default: break;
                }
            }
        }

        private void TextBoxMain_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = textBoxMain.Text;
            int compareInt = string.CompareOrdinal(lastSavedText, text);

            ApplicationView view = ApplicationView.GetForCurrentView();
            if (compareInt != 0)
            {
                if (!view.Title.EndsWith(pattern))
                    view.Title += pattern;
            }
            else if (compareInt == 0 && view.Title.EndsWith(pattern))
            {
                view.Title = view.Title.Replace(pattern, string.Empty);
            }

            UndoBtn.IsEnabled = textBoxMain.CanUndo;
            RedoBtn.IsEnabled = textBoxMain.CanRedo;
        }

        private void AutoSaveToggle_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer dataContainer = ApplicationData.Current.LocalSettings;

            if (autoSaveToggle.IsChecked)
            {
                timer.Start();
                dataContainer.Values[autoSavePref] = "True";
            }
            else
            {
                timer.Stop();
                dataContainer.Values[autoSavePref] = "False";
            }
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e) => contextOptions.Undo();
        private void RedoBtn_Click(object sender, RoutedEventArgs e) => contextOptions.Redo();
        private void CalculateBtn_Click(object sender, RoutedEventArgs e) => contextOptions.Calculate();
        private void StatisticsBtn_Click(object sender, RoutedEventArgs e) => miscellaneous.GetStatistics();

        private void CleanCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            miscellaneous.CleanCode(textBoxMain);
        }

        private void OpenDirectUrlBtn_Click(object sender, RoutedEventArgs e)
        {
            contextOptions.OpenDirectUrl();
        }

        private async Task SavePDFAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBoxMain.Text)) return;
                var saveDialog = new Windows.Storage.Pickers.FileSavePicker();
                saveDialog.FileTypeChoices.Add("PDF File", new List<string>() { ".pdf" });
                saveDialog.DefaultFileExtension = ".pdf";
                saveDialog.SuggestedFileName = "PDF_File";
                StorageFile file = await saveDialog.PickSaveFileAsync();
                PdfDocumentBuilder builder = new PdfDocumentBuilder();
                PdfPageBuilder page = builder.AddPage(PageSize.A4);
                PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);

                string[] lines = textBoxMain.Text.Split(
                                new string[] { "\r\n", "\r", "\n" },
                                StringSplitOptions.None
                                );

                int x_axis = 30;
                int y_axis = 800;
                foreach (string line in lines)
                {
                    if (y_axis <= 40)
                    {
                        page = builder.AddPage(PageSize.A4);
                        y_axis = 800;
                    }

                    page.AddText(line, 12, new PdfPoint(x_axis, y_axis), font);
                    y_axis -= 15;
                }

                byte[] documentBytes = builder.Build();
                await FileIO.WriteBytesAsync(file, documentBytes);
                CommonUtils.ShowDialog("Saved!", "Success");
            }
            catch (Exception ex)
            {
                CommonUtils.ShowDialog("Error: " + ex.Message, "ERROR");
            }
        }

        private void SavePdfButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SavePDFAsync();
        }

        private void EncodeBtn_Click(object sender, RoutedEventArgs e)
        {
            contextOptions.UrlEncode();
        }

        private void DefineBtn_Click(object sender, RoutedEventArgs e)
        {
            contextOptions.GetWordMeaning();
        }

        private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNewWindowAsync();
        }

        private async void ShowNewWindowAsync()
        {
            try
            {
                CoreApplicationView newView = CoreApplication.CreateNewView();
                int newViewId = 0;
                await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Frame frame = new Frame();
                    frame.Navigate(typeof(MainPage), null);
                    Window.Current.Content = frame;
                    Window.Current.Activate();

                    newViewId = ApplicationView.GetForCurrentView().Id;
                });

                bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
            }
            catch (Exception)
            {
            }
        }

        private void CharMapBtn_Click(object sender, RoutedEventArgs e)
        {
            operations.LaunchCharMapAsync();
        }
    }
}
