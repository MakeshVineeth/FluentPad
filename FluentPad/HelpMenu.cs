using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentPad
{
    internal class HelpMenu
    {
        public async void ShowHelp()
        {
            var messageBox = new ContentDialog { Content = @"Alt - Show/Hide Menu
Ctrl + O to open file
Ctrl + S to save current file
Ctrl + F to search for text
Ctrl + H for replacing text
Ctrl + G to search in Google
Ctrl + I to insert date/time
Ctrl + U for Upper Case
Ctrl + L for Lower Case
Ctrl + P for Calculating
Ctrl + K for Statistics", Title = "Shortcuts Guide" };

            await messageBox.ShowAsync();
        }

        public async void ShowAbout()
        {
            var messageBox = new ContentDialog { Content = @"Developed by Makesh Vineeth
Version 1.0
Copyright © 2022
All Rights Reserved.", Title = "About Notepad", CloseButtonText = "Ok" };

            await messageBox.ShowAsync();
        }

        public void ExitApp() => Application.Current.Exit();
    }
}
