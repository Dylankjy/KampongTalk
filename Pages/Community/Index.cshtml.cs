using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using IdGen;
using KampongTalk.Models;
using KampongTalk.Pages.Search;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace KampongTalk.Pages.Community
{
    public class Index : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public Index(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        
        // Current user prop
        public User CurrentUser { get; set; }

        // User preferences prop
        public dynamic LangData { get; set; }

        // Profile props
        public dynamic ViewingCommunity { get; set; }
        public string CreateDate { get; set; }
        public bool IsCurrentUserOwner { get; set; }
        
        // Community Icon prop
        [BindProperty] public IFormFile IconImage { get; set; }

        // Posts
        public IEnumerable<dynamic> PostsToDisplay { get; set; }
        public int PostCount { get; set; }
        
        // Page number
        public int PageNo { get; set; }
        public int PreviousPageNo { get; set; }
        public int NextPageNo { get; set; }

        // Profile edit props
        [BindProperty] [Required] public string EditDescription { get; set; }

        // Error handling props
        public bool ShowCommunityNotFoundError { get; set; }

        public IActionResult OnGet(string c, int p)
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // Get user preferences
            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            // Database declarations
            var dbCommunities =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Communities");
            var dbPost =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Post");

            // Get user by PhoneNumber
            ViewingCommunity = dbCommunities.Single(new
            {
                Cid = c
            });

            // If User doesn't exist, show error page
            if (ViewingCommunity == null)
            {
                ShowCommunityNotFoundError = true;
                return Page();
            }

            string[] months =
            {
                "January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
                "November", "December"
            };

            DateTime createDateRaw = ViewingCommunity.TimeCreated;

            CreateDate = $"{months[createDateRaw.Month - 1]} {createDateRaw.Year}";

            // Set default value for EditField
            EditDescription = ViewingCommunity.Description;

            // Set owner flag
            long communityOwnerId = ViewingCommunity.CreatorId;
            if (CurrentUser != null && CurrentUser.Uid.ToString() == communityOwnerId.ToString())
                IsCurrentUserOwner = true;

            // Get posts in this community from database
            var postsInThisCommunity = dbPost.All(new {InCommunity = ViewingCommunity.Cid, IsComment = 0});

            var numberOfObjectsPerPage = 10;
            var posts = postsInThisCommunity.ToList();
            PostsToDisplay = posts.Skip(numberOfObjectsPerPage * p)
                .Take(numberOfObjectsPerPage).OrderBy(post => post.Timestamp);
            PostCount = posts.Count();
            
            // Set page number
            PageNo = p;
            PreviousPageNo = p - 1;
            NextPageNo = p + 1;

            return Page();
        }


        public IActionResult OnPost(string c)
        {
            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");
            var dbPost =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Post");

            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            
            // Get user preferences
            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            // Check user is signed on
            if (CurrentUser == null) return RedirectToPage("/Accounts/Login");

            // Database declarations
            var dbCommunities =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Communities", "Cid");

            var selectedCommunity = dbCommunities.Single(new
            {
                CreatorId = CurrentUser.Uid,
                Cid = c
            });

            // Check permission
            if (selectedCommunity.CreatorId != CurrentUser.Uid) return Page();

            selectedCommunity.Description = EditDescription;
            
            // Set icon
            // Profile image
            if (IconImage != null)
            {
                var imgExt = IconImage.FileName.Split('.').Last();
                var genImgNum = new IdGenerator(66).CreateId();
                var genImgName = genImgNum + "." + imgExt;
                var file = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata/communities", genImgName);
                var genImgNameOriginal = genImgNum + "-original." + imgExt;
                var fileOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata/communities", genImgNameOriginal);

                using (var fileStream = new FileStream(fileOriginal, FileMode.Create))
                {
                    IconImage.CopyTo(fileStream);
                }

                using (var image = Image.Load(fileOriginal))
                {
                    var ratio = Convert.ToDouble(image.Height) / Convert.ToDouble(image.Width);
                    var width = 384;
                    var height = (int) Math.Round(384 * ratio, 0);
                    image.Mutate(x => x.Resize(width, height));

                    image.Save(file);
                }

                // if current image is not default.jpg, we will delete on our end
                if (selectedCommunity.IconImg != "default-community.png")
                {
                    string oldAvatarImg = selectedCommunity.IconImg;
                    var oldAvatarImgOriginal =
                        oldAvatarImg.Split(".").First() + "-original." + oldAvatarImg.Split(".").Last();

                    var currentImgPath = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata/communities", oldAvatarImg);
                    var currentImgPathOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata/communities",
                        oldAvatarImgOriginal);
                    System.IO.File.Delete(currentImgPath);
                    System.IO.File.Delete(currentImgPathOriginal);
                }

                selectedCommunity.IconImg = genImgName;
            }

            // Commit changes
            dbCommunities.Update(selectedCommunity);

            // Add to user audit log
            dbActionLogs.Insert(new ActionLog
            {
                Uid = CurrentUser.Uid,
                ActionExecuted = "community_edit",
                Metadata = null,
                Info = $"{selectedCommunity.Name}'s description was changed."
            });

            // Add notification to community as post.
            dbPost.Insert(new Post
            {
                Author = 0,
                Content = $"The community's description was changed to \"{EditDescription}\".",
                InCommunity = selectedCommunity.Cid,
                Timestamp = DateTime.Now
            });

            // Update relevancy
            SearchApi.PutRelevancy(selectedCommunity.Description, selectedCommunity.Cid);
            SearchApi.PutKeyword(selectedCommunity.Name, 20, selectedCommunity.Cid);

            return Redirect($"/Community?c={selectedCommunity.Cid}");
        }
    }
}