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

        

        public List<SelectListItem> LangPrefList { get; set; } = new List<SelectListItem>
            {
                new SelectListItem {Text = "English", Value = "en"},
                new SelectListItem {Text = "中文", Value = "zh"},
                new SelectListItem {Text = "தமிழ்", Value = "ta"},
                new SelectListItem {Text = "Melayu", Value = "ms"},
            };

        public List<SelectListItem> TextPrefSize { get; set; } = new List<SelectListItem>
            {
                new SelectListItem {Text = "large", Value = "large"},
                new SelectListItem {Text = "larger", Value = "larger"},
                new SelectListItem {Text = "largest", Value = "largest"},
              
            };

        public List<SelectListItem> SpeechGender { get; set; } = new List<SelectListItem>
            {
                new SelectListItem {Text = "Male", Value = "Male"},
                new SelectListItem {Text = "Female", Value = "Female"},
             
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
            currentUserPreference.TextSize = newUserPreference.TextSize;
            currentUserPreference.SpeechGender = newUserPreference.SpeechGender;
            currentUserPreference.UseAudioCues = newUserPreference.UseAudioCues;
            currentUserPreference.UsePasswordLess = newUserPreference.UsePasswordLess;
            prefDb.Update(currentUserPreference);
            return Redirect("/Settings/Index");
        }
    }
}
