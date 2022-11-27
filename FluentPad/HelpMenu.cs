using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentPad
{
    internal class HelpMenu
    {
        public void ShowHelp()
        {
            CommonUtils.ShowDialog(@"Alt - Show/Hide Menu
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
        }

        public void ShowAbout()
        {
            CommonUtils.ShowDialog(@"Developed by Makesh Vineeth
Version 1.0
Copyright © 2022
All Rights Reserved.", "About Notepad");
        }

        public void ExitApp() => Application.Current.Exit();
    }
}
