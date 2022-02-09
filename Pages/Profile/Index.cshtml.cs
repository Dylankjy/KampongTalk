using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using IdGen;
using KampongTalk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace KampongTalk.Pages.Profile
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

        // Profile props
        public dynamic ViewingUser { get; set; }
        public string JoinDate { get; set; }
        public bool IsCurrentUserOwnPage { get; set; }

        // Profile image prop
        [BindProperty] public IFormFile ProfileImage { get; set; }
        [BindProperty] public IFormFile BannerImage { get; set; }

        // Posts
        public IEnumerable<dynamic> PostsToDisplay { get; set; }
        public int PostCount { get; set; }

        // Profile edit props
        [BindProperty] [Required] public string EditName { get; set; }
        [BindProperty] public string EditBio { get; set; }
        [BindProperty] public string EditBirthday { get; set; }

        // Error handling props
        public bool ShowUserNotFoundError { get; set; }

        public IActionResult OnGet(string u, int p)
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // Database declarations
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");

            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");

            var dbPost =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Post");

            // Get user by PhoneNumber
            ViewingUser = dbUsers.Single(new
            {
                Uid2 = u
            });

            // If User doesn't exist, show error page
            if (ViewingUser == null)
            {
                ShowUserNotFoundError = true;
                return Page();
            }

            DateTime joinDateRaw = dbActionLogs.Single(new
            {
                ViewingUser.Uid,
                ActionExecuted = "account_create"
            }).Timestamp;

            string[] months =
            {
                "January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
                "November", "December"
            };

            JoinDate = $"{months[joinDateRaw.Month - 1]} {joinDateRaw.Year}";

            // Set owner flag
            if (CurrentUser != null && CurrentUser.Uid.ToString() == ViewingUser.Uid.ToString())
                IsCurrentUserOwnPage = true;

            // Get posts by this user from database
            var postsByThisUser = dbPost.All(new {Author = ViewingUser.Uid});

            var numberOfObjectsPerPage = 10;
            PostsToDisplay = postsByThisUser.ToList().Skip(numberOfObjectsPerPage * p)
                .Take(numberOfObjectsPerPage);
            PostCount = PostsToDisplay.Count();

            // Set default value for textarea, Bio
            EditBio = ViewingUser.Bio;

            return Page();
        }

        public IActionResult OnPost()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // If current user null, don't bother
            // Check account existence
            if (CurrentUser == null) return RedirectToPage("/Accounts/Login");

            // Database declarations
            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users", "Uid");

            // Get current user dynamic object from database
            var currentUserFromDb = dbUsers.Single(new {CurrentUser.Uid, CurrentUser.Uid2});

            // Check account existence
            if (currentUserFromDb == null) return RedirectToPage("/Accounts/Login");

            // Modify user
            // ReSharper disable once PossibleNullReferenceException
            // Linting suggestion addressed above.
            currentUserFromDb.Name = EditName;
            currentUserFromDb.Bio = EditBio;

            // Check if birthday is already set, if it hasn't set it
            if (currentUserFromDb.DateOfBirth == null) currentUserFromDb.DateOfBirth = EditBirthday;

            // Profile image
            if (ProfileImage != null)
            {
                var imgExt = ProfileImage.FileName.Split('.').Last();
                var genImgNum = new IdGenerator(64).CreateId();
                var genImgName = genImgNum + "." + imgExt;
                var file = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", genImgName);
                var genImgNameOriginal = genImgNum + "-original." + imgExt;
                var fileOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", genImgNameOriginal);

                using (var fileStream = new FileStream(fileOriginal, FileMode.Create))
                {
                    ProfileImage.CopyTo(fileStream);
                }

                using (var image = Image.Load(fileOriginal))
                {
                    var ratio = Convert.ToDouble(image.Height) / Convert.ToDouble(image.Width);
                    var width = 512;
                    var height = (int) Math.Round(512 * ratio, 0);
                    image.Mutate(x => x.Resize(width, height));

                    image.Save(file);
                }

                // if current image is not default.jpg, we will delete on our end
                if (currentUserFromDb.AvatarImg != "default.jpg")
                {
                    string oldAvatarImg = currentUserFromDb.AvatarImg;
                    var oldAvatarImgOriginal =
                        oldAvatarImg.Split(".").First() + "-original." + oldAvatarImg.Split(".").Last();

                    var currentImgPath = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", oldAvatarImg);
                    var currentImgPathOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata",
                        oldAvatarImgOriginal);
                    System.IO.File.Delete(currentImgPath);
                    System.IO.File.Delete(currentImgPathOriginal);
                }

                currentUserFromDb.AvatarImg = genImgName;
            }

            if (BannerImage != null)
            {
                var imgExt = BannerImage.FileName.Split('.').Last();
                var genImgNum = new IdGenerator(65).CreateId();
                var genImgName = genImgNum + "-banner." + imgExt;
                var file = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", genImgName);
                var genImgNameOriginal = genImgNum + "-banner-original." + imgExt;
                var fileOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", genImgNameOriginal);

                using (var fileStream = new FileStream(fileOriginal, FileMode.Create))
                {
                    BannerImage.CopyTo(fileStream);
                }

                using (var image = Image.Load(fileOriginal))
                {
                    var ratio = Convert.ToDouble(image.Height) / Convert.ToDouble(image.Width);
                    var width = 512;
                    var height = (int) Math.Round(512 * ratio, 0);
                    image.Mutate(x => x.Resize(width, height));

                    image.Save(file);
                }

                // if current image is not default.jpg, we will delete on our end
                if (currentUserFromDb.BannerImg != "default-banner.jpg")
                {
                    string oldBannerImg = currentUserFromDb.BannerImg;
                    var oldBannerImgOriginal =
                        oldBannerImg.Split(".").First() + "-original." + oldBannerImg.Split(".").Last();

                    var currentImgPath = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", oldBannerImg);
                    var currentImgPathOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata",
                        oldBannerImgOriginal);
                    System.IO.File.Delete(currentImgPath);
                    System.IO.File.Delete(currentImgPathOriginal);
                }

                currentUserFromDb.BannerImg = genImgName;
            }

            // Add audit log
            var editLog = new ActionLog
            {
                Uid = currentUserFromDb.Uid,
                ActionExecuted = "profile_edit",
                Metadata = null,
                Info = "Your profile page was edited."
            };
            dbActionLogs.Insert(editLog);

            // Commit profile changes
            dbUsers.Update(currentUserFromDb);

            return Redirect($"/Profile?u={currentUserFromDb.Uid2}");
        }
    }
}