﻿using org.mariuszgromada.math.mxparser;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using Windows.Data.Json;
using Windows.System;
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
                    string url = @"https://api.dictionaryapi.dev/api/v2/entries/en/" + text;
                    string response = await httpClient.GetStringAsync(url);

                    if (string.IsNullOrWhiteSpace(response)) { return; }

                    response = response.Remove(0, 1);
                    response = response.Remove(response.Length - 1, 1);
                    JsonObject obj = JsonObject.Parse(response);
                    IJsonValue def = obj["meanings"];
                    JsonArray arr = def.GetArray();
                    string meaning = string.Empty;

                    foreach (var each_item in arr)
                    {
                        JsonObject item1 = each_item.GetObject();
                        string parts_of_speech = item1.GetNamedString("partOfSpeech");
                        parts_of_speech = ToTitleCase(parts_of_speech);

                        JsonArray values = item1.GetNamedArray("definitions");
                        foreach (var each_sub_value in values)
                        {
                            JsonObject obj2 = each_sub_value.GetObject();

                            string definition = obj2.GetNamedString("definition");
                            string example = string.Empty;

                            if (obj2.ContainsKey("example"))
                            {
                                example = obj2.GetNamedString("example");
                                meaning += string.Format("({0}) {1}\n\"{2}\"\n\n", parts_of_speech, definition, example);
                            }
                            else
                            {
                                meaning += string.Format("({0}) {1}\n\n", parts_of_speech, definition);
                            }
                        }
                    }

                    CommonUtils.ShowDialog(meaning, "Define " + ToTitleCase(text));
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
