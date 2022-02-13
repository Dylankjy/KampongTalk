using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mighty;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace KampongTalk.Pages.Board
{
    public class PostController : Controller
    {
        public User CurrentUser { get; set; }
        public static MightyOrm postDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Post");


        // ROUTE METHODS
        [HttpPost]
        [Route("/Board/renderpost")]
        public ActionResult Renderpost(string postType)
        {
            int pPage = Convert.ToInt32(HttpContext.Session.GetString("PostPage"));
            pPage = pPage + 1;
            HttpContext.Session.SetString("PostPage", pPage.ToString());

            dynamic nPost = new ExpandoObject();
            try
            {
                var allPosts = postDB.Paged(orderBy: "Timestamp DESC", where: "IsComment = 0 AND Author != '0'", pageSize: 1, currentPage: pPage + 5);
                nPost = GetPost(postType, pPage);
            }
            catch
            {
                return null;
            }

            return PartialView("/Pages/Partials/_Post.cshtml", nPost);
        }

        [HttpGet]
        [Route("/Board/searchcommunity")]
        public ActionResult SearchCommunity(string commName)
        {
            var communitiesDB = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Communities");
            string query = commName.Trim();
            string chosenCommName = "";
            try
            {
                var chosenCommObj = communitiesDB.Single($"Name like '{query}'");
                chosenCommName = chosenCommObj.Name;
            }
            catch
            {
                Debug.WriteLine("sth went wrong");
            }

            return new JsonResult(chosenCommName);
        }

        [HttpGet]
        [Route("/Board/communitylist")]
        public ActionResult GetCommunityList()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            var communitiesDB = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Communities");
            var communityResp = new List<string>();

            try
            {
                var someCommunities = communitiesDB.Paged(orderBy: "TimeCreated", currentPage: 0, pageSize: 5);

                if (someCommunities.TotalRecords > 0)
                {
                    foreach (var c in someCommunities.Items)
                    {
                        communityResp.Add(c.Name);

                    }

                }

            }
            finally { }

            return new JsonResult(communityResp);
        }

        [HttpPost]
        [Route("/Board/setpos")]
        public void SetPosition(string postDivId)
        {
            HttpContext.Session.SetString("PostDivID", postDivId);
        }

        [HttpPost]
        [Route("/Board/removepos")]
        public void RemovePosition()
        {
            HttpContext.Session.Remove("PostDivID");
        }


        // FUNCTIONS

        public dynamic GetPost(string postType, int postPage)
        {
            var CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            PagedResults<dynamic> nPosts = new PagedResults<dynamic>();

            if (postType == "Friends")
            {
                // Generating friends list in format (Uid, Uid, Uid)
                MightyOrm relationsDB = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Relationships");
                dynamic friendsList1 = relationsDB.All(where: $"(UserA = {CurrentUser.Uid} AND Status = 'friends')", columns: "UserB");
                dynamic friendsList2 = relationsDB.All(where: $"(UserB = {CurrentUser.Uid} AND Status = 'friends')", columns: "UserA");

                string friendIDsStr = "";

                if ((!string.IsNullOrEmpty(Convert.ToString(friendsList1))) || (!string.IsNullOrEmpty(Convert.ToString(friendsList2))))
                {
                    friendIDsStr = "(";

                    if (!string.IsNullOrEmpty(Convert.ToString(friendsList1)))
                    {
                        foreach (var friend in friendsList1)
                        {
                            string friendID = friend.UserB.ToString();
                            friendIDsStr = friendIDsStr + friendID + ", ";
                        };
                    }

                    if (!string.IsNullOrEmpty(Convert.ToString(friendsList2)))
                    {
                        foreach (var friend in friendsList2)
                        {
                            string friendID = friend.UserA.ToString();
                            friendIDsStr = friendIDsStr + friendID + ", ";
                        };
                    }
                    if (friendIDsStr.Length > 3)
                    {
                        friendIDsStr = friendIDsStr[0..^2];
                        friendIDsStr = friendIDsStr + ")";

                        nPosts = postDB.Paged(orderBy: "Timestamp DESC", where: $"IsComment = 0 AND Author IN {friendIDsStr}", pageSize: 1, currentPage: postPage + 5);

                    }

                }

            }
            else
            {
                // Whole Kampong, All posts
                nPosts = postDB.Paged(orderBy: "Timestamp DESC", where: "IsComment = 0 AND Author != '0'", pageSize: 1, currentPage: postPage + 5);
            }
            return nPosts.Items.First();
        }

    }
}
