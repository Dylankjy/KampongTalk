using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdGen;
using KampongTalk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace KampongTalk.Pages.Onboarding
{
    public class ProfileSettingsModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public ProfileSettingsModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // Current user prop
        public User CurrentUser { get; set; }

        // Profile props
        public dynamic ViewingUser { get; set; }
        public bool IsCurrentUserOwnPage { get; set; }

        // Profile image prop
        [BindProperty] public IFormFile ProfileImage { get; set; }
        [BindProperty] public IFormFile BannerImage { get; set; }

        // Profile edit props
        [BindProperty] public string EditBio { get; set; }
        [BindProperty] public string EditBirthday { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            Debug.WriteLine("onpost called");
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
            var currentUserFromDb = dbUsers.Single(new { CurrentUser.Uid, CurrentUser.Uid2 });

            // Check account existence
            if (currentUserFromDb == null) return RedirectToPage("/Accounts/Login");


            // Modify user
            // ReSharper disable once PossibleNullReferenceException
            // Linting suggestion addressed above.
            currentUserFromDb.Bio = EditBio;

            // Check if birthday is already set, if it hasn't set it
            if (currentUserFromDb.DateOfBirth == null) currentUserFromDb.DateOfBirth = EditBirthday;

            Debug.WriteLine("i am here 1");
            // Profile image
            if (ProfileImage != null)
            {
                Debug.WriteLine("pfp is not null");
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
                    var width = 384;
                    var height = (int)Math.Round(384 * ratio, 0);
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
                Debug.WriteLine("banner image is not null");
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
                    var width = 1080;
                    var height = (int)Math.Round(1080 * ratio, 0);
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
            Debug.WriteLine("here!!!!");

            //// Add audit log
            //var editLog = new ActionLog
            //{
            //    Uid = currentUserFromDb.Uid,
            //    ActionExecuted = "profile_edit",
            //    Metadata = null,
            //    Info = "Your profile page was edited."
            //};
            //dbActionLogs.Insert(editLog);

            // Commit profile changes
            dbUsers.Update(currentUserFromDb);

            return Redirect("/Onboarding/Done");
        }
    }
}
