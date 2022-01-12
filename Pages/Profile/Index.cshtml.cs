using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Profile
{
    public class Index : PageModel
    {
        // Profile props
        public dynamic ViewingUser { get; set; }
        public string JoinDate { get; set; }

        // Error handling props
        public bool ShowUserNotFoundError { get; set; }

        public IActionResult OnGet(string u)
        {
            // Database declarations
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");

            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");

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

            DateTime JoinDateRaw = dbActionLogs.Single(new
            {
                ViewingUser.Uid,
                ActionExecuted = "account_create"
            }).Timestamp;

            string[] months =
            {
                "January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
                "November", "December"
            };

            JoinDate = $"{months[JoinDateRaw.Month - 1]} {JoinDateRaw.Year}";

            return Page();
        }
    }
}