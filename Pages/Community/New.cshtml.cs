using System;
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
    public class New : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        public New(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        
        // Current user prop
        public User CurrentUser { get; set; }

        // New community prop
        [BindProperty] public Models.Community NewCommunity { get; set; }

        // Error flag prop
        public bool ShowDuplicateError { get; set; }
        public bool ShowAlreadyHaveCommunityError { get; set; }
        public dynamic ExistingCommunity { get; set; }
        public string CommunityNameClass { get; set; }
        [BindProperty] public IFormFile IconImage { get; set; }
        
        // Language 
        public dynamic LangData { get; set; }

        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            
            // Get user preferences
            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            // If the current user is verified, naturally, the object is present, so just redirect them.
            if (CurrentUser is null) return RedirectToPage("/Accounts/Login");

            // If the user has not OTP verified
            if (CurrentUser is {IsVerified: false}) return RedirectToPage("/Accounts/Verify");

            // Database declarations
            var dbCommunities =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Communities");

            // Check whether user already has a community created
            // If so, show error message
            var selectedCommunityByUser = dbCommunities.Single(new {CreatorId = CurrentUser.Uid});

            if (selectedCommunityByUser != null)
            {
                ShowAlreadyHaveCommunityError = true;
                ExistingCommunity = selectedCommunityByUser;
                return Page();
            }

            // Show page if not logged in already.
            return Page();
        }

        public IActionResult OnPost()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            if (CurrentUser == null) return RedirectToPage("/Accounts/Login");
            
            // Get user preferences
            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            // Database declarations
            var dbCommunities =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Communities");

            // Check whether a community with same Cid exists
            var selectedConflictCommunity = dbCommunities.Single(new {NewCommunity.Cid});

            // Show error message if community with same Cid exists
            if (selectedConflictCommunity != null)
            {
                ShowDuplicateError = true;
                CommunityNameClass = "has-text-danger";
                return Page();
            }

            // Check whether user already has a community created
            var selectedCommunityByUser = dbCommunities.Single(new {CreatorId = CurrentUser.Uid});

            if (selectedCommunityByUser != null) return Page();

            // Call method to process Name and set Cid.
            NewCommunity.SetCid();
            NewCommunity.CreatorId = CurrentUser.Uid;

            if (IconImage != null)
            {
                Console.WriteLine("dfghjdfhjgfjsdh");
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
                if (NewCommunity.IconImg != "default-community.png")
                {
                    string oldAvatarImg = NewCommunity.IconImg;
                    var oldAvatarImgOriginal =
                        oldAvatarImg.Split(".").First() + "-original." + oldAvatarImg.Split(".").Last();

                    var currentImgPath = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata/communities", oldAvatarImg);
                    var currentImgPathOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata/communities",
                        oldAvatarImgOriginal);
                    System.IO.File.Delete(currentImgPath);
                    System.IO.File.Delete(currentImgPathOriginal);
                }

                NewCommunity.IconImg = genImgName;
            }

            // Insert community object
            dbCommunities.Insert(NewCommunity);

            // Update relevancy
            SearchApi.PutRelevancy(NewCommunity.Description, NewCommunity.Cid);
            SearchApi.PutKeyword(NewCommunity.Name, 20, NewCommunity.Cid);

            return Redirect($"/Community?c={NewCommunity.Cid}");
        }
    }
}