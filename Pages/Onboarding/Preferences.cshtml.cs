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

namespace KampongTalk.Pages.Onboarding
{
    // This code was taken from Settings/Preference.cshtml.cs , done by Trumen

    public class PreferencesModel : PageModel
    {
        public static MightyOrm prefDb { get; set; } = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "UserPreferences", "Uid");

        // Update using this one
        [BindProperty]
        public UserPreferences newUserPreference { get; set; } = new UserPreferences();

        // Saved in the db. Use this to render the current attributes
        public dynamic currentUserPreference { get; set; }

        private User CurrentUser { get; set; }

        public List<SelectListItem> LangPrefList { get; set; } = new List<SelectListItem>
            {
                new SelectListItem {Text = "English", Value = "en"},
                new SelectListItem {Text = "中文", Value = "zh"},
                new SelectListItem {Text = "தமிழ்", Value = "ta"},
                new SelectListItem {Text = "Melayu", Value = "ms"},
            };

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
            currentUserPreference.TextSize = newUserPreference.TextSize;
            currentUserPreference.UseAudioCues = newUserPreference.UseAudioCues;
            prefDb.Update(currentUserPreference);
            return Redirect("/Onboarding/ProfileSettings");
        }
    }
}
