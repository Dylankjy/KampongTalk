using System;
using System.Linq;
using Mighty;

namespace KampongTalk.Models
{
    public class Likes
    {
        public long EntityId { get; set; }
        public long Uid { get; set; }
    }

    public static class LikesApi
    {
        public static int GetLikesByPid(long pid)
        {
            var dbLikes = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Likes");
            return dbLikes.All(new { EntityId = pid }).ToList().Count;
        }

        public static bool IsLiked(long pid, long uid)
        {
            var dbLikes = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Likes");
            var isLiked = dbLikes.Count($"EntityId = '{pid}' && Uid = '{uid}'");
            if (isLiked.ToString() == "1")
            {
                return true;
            }
            return false;
        }
    }
}