using System;
using System.Linq;
using System.Text.RegularExpressions;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Accounts
{
    public class Register : PageModel
    {
        // Other stuff
        private static readonly Random Rnd = new Random();

        // Current user prop
        public User CurrentUser { get; set; }

        // Prop declarations
        [BindProperty] public User NewUserAccount { get; set; }
        [BindProperty] public string NewUserPassword { get; set; }

        public string PasswordWarn { get; set; }
            = "<i class='fas fa-info-circle'></i>" +
              "&ensp;Your password needs to contain at least 8 characters and a number.";

        public string PasswordInputClass { get; set; } = string.Empty;
        public string PhoneWarn { get; set; }
        public string PhoneInputClass { get; set; }

        // --------------------------------------

        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // If the current user is verified, naturally, the object is present, so just redirect them.
            if (CurrentUser is {IsVerified: true}) return RedirectToPage("Index");

            // If the user has not OTP verified
            if (CurrentUser is {IsVerified: false}) return RedirectToPage("Verify");

            // And then show them the page
            // If verification is not yet done, the verification screen will show up
            // else the registration form is shown.
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
            if (currentUser is {IsVerified: true}) return RedirectToPage("Index");

            var abortFormSubmission = false;
            // If the password is not of expected strength
            // Incredibly stupid work around to allow passwords with special characters
            if (!Regex.IsMatch(NewUserPassword, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$") &&
                !Regex.IsMatch(NewUserPassword, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$"))
            {
                PasswordInputClass = "is-danger";
                PasswordWarn =
                    "<span class='has-text-danger'><i class='fas fa-exclamation-triangle'></i>" +
                    "&ensp;Your password needs to contain at least 8 characters with a number.</span>";

                abortFormSubmission = true;
            }

            // If phone number is invalid 
            if (NewUserAccount.PhoneNumber.Length != 8 ||
                NewUserAccount.PhoneNumber[0] != '8' && NewUserAccount.PhoneNumber[0] != '9')
            {
                PhoneInputClass = "is-danger";
                PhoneWarn =
                    "<i class='fas fa-exclamation-triangle'></i>" +
                    "&ensp;The phone number you provided is not valid.<br>";

                abortFormSubmission = true;
            }

            // Check whether phone number has already been used
            if (dbUsers.All(new {NewUserAccount.PhoneNumber}).ToList().Count == 1)
            {
                PhoneInputClass = "is-danger";
                PhoneWarn =
                    "<i class='fas fa-exclamation-triangle'></i>" +
                    "&ensp;This phone number is already linked to an account. <a href='/Accounts/Login'>Login instead?</a><br>";

                abortFormSubmission = true;
            }

            if (abortFormSubmission) return Page();

            // If all is well, set the password
            NewUserAccount.SetPassword(NewUserPassword);

            // Push user object to database
            dbUsers.Insert(NewUserAccount);

            // Generate new OTP code and insert into DB
            var otpRecord = new ActionLog
            {
                Uid = NewUserAccount.Uid,
                ActionExecuted = "account_otp_verify_sent",
                Metadata = Rnd.Next(100000, 999999).ToString(),
                Info = "This is a system action for account verification."
            };
            dbActionLogs.Insert(otpRecord);

            // Send SMS
            NewUserAccount.SendSms(
                "You are almost done creating your KampongTalk account." +
                "We need you to verify this phone number in order to complete the creation of your account.\n" +
                $"Please enter {otpRecord.Metadata} into the website."
            );

            // Set the session
            HttpContext.Session.SetString("CurrentUser", NewUserAccount.ToJson());
            // HttpContext.Session.SetString("OTPPending", "true");
            
            // Add action to logs
            dbActionLogs.Insert(new ActionLog()
            {
                Uid = NewUserAccount.Uid,
                ActionExecuted = "account_create",
                Metadata = null,
                Info = "Your account was created and this record documents this."
            });

            // Redirect to OTP page
            return RedirectToPage("Verify");
        }
    }
}