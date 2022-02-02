using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Google.Type;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;
using DateTime = System.DateTime;

namespace KampongTalk.Pages.Community
{
    public class Index : PageModel
    {
        
        // Current user prop
        private User CurrentUser { get; set; }
        
        // Profile props
        public dynamic ViewingCommunity { get; set; }
        public string CreateDate { get; set; }
        public bool IsCurrentUserOwner { get; set; }
        
        // Posts
        public IEnumerable<dynamic> PostsToDisplay { get; set; }
        public int PostCount { get; set; }

        // Profile edit props
        [BindProperty, Required] public string EditDescription { get; set; }

        // Error handling props
        public bool ShowCommunityNotFoundError { get; set; }
        
        public IActionResult OnGet(string c, int p)
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

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
            
            // TODO: The thingy no work for the textarea
            
            // Set owner flag
            long communityOwnerId = ViewingCommunity.CreatorId;
            if (CurrentUser != null && CurrentUser.Uid.ToString() == communityOwnerId.ToString())
            {
                IsCurrentUserOwner = true;
            }
            
            // Get posts in this community from database
            var postsInThisCommunity = dbPost.All(new { InCommunity = ViewingCommunity.Cid});

            var numberOfObjectsPerPage = 10;
            PostsToDisplay = postsInThisCommunity.ToList().Skip(numberOfObjectsPerPage * p)
                .Take(numberOfObjectsPerPage);
            PostCount = PostsToDisplay.Count();

            return Page();
        }


        public IActionResult OnPost(string c)
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            
            // Check user is signed on
            if (CurrentUser == null)
            {
                return RedirectToPage("/Accounts/Login");
            }
            
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
            if (selectedCommunity.CreatorId != CurrentUser.Uid)
            {
                return Page();
            }

            selectedCommunity.Description = EditDescription;
            
            // Commit changes
            dbCommunities.Update(selectedCommunity);

            return Redirect($"/Community?c={selectedCommunity.Cid}");
        }
    }
}