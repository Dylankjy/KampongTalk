namespace KampongTalk.Models
{
    public class UserPreferences
    {
        // Contains the Uid that owns this preference
        public long Uid { get; set; }

        public bool IsPublic { get; set; } = true;
        public bool UseTts { get; set; } = false;
        public bool UseAudioCues { get; set; } = false;

        // Login preference
        public bool UsePasswordLess { get; set; } = false;

        // Use 2-letter iso code for this field
        public string Language { get; set; } = "en";

        public int TextSize { get; set; } = 16;

        // Notification options
        public bool NotifyTaggedAndReplies { get; set; } = true;
        public bool NotifyLikes { get; set; } = true;
        public bool NotifyFriendRequest { get; set; } = true;
        public bool NotifyChat { get; set; } = true;

        // This settings govern when an account is considered inactive
        public string DoWhenInactive { get; set; } = "none";
        public int DurationBeforeInactive { get; set; } = 12;
    }
}