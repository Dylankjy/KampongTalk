using System;
using KampongTalk.i18n;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Accounts
{
    public class Recover : PageModel
    {
        // Other stuff
        private static readonly Random Rnd = new Random();

        // Current user prop
        private User CurrentUser { get; set; }

        // Prop declarations
        [BindProperty] public string LoginPhoneNumber { get; set; }

        public dynamic LangData { get; } = Internationalisation.LoadLanguage("jp");
        private static dynamic LangDataStatic { get; } = Internationalisation.LoadLanguage("jp");
        public string FieldClass { get; set; }

        // For invalid account resets
        public bool ShowErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // If the current user is verified, naturally, the object is present, so just redirect them.
            if (CurrentUser is {IsVerified: true}) return RedirectToPage("/Index");

            // If the user has not OTP verified
            if (CurrentUser is {IsVerified: false}) return RedirectToPage("/Verify");

            // Show recovery page if not logged in already.
            return Page();
        }

        public IActionResult OnPost()
        {
            // Database declarations
            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");

            // Block OnPost if user is verified and already authenticated
            var currentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (currentUser != null) return RedirectToPage("Index");

            // Get user by PhoneNumber
            var selectedUserFromDb = dbUsers.Single(new
            {
                PhoneNumber = LoginPhoneNumber
            });

            if (selectedUserFromDb == null)
            {
                ShowErrorMessage = true;
                FieldClass = "is-danger";
                return Page();
            }

            // Convert dynamic to user object
            User selectedUser = new User().ToUser(selectedUserFromDb);

            // Generate new OTP code and insert into DB
            var otpRecord = new ActionLog
            {
                Uid = selectedUser.Uid,
                ActionExecuted = "account_passwd_reset_sent",
                Metadata = Rnd.Next(100000, 999999).ToString(),
                Info = "A password reset was requested and reset SMS was sent. This is a system action."
            };
            dbActionLogs.Insert(otpRecord);

            string i18nRecoverAccount = LangData.smsMessages.recoverAccount;
            selectedUser.SendSms(i18nRecoverAccount
                .Replace("{selectedUser.Name}", selectedUser.Name)
                .Replace("{selectedUser.Uid2}", selectedUser.Uid2)
                .Replace("{otpRecord.Metadata}", otpRecord.Metadata)
            );

            // Set session attribute
            HttpContext.Session.SetString("PasswordResetNumber", selectedUser.PhoneNumber);

            return RedirectToPage("./VerifyReset");
        }
    }
}