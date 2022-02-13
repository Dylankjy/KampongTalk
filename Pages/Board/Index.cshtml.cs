using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdGen;
using KampongTalk.Models;
using KampongTalk.Pages.Search;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Mighty;
using MoreLinq;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace KampongTalk.Pages.Board
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public IndexModel(IWebHostEnvironment env)
        {
            webHostEnvironment = env;
        }

        [BindProperty] public Post newPost { get; set; } = new Post();

        [BindProperty] public Likes newLike { get; set; } = new Likes();

        [BindProperty] public Post newComment { get; set; } = new Post();

        public User CurrentUser { get; set; }
        public dynamic LangData { get; set; }

        public string PrevPostDiv { get; set; }

        public static MightyOrm postDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Post");

        public static MightyOrm userDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Users");

        public static MightyOrm likesDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Likes");

        public static MightyOrm communitiesDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Communities");


        [BindProperty] public IEnumerable<dynamic> postList { get; set; } = postDB.All();

        [BindProperty] public IEnumerable<dynamic> userList { get; set; } = userDB.All();

        [BindProperty] public IEnumerable<dynamic> likesList { get; set; } = likesDB.All();

        [BindProperty] public IFormFile postImg { get; set; }

        // Posts to display on board
        public PagedResults<dynamic> PostsToDisplay { get; set; }

        public List<string> commsByPopularity = new List<string>();

        public List<string> eventsByPopularity = new List<string>();

        private int postPage { get; set; }

        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null)
            {
                return Redirect("/Accounts/Login");
            }
            LangData = UserPrefApi.GetLangByUid(CurrentUser);
            HttpContext.Session.SetString("PostPage", "1");
            PostsToDisplay = GetPosts("");
            GetPopularCommunities();
            GetPopularEvents();

            PrevPostDiv = HttpContext.Session.GetString("PostDivID");
            return Page();
        }

        public IActionResult OnGetFriends()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null)
            {
                return Redirect("/Accounts/Login");
            }
            LangData = UserPrefApi.GetLangByUid(CurrentUser);
            HttpContext.Session.SetString("PostPage", "1");
            PostsToDisplay = GetPosts("Friends");
            GetPopularCommunities();
            GetPopularEvents();

            PrevPostDiv = HttpContext.Session.GetString("PostDivID");
            return Page();
        }


        // Inititalise posts
        public dynamic GetPosts(string postType)
        {
            PagedResults<dynamic> allPosts = new PagedResults<dynamic>();
            if (postType == "Friends")
            {
                // Generating friends list in format (Uid, Uid, Uid)
                MightyOrm relationsDB = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Relationships");
                dynamic friendsList1 = relationsDB.All(where: $"(UserA = {CurrentUser.Uid} AND Status = 'friends')", columns: "UserB");
                dynamic friendsList2 = relationsDB.All(where: $"(UserB = {CurrentUser.Uid} AND Status = 'friends')", columns: "UserA");
                string friendIDsStr = "";

                if ( (!string.IsNullOrEmpty(Convert.ToString(friendsList1))) || (!string.IsNullOrEmpty(Convert.ToString(friendsList2))))
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
                        allPosts = postDB.Paged(orderBy: "Timestamp DESC", where: $"IsComment = 0 AND Author IN {friendIDsStr}", pageSize: 5, currentPage: 0);

                    }
                   
                }

            }
            else
            {
                // Whole Kampong, All posts
                allPosts = postDB.Paged(orderBy: "Timestamp DESC", where: "IsComment = 0 AND Author != '0'", pageSize: 5, currentPage: 0);
            }
            return allPosts;
        }

        public IActionResult OnPost()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null)
            {
                return Redirect("/Accounts/Login");
            }

            if (CurrentUser != null && ModelState.IsValid)
            {
                newPost.Author = CurrentUser.Uid;
                newPost.Timestamp = DateTime.Now;
                try
                {
                    var cid = communitiesDB.Single($"Name = '{newPost.InCommunity}'");
                    newPost.InCommunity = cid.Cid;
                }
                catch
                {
                    newPost.InCommunity = null;
                }

                if (postImg != null)
                {
                    var uploadFolder = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot/userdata/posts");
                    var extension = postImg.FileName.Split('.').Last();
                    var uniqueImgName = new IdGenerator(1).CreateId();
                    var attachmentImg = uniqueImgName + "." + extension;
                    var filePath = Path.Combine(uploadFolder, attachmentImg);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        postImg.CopyTo(fileStream);
                    }

                    newPost.AttachmentImg = attachmentImg;
                }
                else
                {
                    newPost.AttachmentImg = null;
                }

                try
                {
                    postDB.Insert(newPost);
                    
                    SearchApi.PutKeyword(CurrentUser.Name, 5, newPost.Pid);
                    SearchApi.PutRelevancy(newPost.Content, newPost.Pid);
                }
                catch
                {
                    
                }

                return Redirect("/Board");
            }

            return Redirect("/Board");
        }

        public ActionResult OnPostLike(string Pid)
        {

            try
            {
                CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
                var existLike = likesDB.Single(new { newLike.EntityId, CurrentUser.Uid });
                if (existLike != null)
                {
                    likesDB.Delete($"EntityId = '{newLike.EntityId}' && Uid = '{CurrentUser.Uid}'");
                }
                else
                {
                    newLike.Uid = CurrentUser.Uid;
                    likesDB.Insert(newLike);
                }

                var likeCount = likesDB.Count(where: $"EntityId = '{newLike.EntityId}'");
                var isLiked = likesDB.Count($"EntityId = '{newLike.EntityId}' && Uid = '{newLike.Uid}'");

                var likeResp = new List<string>
                {
                    isLiked.ToString(),
                    likeCount.ToString()
                };
                return new JsonResult(likeResp);
            }
            catch
            {
                return Redirect("/Board");
            }
        }

        public void GetPopularCommunities()
        {
            try
            {
                var commPosts = postDB.All(where: "IsComment = 0 AND Author != '0' AND InCommunity != ''");

                var commsByPopularityObj = commPosts
                            .GroupBy(p => p.InCommunity)
                            .Select(p => new
                            {
                                Community = p.Key,
                                Popularity = p.Count()
                            })
                            .OrderBy(p => p.Popularity)
                            .Take(5);

                if (commsByPopularityObj.Count() > 0)
                {
                    foreach (var comm in commsByPopularityObj)
                    {
                        string commName = comm.Community.ToString();
                        commsByPopularity.Add(commName);

                    }
                }

            }
            finally { }
        }

        public void GetPopularEvents()
        {
            try
            {
                var eventsDB = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");

                var allEvents = eventsDB.All(where: $"Attendees != '' AND Date >= CURDATE()");

                var eventsByPopularityObj = allEvents
                    .Select(e => new
                    {
                        Event = e.Eid,
                        AttendeeCount = e.Attendees.Split(";").Length
                    })
                    .OrderBy(e => e.AttendeeCount)
                    .Take(5);

                if (eventsByPopularityObj.Count() > 0)
                {
                    foreach (var e in eventsByPopularityObj)
                    {
                        string eventName = e.Event.ToString();
                        eventsByPopularity.Add(eventName);

                    }
                }

            }
            finally { }
        }

    }
}