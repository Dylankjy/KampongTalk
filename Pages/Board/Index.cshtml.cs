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
        public dynamic PostsToDisplay { get; set; }

        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null)
            {
                return Redirect("/Accounts/Login");
            }
            GetPosts();

            return Page();
        }

        public void GetPosts()
        {
            // Kinda feel your pain, so I fixed this mess for you.
            // You might need to fix the like button back again, I broke it when I changed it to the new post template
            // :D
            var allPosts = postDB.All(new { IsComment = 0 });
            PostsToDisplay = allPosts;
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
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
                    // SearchApi.PutRelevancy(newPost.Content, newPost.Pid);
                }
                catch
                {

                }

                return Redirect("/Board");
            }

            return Redirect("/Board");
        }

        public ActionResult OnPostLike()
        {
            //var isLiked = false;

            // Testing getting postid from request object body
            {
                MemoryStream stream = new MemoryStream();
                Request.Body.CopyTo(stream);
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    string requestBody = reader.ReadToEnd();
                    if (requestBody.Length > 0)
                    {
                        var obj = JsonConvert.DeserializeObject<Likes>(requestBody);
                        if (obj != null)
                        {
                            newLike = obj;
                        }
                    }
                }
            }

            try
            {
                CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
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