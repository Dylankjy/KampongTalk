namespace KampongTalk.Models
{
    public class UserPreferences
    {
        // Contains the UserId that owns this preference
        public long UserId { get; set; }

        public bool IsPublic { get; set; } = true;
        public bool UseTts { get; set; } = false;

        // Use 2-letter iso code for this field
        public string Language { get; set; } = "en";

        public int TextSize { get; set; } = 16;

        // This settings govern when an account is considered inactive
        public string DoWhenInactive { get; set; } = "none";
        public int DurationBeforeInactive { get; set; } = 12;
    }
}