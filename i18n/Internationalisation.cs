using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KampongTalk.i18n
{
    public static class Internationalisation
    {
        public static dynamic LoadLanguage (string langCode)
        {
            string json;
            dynamic languageData;
                
            try
            {
                json = File.ReadAllText(@$"{Environment.CurrentDirectory}\i18n\{langCode}.json");
            }
            catch (Exception)
            {
                throw new Exception("Attempted to load a language file that does not exist. Check i18n file's existence and try again.");
            }

            try
            {
                languageData = JObject.Parse(json);
            }
            catch (Exception)
            {
                throw new Exception("Language file cannot be properly parsed. Check for JSON errors.");
            }

            return languageData;
        }
    }
}