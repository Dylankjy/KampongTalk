using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mighty;

namespace KampongTalk.Pages.Settings
{
    public class PreferenceModel : PageModel
    {
        public static MightyOrm prefDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "UserPreferences", "Uid");

        public IEnumerable<dynamic> myUpcomingEvents { get; set; }

        public List<SelectListItem> LangPrefList { get; set; } = new List<SelectListItem>
            {
                new SelectListItem {Text = "English", Value = "en"},
                new SelectListItem {Text = "中文", Value = "zh"},
                new SelectListItem {Text = "Tamil", Value = "ta"},
                new SelectListItem {Text = "Malay", Value = "ms"},
            };

        // Saved in the db. Use this to render the current attributes
        public dynamic currentUserPreference { get; set; }
        
        // Update using this one
        [BindProperty]
        public UserPreferences newUserPreference { get; set; } = new UserPreferences();

        private User CurrentUser { get; set; }

        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");

            currentUserPreference = prefDb.Single($"Uid = {CurrentUser.Uid}");

            return Page();
        }

        public IActionResult OnPost()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            // currentUserPreference = new UserPreferences().ToUserPreferences(prefDb.Single($"Uid = {CurrentUser.Uid}"));
            currentUserPreference = prefDb.Single($"Uid = {CurrentUser.Uid}");
            currentUserPreference.Language = newUserPreference.Language;
            prefDb.Update(currentUserPreference);
            return Redirect("/Settings/Index");
        }
    }
}
