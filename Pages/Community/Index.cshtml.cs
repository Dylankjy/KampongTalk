using System.ComponentModel.DataAnnotations;
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
        
        // Profile edit props
        [BindProperty, Required] public string EditDescription { get; set; }

        // Error handling props
        public bool ShowCommunityNotFoundError { get; set; }
        
        public IActionResult OnGet(string c)
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // Database declarations
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");
            var dbCommunities =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Communities");
            
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