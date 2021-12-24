using Google.Cloud.Translation.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KampongTalk.Tools
{
    public interface ITranslator
    {
        string TranslateText(string text, string language);
        string TranslateHtml(string text, string language);
    }

    public class Translator : ITranslator
    {
        private readonly TranslationClient client;

        public Translator()
        {
            client = TranslationClient.Create();
        }

        public string TranslateHtml(string text, string language)
        {
            var resp = client.TranslateHtml(text, language);
            return resp.TranslatedText;
        }

        public string TranslateText(string text, string language)
        {
            var resp = client.TranslateText(text, language);
            return resp.TranslatedText;
        }
    }
}
