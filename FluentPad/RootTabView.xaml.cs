using System;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;


namespace FluentPad
{
    public sealed partial class RootTabView : Page
    {
        public RootTabView()
        {
            InitializeComponent();
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            Window.Current.SetTitleBar(CustomDragRegion);
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

        private void TabView_AddTabButtonClick(muxc.TabView sender, object args)
        {
            var newTab = new muxc.TabViewItem
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

        private void TabView_TabCloseRequested(muxc.TabView sender, muxc.TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
        }
    }
}
