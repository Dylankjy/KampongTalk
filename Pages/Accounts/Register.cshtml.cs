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
    public class Register : PageModel
    {
        // Other stuff
        private static readonly Random Rnd = new Random();

        // Current user prop
        public User CurrentUser { get; set; }

        // Language prop
        public dynamic LangData { get; set; }
        private static dynamic LangDataStatic { get; set; }

        // Prop declarations
        [BindProperty] public User NewUserAccount { get; set; }
        [BindProperty] public string NewUserPassword { get; set; }

        public string PasswordWarn { get; set; }

        public string PasswordInputClass { get; set; } = string.Empty;
        public string PhoneWarn { get; set; }
        public string PhoneInputClass { get; set; }

        // --------------------------------------

        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // If the current user is verified, naturally, the object is present, so just redirect them.
            // Don't return RedirectToPage("Index"). This will throw an error. Use Redirect("/") instead
            if (CurrentUser is {IsVerified: true}) return Redirect("/");

            // If the user has not OTP verified
            if (CurrentUser is {IsVerified: false}) return RedirectToPage("Verify");
            
            // Set language data
            LangData = Internationalisation.LoadLanguage(HttpContext.Request.GetTypedHeaders().AcceptLanguage
                .First().ToString().Split("-").First());
            LangDataStatic = Internationalisation.LoadLanguage(HttpContext.Request.GetTypedHeaders().AcceptLanguage
                .First().ToString().Split("-").First());

            PasswordWarn = LangDataStatic.accounts.register.passwordHint;

            // And then show them the page
            // If verification is not yet done, the verification screen will show up
            // else the registration form is shown.
            return Page();
        }

        public IActionResult OnPost()
        {
            // Set language data
            LangData = Internationalisation.LoadLanguage(HttpContext.Request.GetTypedHeaders().AcceptLanguage
                .First().ToString().Split("-").First());
            LangDataStatic = LangData;
            PasswordWarn = LangDataStatic.accounts.register.passwordHint;
            
            // Database declarations
            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");
            var dbPrefs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "UserPreferences");

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
                PasswordWarn = LangDataStatic.accounts.register.passwordNotSufficient;
                abortFormSubmission = true;
            }

            // If phone number is invalid 
            if (NewUserAccount.PhoneNumber.Length != 8 || !NewUserAccount.PhoneNumber.All(char.IsDigit) ||
                NewUserAccount.PhoneNumber[0] != '8' && NewUserAccount.PhoneNumber[0] != '9')
            {
                PhoneInputClass = "is-danger";
                PhoneWarn = LangDataStatic.accounts.register.phoneInvalid;

                abortFormSubmission = true;
            }

            // Check whether phone number has already been used
            if (dbUsers.All(new {NewUserAccount.PhoneNumber}).ToList().Count == 1)
            {
                PhoneInputClass = "is-danger";
                PhoneWarn = LangDataStatic.accounts.register.phoneAlreadyInUse;

                abortFormSubmission = true;
            }

            if (abortFormSubmission) return Page();

            // If all is well, set the password
            NewUserAccount.SetPassword(NewUserPassword);
            NewUserAccount.SetNewUid2(NewUserAccount.Name);

            var userPrefs = new UserPreferences
            {
                Uid = NewUserAccount.Uid
            };

            // Push user object to database
            dbUsers.Insert(NewUserAccount);
            dbPrefs.Insert(userPrefs);


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
            string i18nVerifyPhone = LangDataStatic.smsMessages.verifyPhone;
            NewUserAccount.SendSms(i18nVerifyPhone
                .Replace("{selectedUser.Name}", NewUserAccount.Name)
                .Replace("{otpRecord.Metadata}", otpRecord.Metadata)
            );

            // Set the session
            HttpContext.Session.SetString("CurrentUser", NewUserAccount.ToJson());
            // HttpContext.Session.SetString("OTPPending", "true");

            // Add action to logs
            dbActionLogs.Insert(new ActionLog
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