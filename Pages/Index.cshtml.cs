using System;
using System.Linq;
using IdGen;
using KampongTalk.i18n;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages
{
    public class IndexModel : PageModel
    {
        // private readonly ILogger<IndexModel> _logger;
        //
        // public IndexModel(ILogger<IndexModel> logger)
        // {
        //     _logger = logger;
        // }

        // Use dynamic here, not User. Because MightyORM does not return a User object
        public User CurrentUser { get; set; }

        public string textSize { get; set; }

        public bool useAudioCues { get; set; }

        public dynamic LangData { get; set; }

        public bool IsLoggedIn { get; set; }


        // Prop declarations
        [BindProperty] public string LoginPhoneNumber { get; set; }
        [BindProperty] public string LoginPassword { get; set; }
        public string FieldClass { get; set; }
        public bool ShowErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var db = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                "Users");

            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // Set user language
            if (CurrentUser == null)
            {
                LangData = Internationalisation.LoadLanguage(HttpContext.Request.GetTypedHeaders().AcceptLanguage
                    .First().ToString().Split("-").First());
            }
            else
            {
                LangData = UserPrefApi.GetLangByUid(CurrentUser);
            }

            if (CurrentUser == null)
            {
                IsLoggedIn = false;
                return Page();
            }

            IsLoggedIn = true;

            var preferencesDB =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "UserPreferences");

            var uid = CurrentUser.Uid;
            var loggedInUserPrefs = preferencesDB.Single($"Uid = {uid}");
            useAudioCues = loggedInUserPrefs.UseAudioCues;

            textSize = "larger";

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

            var dbPrefs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "UserPreferences");

            // Block OnPost if user is verified and already authenticated
            var currentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (currentUser is {IsVerified: true}) return RedirectToPage("Index");

            if (currentUser == null)
            {
                LangData = Internationalisation.LoadLanguage(HttpContext.Request.GetTypedHeaders().AcceptLanguage
                    .First().ToString().Split("-").First());
            }
            else
            {
                LangData = UserPrefApi.GetLangByUid(CurrentUser);
            }
            

            // Get user by PhoneNumber
            var selectedUserFromDb = dbUsers.Single(new
            {
                PhoneNumber = LoginPhoneNumber
            });

            // If user doesn't exist, initialise new user to prevent timing attacks
            if (selectedUserFromDb == null)
            {
                selectedUserFromDb = new User();
                selectedUserFromDb.SetPassword(new IdGenerator(0).CreateId().ToString());
            }

            // Convert dynamic to user object
            User selectedUser = new User().ToUser(selectedUserFromDb);

            // Compare password
            if (!selectedUser.ComparePassword(LoginPassword))
            {
                ShowErrorMessage = true;
                FieldClass = "is-danger";
                return Page();
            }

            // If all is well, set the user into session
            // Set the session
            HttpContext.Session.SetString("CurrentUser", selectedUser.ToJson());

            // Redirect to verification page if unverified
            if (!selectedUser.IsVerified) return RedirectToPage("Verify");

            // Generate new OTP code and insert into DB
            var otpRecord = new ActionLog
            {
                Uid = selectedUser.Uid,
                ActionExecuted = "account_login_success",
                Metadata = null,
                Info = "Successful login attempt performed on your account."
            };
            dbActionLogs.Insert(otpRecord);

            // Else, go to index
            return Redirect("/");
        }
    }
}