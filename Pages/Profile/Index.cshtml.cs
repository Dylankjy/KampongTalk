using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Profile
{
    public class Index : PageModel
    {
        // Current user prop
        private User CurrentUser { get; set; }

        // Profile props
        public dynamic ViewingUser { get; set; }
        public string JoinDate { get; set; }
        public bool IsCurrentUserOwnPage { get; set; }

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