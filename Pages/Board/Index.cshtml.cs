using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mighty;
using IdGen;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace KampongTalk.Pages.Board
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public Post newPost { get; set; } = new Post();
        public User CurrentUser { get; set; }

        public static MightyOrm postDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Post");
        [BindProperty]
        public IEnumerable<dynamic> postList { get; set; } = postDB.All();

        [BindProperty]
        public IFormFile postImg { get; set; }

        private readonly IWebHostEnvironment webHostEnvironment;

        public IndexModel(IWebHostEnvironment env)
        {
            webHostEnvironment = env;
        }

        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null)
            {
                return Redirect("/Accounts/Login");
            }
            var postsList = postDB.Paged(orderBy: "Timestamp", currentPage: 1, pageSize: 10);
            return Page();

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
                    newPost.AttachmentImg = "";
                }
                postDB.Insert(newPost);
                return Page();
            }
            return Page();
        }
    }
}
