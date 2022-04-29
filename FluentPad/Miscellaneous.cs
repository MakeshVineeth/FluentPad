using System;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace FluentPad
{
    internal class Miscellaneous
    {
        public async void GetStatistics(TextBox textBoxMain)
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
