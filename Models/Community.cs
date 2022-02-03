using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IdGen;
using Mighty;

namespace KampongTalk.Models
{
    public class Community
    {
        public string Cid { get; set; }
        [Required] public string Name { get; set; }
        public long CreatorId { get; set; }
        public string IconImg { get; set; } = "default_community.png";
        [Required, MaxLength(255), MinLength(20)] public string Description { get; set; }
        public DateTime TimeCreated { get; set; } = DateTime.Now;

        public void SetCid()
        {
            Cid = $"{Name.Replace(" ", "").ToLower()}";
        }
    }

    public static class CommunityApi
    {
        public static dynamic GetCommunityById(string cid)
        {
            // Database declarations
            var dbCommunities =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Communities");

            return dbCommunities.Single(new
            {
                Cid = cid
            });
        }

        public static int GetPostCountByCid(string cid)
        {
            // Database declarations
            var dbPost =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Post");

            return dbPost.All(new
            {
                InCommunity = cid
            }).ToList().Count;
        }
    }
}