namespace KampongTalk.Models
{
    public class Relationships
    {
        // Always set UserA as the invoker
        public long UserA { get; set; }

        // UserB shall always be the receiving user
        public long UserB { get; set; }

        // This property governs the relationship between two users
        // "friends" -> Both users are friends
        // "blocked" -> UserA has blocked UserB
        // "pending" -> UserA has sent UserB a friend request, but UserB has yet to accept it
        public string Status { get; set; } = "pending";
    }
}