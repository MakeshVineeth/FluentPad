using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;


namespace FluentPad
{
    public sealed partial class RootTabView : Page
    {
        private readonly Operations operations;

        public RootTabView()
        {
            InitializeComponent();
            operations = new Operations(null);
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            Window.Current.SetTitleBar(CustomDragRegion);
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
        }

        public void HandleFileActivation(FileActivatedEventArgs fileArgs)
        {
            if (fileArgs.Files.Count > 0)
            {
                var file = (StorageFile)fileArgs.Files[0];
                string strFilePath = file.Path;

                if (!string.IsNullOrWhiteSpace(strFilePath))
                {
                    var newTab = new muxc.TabViewItem
                    {
                        IconSource = new muxc.SymbolIconSource() { Symbol = Symbol.Document },
                        Header = Path.GetFileName(strFilePath)
                    };

                    Frame frame = new Frame();
                    newTab.Content = frame;
                    frame.Navigate(typeof(MainPage), fileArgs);
                    tabView.TabItems.Add(newTab);
                    tabView.SelectedIndex = tabView.TabItems.Count - 1;
                }
            }
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            OnCloseEvents(e);
        }

        private void OnCloseEvents(SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            bool flag = false;

            foreach (TabViewItem item in tabView.TabItems.Cast<TabViewItem>())
            {
                if (CommonUtils.CheckIfUnSaved(item))
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                e.Handled = true;
                operations.CloseApplicationPrompt();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (e.Parameter is IActivatedEventArgs args)
                {
                    if (args.Kind == ActivationKind.File)
                    {
                        if (args is FileActivatedEventArgs fileArgs)
                        {
                            HandleFileActivation(fileArgs);
                        }
                    }
                }
                else
                {
                    TabView_AddTabButtonClick(tabView, null);
                }
            }
            catch (Exception ex)
            {
                CommonUtils.ShowDialog("Error: " + ex.Message, "ERROR");
            }
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (FlowDirection == FlowDirection.LeftToRight)
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayRightInset;
                ShellTitlebarInset.MinWidth = sender.SystemOverlayLeftInset;
            }
            else
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayLeftInset;
                ShellTitlebarInset.MinWidth = sender.SystemOverlayRightInset;
            }

            CustomDragRegion.Height = ShellTitlebarInset.Height = sender.Height;
        }

        private void TabView_AddTabButtonClick(TabView sender, object args)
        {
            var newTab = new TabViewItem
            {
                IconSource = new muxc.SymbolIconSource() { Symbol = Symbol.Document },
                Header = "New Document"
            };

            Frame frame = new Frame();
            newTab.Content = frame;
            frame.Navigate(typeof(MainPage));
            tabView.TabItems.Add(newTab);
            tabView.SelectedIndex = tabView.TabItems.Count - 1;
        }

        private async void HandleTabClose(TabViewItem tabViewItem)
        {
            bool flag = false;

            if (CommonUtils.CheckIfUnSaved(tabViewItem))
            {
                if (await CommonUtils.ShowPromptAsync("File is not saved. Do you still want to close this file?", "Prompt"))
                {
                    flag = true;
                }
            }
            else
            {
                flag = true;
            }

            if (flag)
            {
                tabView.TabItems.Remove(tabViewItem);
            }
        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            HandleTabClose(args.Tab);
        }
    }
}
