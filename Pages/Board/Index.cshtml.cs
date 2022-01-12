using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IdGen;
using KampongTalk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;
using MoreLinq;

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

        //public static MightyOrm ktDB { get; set; } =
        //    new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"]);

        public static MightyOrm postDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Post");

        public static MightyOrm userDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Users");

        public static MightyOrm likesDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Likes");

        [BindProperty] public IEnumerable<dynamic> postList { get; set; } = postDB.All();

        [BindProperty] public IEnumerable<dynamic> userList { get; set; } = userDB.All();

        public IEnumerable<dynamic> likesList { get; set; } = likesDB.All();

        //[BindProperty]
        //public List<dynamic> pList { get; set; } = new List<dynamic>();

        [BindProperty] public IEnumerable<dynamic> pList { get; set; } = Enumerable.Empty<dynamic>();

        //    public PagedResults<dynamic> postsList { get; set; } = ktDB.PagedFromSelect(
        //"Post p INNER JOIN Users u ON p.Author = u.Uid LEFT JOIN Likes li ON p.Pid = li.EntityId",
        //"p.Timestamp", // order by
        //"Pid, Content, AttachmentImg, Timestamp, InCommunity, IsComment, TaggedUsers, u.Uid, Name, AvatarImg, COUNT(li.Uid) AS Likes, (SELECT '925664515830317056' IN (SELECT Uid FROM Likes)) AS IsLiked", // columns
        //"p.IsComment = 0", // OPTIONAL WHERE spec
        //currentPage: 1, pageSize: 5 // page specs (defaults are 1 and 20)
        //); // OPTIONAL args for WHERE spec

        [BindProperty] public IFormFile postImg { get; set; }

        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");
            ;
            GetPosts();
            return Page();
        }

        public IEnumerable<dynamic> GetPosts()
        {
            // Query:
            // Inner join Posts and Users 
            // Left join Posts and Likes
            // Fetch count of likes per post
            // Fetch "isLiked" 
            // Left join Posts and Communities
            // "Filter" by requirements (e.g. friends /recc)
            // Incl pagination

            var query = (from post in postList
                join user in userList on post.Author equals user.Uid
                join like in likesList on post.Pid equals like.EntityId into likeGrp
                from sublike in likeGrp.DefaultIfEmpty()
                where post.IsComment == 0
                select new PostInfo
                {
                    Pid = post.Pid, Content = post.Content, AttachmentImg = post.AttachmentImg,
                    Timestamp = post.Timestamp, InCommunity = post.InCommunity, IsComment = post.IsComment,
                    TaggedUsers = post.TaggedUsers,
                    Uid = user.Uid, UserName = user.Name, UserPfp = user.AvatarImg, likeCount = likeGrp.Count()
                }).DistinctBy(post => post.Pid).AsEnumerable();

            // likeCount = sublike.Uid.Count()

            pList = query;


            //if (postType == "friend")
            //{
            //    // fetch list of friends from Relationship DB
            //    // return all posts where author is in list of friend IDs
            //    return;
            //}
            //IEnumerable<dynamic> result = (IEnumerable<dynamic>)ktDB.PagedFromSelect(
            //    "Post p INNER JOIN Users u ON p.Author = u.Uid, LEFT JOIN Likes li ON p.Pid = li.EntityId", // could have just been a table name!
            //    "p.Timestamp", // order by
            //    "Pid, Content, AttachmentImg, Timestamp, InCommunity, IsComment, TaggedUsers, u.Uid, Name, AvatarImg, COUNT(li.Uid) AS Likes, (SELECT '925664515830317056' IN (SELECT Uid FROM Likes)) AS IsLiked", // columns
            //    "p.IsComment = 0", // OPTIONAL WHERE spec
            //    currentPage: 1, pageSize: 5 // page specs (defaults are 1 and 20)
            //    ); // OPTIONAL args for WHERE spec

            return pList;
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
                newPost.Author = CurrentUser.Uid;
                newPost.Timestamp = DateTime.Now;

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

                postDB.Insert(newPost);
                return Page();
            }

            return Page();
        }

        public IActionResult OnPostLike()
        {
            Debug.WriteLine("onpostlike");
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            var existLike = likesDB.Single(new {newLike.EntityId, CurrentUser.Uid});
            if (existLike != null)
            {
                likesDB.Delete($"EntityId = '{newLike.EntityId}' && Uid = '{CurrentUser.Uid}'");
            }
            else
            {
                newLike.Uid = CurrentUser.Uid;
                likesDB.Insert(newLike);
            }

            return Redirect("/Board");
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

        //public void likePost(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("deceased");
        //}

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

        public void OnPostAddlike()
        {
            Debug.WriteLine("Addlike ajax call successful");
        }
    }
}