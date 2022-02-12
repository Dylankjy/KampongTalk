using KampongTalk.i18n;
using Mighty;

namespace KampongTalk.Models
{
    public class UserPreferences
    {
        // Contains the Uid that owns this preference
        public long Uid { get; set; }

        public bool IsPublic { get; set; } = true;
        public bool UseAudioCues { get; set; } = false;

        // Login preference
        public bool UsePasswordLess { get; set; } = false;

        // Notification options
        public bool NotifyTaggedAndReplies { get; set; } = true;
        public bool NotifyLikes { get; set; } = true;
        public bool NotifyFriendRequest { get; set; } = true;
        public bool NotifyChat { get; set; } = true;

        // This settings govern when an account is considered inactive
        public string DoWhenInactive { get; set; } = "none";
        public int DurationBeforeInactive { get; set; } = 12;


        public string TextSize { get; set; } = "large";
        public string Language { get; set; } = "en";
        public string SpeechGender { get; set; } = "Male";

        public UserPreferences ToUserPreferences(dynamic obj)
        {
            if (obj == null) return new UserPreferences { Uid = -1 };

            return new UserPreferences
            {
                Uid = obj.Uid,
                IsPublic = obj.IsPublic,
                UseAudioCues = obj.UseAudioCues,
                UsePasswordLess = obj.UsePasswordLess,
                NotifyTaggedAndReplies = obj.NotifyTaggedAndReplies,
                NotifyLikes = obj.NotifyLikes,
                NotifyFriendRequest = obj.NotifyFriendRequest,
                NotifyChat = obj.NotifyChat,
                DoWhenInactive = obj.DoWhenInactive,
                DurationBeforeInactive = obj.DurationBeforeInactive,
                TextSize = obj.TextSize,
                Language = obj.Language,
                SpeechGender = obj.SpeechGender
            };
        }
    }

    public static class UserPrefApi
    {
        public static dynamic GetLangByUid(dynamic userObj)
        {
            if (userObj == null)
            {
                return Internationalisation.LoadLanguage("en");
            }
            
            var dbPrefs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "UserPreferences");
            
            // Set user language
            var userPref = dbPrefs.Single(new { Uid = userObj.Uid });
            if (userPref != null)
            {
                return Internationalisation.LoadLanguage(userPref.Language);
            }
            return Internationalisation.LoadLanguage("en");
        }
    }
}