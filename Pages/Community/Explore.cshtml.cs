using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Community
{
    public class Explore : PageModel
    {
        // Communities to display
        public dynamic CommToDisplay { get; set; }
        public dynamic CommToDisplayByPopularity { get; set; }

        // Database declarations
        public static MightyOrm dbCommunities { get; set; } =
           new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Communities");

        // Current user prop
        public User CurrentUser { get; set; }

        public dynamic myCommunity { get; set; }

        public void OnGet(int p)
        {
            // Database declarations
            //var dbCommunities =
            //    new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
            //        "Communities");

            // Get posts by this user from database
            var communities = dbCommunities.All().Reverse();

            var numberOfObjectsPerPage = 10;
            CommToDisplay = communities.ToList().Skip(numberOfObjectsPerPage * p)
                .Take(numberOfObjectsPerPage);

            var communityByPopularity = new List<CommunityPopular>();

            foreach (var community in CommToDisplay)
            {
                Console.WriteLine(community.Cid);
                communityByPopularity.Add(new CommunityPopular
                {
                    Cid = community.Cid,
                    Name = community.Name,
                    CreatorId = community.CreatorId,
                    IconImg = community.IconImg,
                    Description = community.Description,
                    TimeCreated = community.TimeCreated,
                    NumberOfPosts = CommunityApi.GetPostCountByCid(community.Cid)
                });
            }

            CommToDisplayByPopularity = communityByPopularity.OrderBy(i => i.NumberOfPosts).Reverse().ToList();
            GetMyCommunity();
        }

        public void GetMyCommunity()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            try
            {
                myCommunity = dbCommunities.Single(new { CreatorId = CurrentUser.Uid });
            }
            catch { }
           
        }
    }


    public class CommunityPopular
    {
        public string Cid { get; set; }
        [Required] public string Name { get; set; }
        public long CreatorId { get; set; }
        public string IconImg { get; set; } = "default_community.png";
        [Required] public string Description { get; set; }
        public DateTime TimeCreated { get; set; }
        public int NumberOfPosts { get; set; }
        public int Weight { get; set; } = 0;
    }
}