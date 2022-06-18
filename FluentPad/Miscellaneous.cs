using System;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentPad
{
    internal class Miscellaneous
    {
        private readonly TextBox textBoxMain;

        public Miscellaneous(TextBox textBoxMain)
        {
            this.textBoxMain = textBoxMain;
        }

        public async void GetStatistics()
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

        public void InsertDateTime(TextBox textBoxMain)
        {
            textBoxMain.Focus(FocusState.Programmatic);
            DateTime now = DateTime.Now;
            string date_time = now.ToString();
            textBoxMain.Text += " " + date_time;
            textBoxMain.SelectionStart = textBoxMain.Text.Length;
            textBoxMain.SelectionLength = 0;
        }

        public void CleanCode(TextBox textBoxMain)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(textBoxMain.Text))
                {
                    textBoxMain.Focus(FocusState.Programmatic);
                    string text = textBoxMain.Text;
                    string pattern1 = "/*";
                    string pattern2 = "*/";

                    int start = text.IndexOf(pattern1);
                    while (start >= 0)
                    {
                        int end = text.IndexOf(pattern2);
                        text = text.Remove(start, end - start + pattern1.Length);
                        start = text.IndexOf(pattern1);
                    }

                    pattern1 = "//";
                    pattern2 = "\n";
                    start = text.IndexOf(pattern1);

                    while (start >= 0)
                    {
                        int end = text.IndexOf(pattern2, start);
                        text = text.Remove(start, end - start);
                        start = text.IndexOf(pattern1);
                    }

                    text = text.Trim();
                    text = text.Replace("class GFG", "class DSA");
                    text = text.Replace("class GfG", "class DSA");
                    text = text.Replace("  ", "\n");
                    text = Regex.Replace(text, @"[\r\n]+", "\n");

                    textBoxMain.Text = text;
                    textBoxMain.SelectionStart = textBoxMain.Text.Length;
                    textBoxMain.SelectionLength = 0;
                }
            }
            catch (Exception)
            {
            }
        }

        public void RemoveSpaces(TextBox textBoxMain)
        {
            textBoxMain.Text = textBoxMain.Text.Trim();
            textBoxMain.Focus(FocusState.Programmatic);
        }

        public void PasteFromClipboard()
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
    }
}
