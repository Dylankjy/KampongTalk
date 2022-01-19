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

        public string displayTextSize { get; set; }

        public string displaySpeechGender { get; set; }

        public string displayAudioCues { get; set; }

        public string displayPasswordLess { get; set; }



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
                    displayLang = "Melayu";
                    break;
                case "ta":
                    displayLang = "தமிழ்";
                    break;
            }

            switch (currentUserPreference.TextSize)
            {
                case "large":
                    displayTextSize = "Large";
                    break;
                case "larger":
                    displayTextSize = "Larger ";
                    break;
                case "largest":
                    displayTextSize = "Largest";
                    break;
            }

            switch (currentUserPreference.SpeechGender)
            {
                case "Male":
                    displaySpeechGender = "Male";
                    break;
                case "Female":
                    displaySpeechGender = "Female ";
                    break;
               
            }

            switch (currentUserPreference.UseAudioCues)
            {
                case true:
                    displayAudioCues = "On";
                    break;
                case false:
                    displayAudioCues = "Off";
                    break;

            }

            switch (currentUserPreference.UsePasswordLess)
            {
                case true:
                    displayPasswordLess = "On";
                    break;
                case false:
                    displayPasswordLess = "Off";
                    break;

            }


            return Page();
        }
    }
}