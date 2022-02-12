using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;
using Microsoft.AspNetCore.Hosting;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using MoreLinq;
using System.Diagnostics;

namespace KampongTalk.Pages.Board
{
    public class DetailModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public DetailModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        [BindProperty]
        public User CurrentUser { get; set; }
        public dynamic LangData { get; set; }

        [BindProperty]
        public long Postid { get; set; }

        [BindProperty]
        public IEnumerable<dynamic> commentList { get; set; } = Enumerable.Empty<dynamic>();

        [BindProperty]
        public dynamic thisPost { get; set; } = new Post();

        [BindProperty]
        public dynamic author { get; set; } = new User();

        [BindProperty] 
        public Post newComment { get; set; } = new Post();

        public static MightyOrm postDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Post");

        public static MightyOrm userDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Users");

        public static MightyOrm likesDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Likes", "EntityId");

        [BindProperty] public IEnumerable<dynamic> postList { get; set; } = postDB.All();

        [BindProperty] public IEnumerable<dynamic> userList { get; set; } = userDB.All();

        public IEnumerable<dynamic> likesList { get; set; } = likesDB.All();

        public IActionResult OnGet(string Pid)
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");
            LangData = UserPrefApi.GetLangByUid(CurrentUser);
            Postid = Convert.ToInt64(Pid);
            // commentList = postDB.All($"IsComment = '{Pid}'");
            commentList = GetComments(Postid);
            thisPost = postDB.Single($"Pid = '{Pid}'");
            var authorId = thisPost.Author;
            author = userDB.Single($"Uid = '{authorId}'");

            return Page();
        }

        public IActionResult OnPostComment(string Pid)
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            newComment.Author = CurrentUser.Uid;
            newComment.Timestamp = DateTime.Now;
            newComment.AttachmentImg = null;
            postDB.Insert(newComment);
            return Redirect("/Board/Detail/" + Pid);
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
                             Author = post.Author,
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
