using IdGen;
using KampongTalk.i18n;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Accounts
{
    public class Login : PageModel
    {
        // Current user prop
        private User CurrentUser { get; set; }

        // Prop declarations
        [BindProperty] public string LoginPhoneNumber { get; set; }
        [BindProperty] public string LoginPassword { get; set; }
        public string FieldClass { get; set; }
        public bool ShowErrorMessage { get; set; }

        public dynamic LangData { get; } = Internationalisation.LoadLanguage("jp");

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
            return RedirectToPage("/Index");
        }
    }
}