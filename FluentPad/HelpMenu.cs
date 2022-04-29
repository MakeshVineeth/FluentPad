using System;
using Windows.UI.Popups;

namespace FluentPad
{
    internal class HelpMenu
    {
        public async void ShowHelp()
        {
            var messageBox = new MessageDialog(@"Alt - Show/Hide Menu
Ctrl + O to open file
Ctrl + S to save current file
Ctrl + F to search for text
Ctrl + H for replacing text
Ctrl + G to search in Google
Ctrl + I to insert date/time
Ctrl + U for Upper Case
Ctrl + L for Lower Case
Ctrl + P for Calculating
Ctrl + K for Statistics", "Shortcuts Guide");

            await messageBox.ShowAsync();
        }

        public async void ShowAbout()
        {
            var messageBox = new MessageDialog(@"Developed by Makesh Vineeth
Version 1.0
Copyright © 2022
All Rights Reserved.", "About Notepad");

            await messageBox.ShowAsync();
        }
    }
}
