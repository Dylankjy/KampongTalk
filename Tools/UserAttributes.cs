using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Mighty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KampongTalk.Tools
{
    public interface IUserAttributes
    {
        public string getTextSize();
        public string getTranslateLanguage();
        public string getSpeechLanguage();
        public string getSpeechGender();
    }

    public class UserAttributes: IUserAttributes
    {
        public IHttpContextAccessor _httpContextAccessor;
        public User CurrentUser { get; set; }
        public static MightyOrm dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");
        public static MightyOrm dbPrefs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "UserPreferences");

        public UserAttributes(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            CurrentUser = new User().FromJson(_httpContextAccessor.HttpContext.Session.GetString("CurrentUser"));
        }

        public string getTextSize()
        {
            // We want to retrieve the most updated information. Hence, we take from database, rather than take from the cookie (Which might be outdated)
            if (CurrentUser != null)
            {
                var uid = CurrentUser.Uid;
                dynamic loggedInUserPrefs = dbPrefs.Single($"Uid = {uid}");
                return loggedInUserPrefs.TextSize;
            }
            else
            {
                return "large";
            }
        }

        public string getTranslateLanguage()
        {
            if (CurrentUser != null)
            {
                var uid = CurrentUser.Uid;
                dynamic loggedInUserPrefs = dbPrefs.Single($"Uid = {uid}");
                return loggedInUserPrefs.Language;
            }
            else
            {
                return "en";
            }
        }

        // Must convert from TranslateAPI language code to SpeechAPI langauge code
        public string getSpeechLanguage()
        {
            if (CurrentUser != null)
            {
                var uid = CurrentUser.Uid;
                dynamic loggedInUserPrefs = dbPrefs.Single($"Uid = {uid}");
                switch (loggedInUserPrefs.Language)
                {
                    case "en":
                        return "en-US";
                    case "zh":
                        return "cmn-CN";
                    case "ta":
                        return "ta";
                    case "ms":
                        return "ms";
                    default:
                        return "en-US";
                }
            }
            else
            {
                return "en-US";
            }
        }

        public string getSpeechGender()
        {
            if (CurrentUser != null)
            {
                var uid = CurrentUser.Uid;
                dynamic loggedInUserPrefs = dbPrefs.Single($"Uid = {uid}");
                return loggedInUserPrefs.SpeechGender;
            }
            else
            {
                return "Male";
            }
        }
    }
}
