using System;
using System.Linq;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Accounts
{
    public class Verify : PageModel
    {
        // Current user prop
        public User CurrentUser { get; set; }

        // FormName prop
        [BindProperty] public string FormName { get; set; }

        // OTP prop
        [BindProperty] public string IncomingOtpCode { get; set; }
        public string OtpCodeWarn { get; set; }
        public string OtpInputClass { get; set; }


        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            return Page();
        }

        public IActionResult OnPost()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // Database declarations
            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");

            if (FormName == "VerifyOTP")
            {
                // Find selected row and filter timestamp, less than 10 minutes ago.
                var selectedOtpVerification = dbActionLogs.All(new
                {
                    CurrentUser.Uid,
                    ActionExecuted = "account_otp_verify_sent",
                    Metadata = IncomingOtpCode
                }).Where(o => o.Timestamp >= DateTime.Now.AddMinutes(-10));

                // If 0, means that OTP is invalid
                if (selectedOtpVerification.ToList().Count == 0)
                {
                    OtpInputClass = "is-danger";
                    OtpCodeWarn = "<i class='fas fa-exclamation-triangle'></i>" +
                                  "&ensp;This OTP has either expired or is invalid. Please request for a new one.";

                    return Page();
                }

                // If all is well, delete the OTP record
                dbActionLogs.Delete(new
                {
                    UserId = CurrentUser.Uid,
                    ActionExecuted = "account_otp_verify_sent",
                    Metadata = IncomingOtpCode
                });

                // Set account info to verified
                CurrentUser.IsVerified = true;
                dbUsers.Update(CurrentUser);

                // And then, return the user to the main index page.
                return RedirectToPage("Index");
            }

            // if (FormName == "ResendMessageAction")
            // {
            //     
            // }
            
            // Catch all
            return Page();
        }
    }
}