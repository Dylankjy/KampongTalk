using System;
using System.Linq;
using KampongTalk.i18n;
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

        // Language prop
        public dynamic LangData { get; set; }

        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // Set language data
            LangData = Internationalisation.LoadLanguage(HttpContext.Request.GetTypedHeaders().AcceptLanguage
                .First().ToString().Split("-").First());
            
            // If the user has not OTP verified
            if (CurrentUser is {IsVerified: false}) return Page();

            return RedirectToPage("/Index");
        }

        public IActionResult OnPost()
        {
            LangData = Internationalisation.LoadLanguage(HttpContext.Request.GetTypedHeaders().AcceptLanguage
                .First().ToString().Split("-").First());
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // Database declarations
            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs", "ActionId");
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users", "Uid");

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
                    OtpCodeWarn = LangData.accounts.verify.otpInvalid;

                    return Page();
                }

                // Clear all OTP records for user.
                dbActionLogs.Delete($"Uid = {CurrentUser.Uid} AND ActionExecuted = 'account_otp_verify_sent'");

                // Set account info to verified

                var selectedDbCurrentUser = dbUsers.Single(new {CurrentUser.Uid});
                selectedDbCurrentUser.IsVerified = true;
                dbUsers.Update(selectedDbCurrentUser);

                // Update session to reflect IsVerified to true.
                CurrentUser.IsVerified = true;
                HttpContext.Session.SetString("CurrentUser", CurrentUser.ToJson());
                
                // Set language data
                LangData = Internationalisation.LoadLanguage(HttpContext.Request.GetTypedHeaders().AcceptLanguage
                    .First().ToString().Split("-").First());

                // And then, return the user to the main index page.
                return RedirectToPage("/Onboarding/Preferences");
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