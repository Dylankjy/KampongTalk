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

        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null)
            {
                return Redirect("/Accounts/Login");
            }
            GetPosts(CurrentUser);
            return Page();
        }

        public IEnumerable<dynamic> GetPosts(User currUser)
        {
            // Query:
            // Inner join Posts and Users 
            // Left join Posts and Likes
            // Fetch count of likes per post
            // Fetch "isLiked" 
            // Left join Posts and Communities
            // "Filter" by requirements (e.g. friends /recc)
            // Incl pagination

            // Old one that worked but stopped working 
            //var query = (from post in postList
            //             join user in userList on post.Author equals user.Uid
            //             join like in likesList on post.Pid equals like.EntityId into likeGrp
            //             //from sublike in likeGrp.DefaultIfEmpty()
            //             where post.IsComment == 0
            //             select new PostInfo
            //             {
            //                 Pid = post.Pid,
            //                 Content = post.Content,
            //                 AttachmentImg = post.AttachmentImg,
            //                 Timestamp = post.Timestamp,
            //                 InCommunity = post.InCommunity,
            //                 IsComment = post.IsComment,
            //                 TaggedUsers = post.TaggedUsers,
            //                 Uid = user.Uid,
            //                 UserName = user.Name,
            //                 UserPfp = user.AvatarImg,
            //                 likeCount = likeGrp.Count()
            //             }).DistinctBy(post => post.Pid).AsEnumerable();

            Debug.WriteLine("total likes in db: " + likesList.Count());

            // V 2
            var query = (from post in postList
                         join user in userList on post.Author equals user.Uid
                         where post.IsComment == 0
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
                             likeCount = (from like in likesList
                                          where like.EntityId == post.Pid
                                          select new { like.Uid }).Count(),
                             IsLiked = (from like in likesList
                                        where like.EntityId == post.Pid && like.Uid == currUser
                                        select new { like.EntityId }).Any()
                         }).DistinctBy(post => post.Pid).AsEnumerable();

            var testQuery = (from like in likesList
                             where like.EntityId == 933535030863466500
                             select new { like.Uid }).Count();

            Debug.WriteLine("manually inputted id " + testQuery);

            foreach (var p in query)
            {
                //Debug.WriteLine(p.Pid + " : " + p.Content + " pid of type " + p.Pid.GetType());
                testQuery = (from like in likesList
                                 where like.EntityId == p.Pid
                                 select new { like.Uid }).Count();
                Debug.WriteLine(p.Pid + " : " + p.Content + "has likes " + testQuery);
                //Debug.WriteLine(p.likeCount);
                //Debug.WriteLine(p.IsLiked);
            }
            // ISSUE: When manually input id, likeCount works. But when it iterates through the entries, it returns wrong value, even if ID and ID type is correct :(

            pList = query;

            //if (pList != null && pList.Any())
            //{
            //    foreach (var post in pList)
            //    {
            //        //Debug.WriteLine(post.Pid);
            //        //Debug.WriteLine(post.likeCount);
            //        var likeCount = likesDB.Count($"EntityId = '{post.Pid}'", "Uid");
            //        Debug.WriteLine("like count of post "+ likeCount);
            //        post.likeCount = (int)(long)likeCount;
            //        // Debug.WriteLine("like count of post " + post.likeCount);
            //        //var isLiked = (long)likesDB.Count($"EntityId = '{newLike.EntityId}' && Uid = '{newLike.Uid}'");
            //        //if (isLiked == 1)
            //        //{
            //        //    post.IsLiked = true;
            //        //}
            //        //else
            //        //{
            //        //    post.IsLiked = false;
            //        //}

            //    }

            //}

            return pList;
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