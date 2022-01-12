using System;
using System.Linq;
using System.Text.RegularExpressions;
using KampongTalk.i18n;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Accounts
{
    public class VerifyReset : PageModel
    {
        // Current user prop
        private User CurrentUser { get; set; }

        public dynamic LangData { get; } = Internationalisation.LoadLanguage("jp");
        private static dynamic LangDataStatic { get; } = Internationalisation.LoadLanguage("jp");
        
        // Retrieve phone number
        public string PhoneNumber { get; set; }
        
        // OTP prop
        [BindProperty] public string NewUserPassword { get; set; }
        public string PasswordWarn { get; set; } = LangDataStatic.accounts.register.passwordHint;

        public string PasswordInputClass { get; set; } = string.Empty;
        
        [BindProperty] public string IncomingOtpCode { get; set; }
        public string OtpCodeWarn { get; set; }
        public string OtpInputClass { get; set; }
        
        
        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            // Get phone number attribute
            PhoneNumber = HttpContext.Session.GetString("PasswordResetNumber");
            
            // If current user is already logged in or phone number missing, clear the session. This session is malformed.
            if (CurrentUser != null || PhoneNumber == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("./Recover");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            // Get phone number attribute
            PhoneNumber = HttpContext.Session.GetString("PasswordResetNumber");

            
            // If current user is already logged in or phone number missing, clear the session. This session is malformed.
            if (CurrentUser != null || PhoneNumber == null)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("./Recover");
            }
            
            // Database declarations
            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");
            
            // Get user with phone number
            var selectedUserFromDb = dbUsers.Single(new {PhoneNumber});
            
            // Form abortion flag
            bool abortFormSubmission = false;

            // If the password is not of expected strength
            // Incredibly stupid work around to allow passwords with special characters
            if (!Regex.IsMatch(NewUserPassword, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$") &&
                !Regex.IsMatch(NewUserPassword, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$"))
            {
                PasswordInputClass = "is-danger";
                PasswordWarn = LangDataStatic.accounts.register.passwordNotSufficient;
                abortFormSubmission = true;
            }

            // Convert dynamic to user object
            User selectedUser = new User().ToUser(selectedUserFromDb);

            // Find selected row and filter timestamp, less than 10 minutes ago.
            var selectedOtpVerification = dbActionLogs.All(new
            {
                selectedUser.Uid,
                ActionExecuted = "account_passwd_reset_sent",
                Metadata = IncomingOtpCode
            }).Where(o => o.Timestamp >= DateTime.Now.AddMinutes(-10));

            // If 0, means that OTP is invalid
            if (selectedOtpVerification.ToList().Count == 0 || selectedUser.Uid == -1)
            {
                OtpInputClass = "is-danger";
                OtpCodeWarn = "<i class='fas fa-exclamation-triangle'></i>" +
                              "&ensp;This OTP has either expired or is invalid. Please request for a new one.";

                abortFormSubmission = true;
            }
            
            // Check whether to abort the form
            if (abortFormSubmission)
            {
                return Page();
            }

            // Clear all OTP records for user.
            dbActionLogs.Delete($"Uid = {selectedUser.Uid} AND ActionExecuted = 'account_passwd_reset_sent'");
            
            // Set the new password
            selectedUserFromDb.Password = selectedUser.SetPassword(NewUserPassword);
            
            // Commit update to database
            dbUsers.Update(selectedUserFromDb);
            
            // Audit log the password reset
            // Add action to logs
            dbActionLogs.Insert(new ActionLog
            {
                Uid = selectedUser.Uid,
                ActionExecuted = "account_passwd_reset",
                Metadata = null,
                Info = "Your account's password was successfully reset."
            });
            
            return RedirectToPage("/Index");
        }
        
    }
}