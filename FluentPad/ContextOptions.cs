using org.mariuszgromada.math.mxparser;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using Windows.Data.Json;
using Windows.System;
using Windows.UI.Xaml.Controls;
using System.Text.Json;
using System.Collections.Generic;

namespace FluentPad
{
    internal class ContextOptions
    {
        private readonly TextBox textBoxMain;
        public const string UserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";

        public ContextOptions(TextBox textBoxMain)
        {
            this.textBoxMain = textBoxMain;
        }

        public void Calculate()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBoxMain.Text)) return;
                string expression = textBoxMain.SelectedText;

                if (!string.IsNullOrWhiteSpace(expression))
                {
                    Expression expression1 = new Expression(expression);
                    double result = expression1.calculate();
                    if (double.IsNaN(result) || double.IsInfinity(result))
                    {
                        CommonUtils.ShowDialog("Unable to calculate the expression!", "ERROR");
                    }
                    else
                    {
                        CommonUtils.ShowDialog(result.ToString(), "ANSWER");
                    }
                }
            }
            catch (Exception)
            {
                CommonUtils.ShowDialog("Error has occurred.", "ERROR");
            }
        }

        public async void GenericWebSearch(string endPoint)
        {
            try
            {
                string text = textBoxMain.SelectedText.Trim();
                if (string.IsNullOrWhiteSpace(text)) return;

                bool result = Uri.TryCreate(text, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!result)
                {
                    string url = endPoint + WebUtility.UrlEncode(text);
                    uriResult = new Uri(url);
                }

                bool success = await Launcher.LaunchUriAsync(uriResult);

                if (!success)
                {
                    CommonUtils.ShowDialog("Oops, unable to search.", "FAILED TO OPEN URL");
                }
            }
            catch (Exception ex)
            {
                CommonUtils.ShowDialog("Oops, an error has occurred: " + ex.Message, "ERROR");
            }
        }

        public void OpenDirectUrl()
        {
            string endPointUrl = "https://duckduckgo.com/?q=" + WebUtility.UrlEncode("\\");
            GenericWebSearch(endPointUrl);
        }

        public void GoogleSearch()
        {
            string google = "https://www.google.com/search?q=";
            GenericWebSearch(google);
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

        public void UrlEncode()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBoxMain.SelectedText))
                {
                    return;
                }

                textBoxMain.SelectedText = WebUtility.UrlEncode(textBoxMain.SelectedText);
            }
            catch (Exception)
            {
            }
        }

        public async void GetWordMeaning()
        {
            if (string.IsNullOrWhiteSpace(textBoxMain.SelectedText))
            {
                return;
            }

            string text = textBoxMain.SelectedText;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgentString);
                    string url = @"https://api.dictionaryapi.dev/api/v2/entries/en/" + text;
                    string response = await httpClient.GetStringAsync(url);

                    if (string.IsNullOrWhiteSpace(response)) { return; }

                    List<Root> root_list = JsonSerializer.Deserialize<List<Root>>(response, options: new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    string meaning_text = string.Empty;

                    foreach (Root root_obj in root_list)
                    {
                        foreach (Meaning meaning_obj in root_obj.Meanings)
                        {
                            string parts_of_speech = ToTitleCase(meaning_obj.PartOfSpeech);

                            foreach (DefinitionClass definition_obj in meaning_obj.Definitions)
                            {
                                if (definition_obj.Example != null && !string.IsNullOrWhiteSpace(definition_obj.Example))
                                {
                                    meaning_text += string.Format("({0}) {1}\n\"{2}\"\n\n", parts_of_speech, definition_obj.Definition, definition_obj.Example);
                                }
                                else
                                {
                                    meaning_text += string.Format("({0}) {1}\n\n", parts_of_speech, definition_obj.Definition);
                                }
                            }
                        }
                    }

                    CommonUtils.ShowDialog(meaning_text, "Define " + ToTitleCase(text));
                }
            }
            catch (Exception)
            {
                CommonUtils.ShowDialog("Could not look up for the word!", "ERROR");
            }
        }

        public string ToTitleCase(string text)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(text.ToLower());
        }

        public void ToSentenceCase()
        {
            if (string.IsNullOrWhiteSpace(textBoxMain.SelectedText))
            {
                return;
            }

            textBoxMain.SelectedText = ToTitleCase(textBoxMain.SelectedText);
        }
    }
}
