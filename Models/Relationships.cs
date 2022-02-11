using System.ComponentModel.DataAnnotations;
using IdGen;
using Mighty;

namespace KampongTalk.Models
{
    [DatabaseTable("Relationships")]
    public class Relationships
    {
        [DatabasePrimaryKey] [DatabaseColumn] public long Rid { get; set; } = new IdGenerator(8).CreateId();
        // Always set UserA as the invoker
        [DatabaseColumn] [Required] public long UserA { get; set; }

        // UserB shall always be the receiving user
        [DatabaseColumn] [Required] public long UserB { get; set; }

        // This property governs the relationship between two users
        // "friends" -> Both users are friends
        // "blocked" -> UserA has blocked UserB
        // "pending" -> UserA has sent UserB a friend request, but UserB has yet to accept it
        [DatabaseColumn] [Required] public string Status { get; set; } = "pending";
    }
}