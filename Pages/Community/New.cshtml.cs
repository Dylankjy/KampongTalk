using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Community
{
    public class New : PageModel
    {
        // Current user prop
        public User CurrentUser { get; set; }
        
        // New community prop
        [BindProperty] public Models.Community NewCommunity { get; set; }
        
        // Error flag prop
        public bool ShowDuplicateError { get; set; }
        public string CommunityNameClass { get; set; }
        
        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // If the current user is verified, naturally, the object is present, so just redirect them.
            if (CurrentUser is {IsVerified: true}) return RedirectToPage("/Index");

            // If the user has not OTP verified
            if (CurrentUser is {IsVerified: false}) return RedirectToPage("/Verify");

            // Show login page if not logged in already.
            return Page();
        }

        public IActionResult OnPost()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            if (CurrentUser == null)
            {
                return RedirectToPage("/Accounts/Login");
            }
            
            // Database declarations
            var dbCommunities =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Communities");
            
            // Call method to process Name and set Cid.
            NewCommunity.SetCid();
            NewCommunity.CreatorId = CurrentUser.Uid;
            
            // Check whether a community with same Cid exists
            var selectedConflictCommunity = dbCommunities.Single(new { Cid = NewCommunity.Cid });

            // Show error message if community with same Cid exists
            if (selectedConflictCommunity != null)
            {
                ShowDuplicateError = true;
                CommunityNameClass = "has-text-danger";
                return Page();
            }

            // Insert community object
            dbCommunities.Insert(NewCommunity);

            return Redirect($"/Community?c={NewCommunity.Cid}");
        }
    }
}