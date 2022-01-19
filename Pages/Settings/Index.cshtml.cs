using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Settings
{
    public class Index : PageModel
    {
        public static MightyOrm prefDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "UserPreferences", "Uid");

        // Saved in the db. Use this to render the current attributes
        public dynamic currentUserPreference { get; set; }

        public string displayLang { get; set; }

        private User CurrentUser { get; set; }

        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");

            currentUserPreference = prefDb.Single($"Uid = {CurrentUser.Uid}");

            switch (currentUserPreference.Language)
            {
                case "en":
                    displayLang = "English";
                    break;
                case "zh":
                    displayLang = "中文 ";
                    break;
                case "ms":
                    displayLang = "Malay";
                    break;
                case "ta":
                    displayLang = "Tamil";
                    break;
            }

            return Page();
        }
    }
}