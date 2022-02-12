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
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                "UserPreferences", "Uid");

        // Saved in the db. Use this to render the current attributes
        public dynamic CurrentUserPreference { get; set; }

        public string DisplayLang { get; set; }

        public string DisplayTextSize { get; set; }

        public string DisplaySpeechGender { get; set; }

        public string DisplayAudioCues { get; set; }

        public string DisplayPasswordLess { get; set; }


        public User CurrentUser { get; set; }
        
        // Form type
        [BindProperty] public string ForForm { get; set; }
        
        // KampongID Props
        [BindProperty] public string NewKampongId { get; set; }
        [BindProperty] public string IncomingPassword { get; set; }
        public bool ShowDuplicateUid2Error { get; set; }
        public bool ShowCurrentUid2Error { get; set; }
        public bool ShowPasswordError { get; set; }
        public string ShowDuplicateUid2ErrorClass { get; set; }
        public string ShowPasswordErrorClass { get; set; }
        
        // Modal auto open prop
        public string AutoOpenModalId { get; set; }

        public IActionResult OnGet(string openModal)
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");

            CurrentUserPreference = prefDb.Single($"Uid = {CurrentUser.Uid}");

            switch (CurrentUserPreference.Language)
            {
                case "en":
                    DisplayLang = "English";
                    break;
                case "zh":
                    DisplayLang = "中文 ";
                    break;
                case "ms":
                    DisplayLang = "Melayu";
                    break;
                case "ta":
                    DisplayLang = "தமிழ்";
                    break;
            }

            switch (CurrentUserPreference.TextSize)
            {
                case "large":
                    DisplayTextSize = "Large";
                    break;
                case "larger":
                    DisplayTextSize = "Larger ";
                    break;
                case "largest":
                    DisplayTextSize = "Largest";
                    break;
            }

            switch (CurrentUserPreference.SpeechGender)
            {
                case "Male":
                    DisplaySpeechGender = "Male";
                    break;
                case "Female":
                    DisplaySpeechGender = "Female ";
                    break;
            }

            switch (CurrentUserPreference.UseAudioCues)
            {
                case true:
                    DisplayAudioCues = "On";
                    break;
                case false:
                    DisplayAudioCues = "Off";
                    break;
            }

            switch (CurrentUserPreference.UsePasswordLess)
            {
                case true:
                    DisplayPasswordLess = "On";
                    break;
                case false:
                    DisplayPasswordLess = "Off";
                    break;
            }
            
            // Automatically open modal
            if (openModal != null)
            {
                if (openModal == "kampongId")
                {
                    AutoOpenModalId = "changeKampongIDModal";
                }
                // if () {}
            }
            
            return Page();
        }

        public IActionResult OnPost()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");
            
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users", "Uid");

            var currentUserFromDb = dbUsers.Single(new { CurrentUser.Uid });

            // ReSharper disable once StringLiteralTypo
            if (ForForm == "KAMPONGID_CHANGE")
            {
                // Check if user is trying to change to their own Uid2
                if (CurrentUser.Uid2 == NewKampongId)
                {
                    ShowCurrentUid2Error = true;
                    ShowDuplicateUid2ErrorClass = "is-danger";
                    AutoOpenModalId = "changeKampongIDModal";
                    return Page();
                }
                
                // Check if there is an account already with the same Uid2
                var userWithSameUid2 = dbUsers.Single(new {Uid2 = NewKampongId});
                if (userWithSameUid2 != null)
                {
                    ShowDuplicateUid2ErrorClass = "is-danger";
                    ShowDuplicateUid2Error = true;
                    AutoOpenModalId = "changeKampongIDModal";
                    return Page();
                }
                
                // If password is wrong
                if (!CurrentUser.ComparePassword(IncomingPassword))
                {
                    ShowPasswordErrorClass = "is-danger";
                    ShowPasswordError = true;
                    AutoOpenModalId = "changeKampongIDModal";
                    return Page();
                }

                // Update values
                CurrentUser.Uid2 = NewKampongId;
                currentUserFromDb.Uid2 = NewKampongId;
            
                // Commit values
                HttpContext.Session.SetString("CurrentUser", CurrentUser.ToJson());
                dbUsers.Update(currentUserFromDb);
                
                return Page();
            }
            
            return Page();
        }
    }
}