using System;
using System.Data;
using System.Globalization;
using System.Net;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace FluentPad
{
    internal class ContextOptions
    {
        private readonly TextBox textBoxMain;

        public ContextOptions(TextBox textBoxMain)
        {
            this.textBoxMain = textBoxMain;
        }

        public async void Calculate()
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

        public async void GoogleSearch()
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

        public void PasteContent()
        {
            if (textBoxMain.CanPasteClipboardContent)
                textBoxMain.PasteFromClipboard();
        }

        public void CopyText() => textBoxMain.CopySelectionToClipboard();
        public void CutText() => textBoxMain.CutSelectionToClipboard();
        public void Delete() => textBoxMain.SelectedText = string.Empty;
        public void ToUpperCase() => textBoxMain.SelectedText = textBoxMain.SelectedText.ToUpper();
        public void ToLowerCase() => textBoxMain.SelectedText = textBoxMain.SelectedText.ToLower();

        public void Undo()
        {
            if (textBoxMain.CanUndo)
                textBoxMain.Undo();
        }

        public void Redo()
        {
            if (textBoxMain.CanRedo)
                textBoxMain.Redo();
        }

        public void ToSentenceCase()
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            textBoxMain.SelectedText = ti.ToTitleCase(textBoxMain.SelectedText.ToLower());
        }
    }
}
