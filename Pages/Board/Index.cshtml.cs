using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Mighty;
using MoreLinq;
using Newtonsoft.Json;

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

        [BindProperty] public IEnumerable<dynamic> communityList { get; set; } = communitiesDB.All();
        [BindProperty] public IEnumerable<dynamic> pList { get; set; } = Enumerable.Empty<dynamic>();

        [BindProperty] public IFormFile postImg { get; set; }
        
        // Posts to display on board
        // public dynamic PostsToDisplay { get; set; }
        public IEnumerable<dynamic> PostsToDisplay { get; set; }

        //public IActionResult OnGet()
        //{
        //    return Redirect("/Board/All");
        //}

        public bool needLogin()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null)
            {
                return true;
            }
            PrevPostDiv = HttpContext.Session.GetString("PostDivID"); 
            return false;

        }

        public IActionResult OnGet()
        {
            if (needLogin()) return Redirect("/Accounts/Login");

            GetPosts("");
            return Page();
        }

        public IActionResult OnGetFriends()
        {
            if (needLogin()) return Redirect("/Accounts/Login");
            GetPosts("Friends");
            return Page();
        }

        public IActionResult OnGetRecommended()
        {
            if (needLogin()) return Redirect("/Accounts/Login");
            GetPosts("Recommended");
            return Page();
        }

        public void GetPosts(string postType)
        {
            IEnumerable<dynamic> allPosts = Enumerable.Empty<dynamic>();
            if (postType == "Friends")
            {
                // Friends Posts
                MightyOrm relationsDB = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Relationships");
                dynamic friendsList1 = relationsDB.All(where: $"(UserA = {CurrentUser.Uid} AND Status = 'friends')", columns: "UserB");
                dynamic friendsList2 = relationsDB.All(where: $"(UserB = {CurrentUser.Uid} AND Status = 'friends')", columns: "UserA");
                //List<dynamic>? friendsList2 = Enumerable.ToList(relationsDB.All(where: $"(UserB = {CurrentUser.Uid} AND Status = 'friends')", columns: "UserA"));
                string friendIDsStr = "";

                if ( (!string.IsNullOrEmpty(Convert.ToString(friendsList1))) || (!string.IsNullOrEmpty(Convert.ToString(friendsList2))))
                {
                    friendIDsStr = "(";

                    if (!string.IsNullOrEmpty(Convert.ToString(friendsList1)))
                    {
                        foreach (var friend in friendsList1)
                        {
                            string friendID = friend.UserB.ToString();
                            Debug.WriteLine(friendID);
                            friendIDsStr = friendIDsStr + friendID + ", ";
                        };
                    }

                    if (!string.IsNullOrEmpty(Convert.ToString(friendsList2)))
                    {
                        foreach (var friend in friendsList2)
                        {
                            string friendID = friend.UserA.ToString();
                            Debug.WriteLine(friendID);
                            friendIDsStr = friendIDsStr + friendID + ", ";
                        };
                    }
                    if (friendIDsStr.Length > 3)
                    {
                        friendIDsStr = friendIDsStr[0..^2];
                        friendIDsStr = friendIDsStr + ")";
                        Debug.WriteLine(friendIDsStr, "the string is");
                        allPosts = postDB.All(where: $"IsComment = 0 AND Author IN {friendIDsStr}", orderBy: "Timestamp DESC");

                    }
                   
                }

            }
            else if(postType == "Recommended")
            {
                // Recommended Posts
                allPosts = postDB.All(where: "IsComment = 0 AND Author != '0'", orderBy: "Timestamp DESC", limit: 2);
            }
            else
            {
                // Whole Kampong, All posts
                allPosts = postDB.All(where: "IsComment = 0 AND Author != '0'", orderBy: "Timestamp DESC", limit: 5);
                // allPosts = postDB.All(where: "IsComment = 0 AND Author != '0'", orderBy: "Timestamp DESC");
            }
            //var allPosts = postDB.All(new { IsComment = 0});
            PostsToDisplay = allPosts;
        }

        public IActionResult OnPost()
        {

            if (CurrentUser != null && ModelState.IsValid)
            {
                CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
                newPost.Author = CurrentUser.Uid;
                newPost.Timestamp = DateTime.Now;
                Debug.WriteLine(newPost.InCommunity);
                var cid = communitiesDB.Single($"Name = '{newPost.InCommunity}'");
                newPost.InCommunity = cid.Cid;
                Debug.WriteLine(newPost.InCommunity);

                if (postImg != null)
                {
                    var uploadFolder = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot/imgs");
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
                    
                    // SearchApi.PutKeyword(CurrentUser.Name, 5, newPost.Pid);
                    // SearchApi.PutRelevancy(newPost.Content, newPost.Pid);
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
            Debug.WriteLine("backend: " + Pid);
            //Debug.WriteLine("onpostlike call");
            ////var isLiked = false;

            //// Testing getting postid from request object body
            //{
            //    MemoryStream stream = new MemoryStream();
            //    Request.Body.CopyTo(stream);
            //    stream.Position = 0;
            //    using (StreamReader reader = new StreamReader(stream))
            //    {
            //        string requestBody = reader.ReadToEnd();
            //        if (requestBody.Length > 0)
            //        {
            //            var obj = JsonConvert.DeserializeObject<Likes>(requestBody);
            //            Debug.WriteLine("ent id: " + obj.EntityId + " with type " + obj.EntityId.GetType());
            //            if (obj != null)
            //            {
            //                newLike = obj;

            //            }
            //        }
            //    }
            //}

            try
            {
                CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
                Debug.WriteLine(newLike.EntityId);
                var existLike = likesDB.Single(new { newLike.EntityId, CurrentUser.Uid });
                if (existLike != null)
                {
                    likesDB.Delete($"EntityId = '{newLike.EntityId}' && Uid = '{CurrentUser.Uid}'");
                    //isLiked = true;
                }
                else
                {
                    newLike.Uid = CurrentUser.Uid;
                    likesDB.Insert(newLike);
                }

                var likeCount = likesDB.Count(where: $"EntityId = '{newLike.EntityId}'");
                Debug.WriteLine("like count:" + likeCount);
                var isLiked = likesDB.Count($"EntityId = '{newLike.EntityId}' && Uid = '{newLike.Uid}'");
                Debug.WriteLine("isLiked:" + isLiked);

                var likeResp = new List<string>
                {
                    isLiked.ToString(),
                    likeCount.ToString()
                };
                //return Page();
                return new JsonResult(likeResp);
            }
            catch
            {
                Debug.WriteLine("Error");
                return Redirect("/Board");
            }
        }

        public ActionResult OnGetCommunitylist()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            var communityResp = new List<string>();

            try
            {
                var userComm = communitiesDB.Single($"CreatorId = '{CurrentUser.Uid}'");
                if (userComm != null)
                {
                    communityResp.Add(userComm.Name);
                }
            }
            finally {}

            try
            {
                var someCommunities = communitiesDB.Paged(orderBy: "TimeCreated", currentPage: 0, pageSize: 5);


                if (someCommunities.TotalRecords > 0)
                {
                    Debug.WriteLine(someCommunities.Items);
                    foreach (var c in someCommunities.Items)
                    {
                        communityResp.Add(c.Name);

                    }

                }

            }
            finally { }

            //return Page();
            return new JsonResult(communityResp);
        }

        public void OnPostPosition(string postDivId)
        {
            HttpContext.Session.SetString("PostDivID", postDivId);
        }

        public void OnPostRemovepos()
        {
            HttpContext.Session.Remove("PostDivID");
        }

        public IActionResult OnPostComment()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            newComment.Author = CurrentUser.Uid;
            newComment.Timestamp = DateTime.Now;
            newComment.AttachmentImg = null;
            postDB.Insert(newComment);
            return Redirect("/Board");
        }

        public IEnumerable<dynamic> GetComments(long parentPid)
        {
            var query = (from post in postList
                join user in userList on post.Author equals user.Uid
                join like in likesList on post.Pid equals like.EntityId into likeGrp
                from sublike in likeGrp.DefaultIfEmpty()
                where post.IsComment == parentPid
                select new PostInfo
                {
                    Pid = post.Pid,
                    Content = post.Content,
                    AttachmentImg = post.AttachmentImg,
                    Timestamp = post.Timestamp,
                    InCommunity = post.InCommunity,
                    IsComment = post.IsComment,
                    TaggedUsers = post.TaggedUsers,
                    Uid = user.Uid,
                    UserName = user.Name,
                    UserPfp = user.AvatarImg,
                    likeCount = likeGrp.Count()
                }).DistinctBy(post => post.Pid).AsEnumerable();
            return query;
        }

    }
}