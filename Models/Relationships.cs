using System.Collections.Generic;
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

    public static class RelApi
    {
        public static bool IsAccessible(long currentUid, long viewingUid)
        {
            var dbPrefs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "UserPreferences");
            var dbRelation =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Relationships");

            var viewingUserPref = dbPrefs.Single(new {Uid = viewingUid});

            if (viewingUserPref.IsPublic == true || currentUid == viewingUid)
            {
                return true;
            }
            
            // Get friend list
            var friendListRaw =
                dbRelation.Query(
                    $"Select * from Relationships where (UserA = '{viewingUid}' or UserB = '{viewingUid}') AND Status = 'friends'");

            bool isFriends = false;
            List<dynamic> friendList = new List<dynamic>();

            if (friendListRaw != null)
            {
                foreach (var relationRow in friendListRaw)
                {
                    if (relationRow.UserA != viewingUid)
                    {
                        friendList.Add(UserApi.GetUserById(relationRow.UserA));
                    }
                    else
                        friendList.Add(UserApi.GetUserById(relationRow.UserB));
                }

                // Check whether the current user is a friend
                foreach (var row in friendList)
                {
                    if (currentUid == row.Uid)
                    {
                        isFriends = true;
                    }
                }
            }
            
            if (!isFriends)
            {
                return false;
            }

            return true;
        }
    }
}