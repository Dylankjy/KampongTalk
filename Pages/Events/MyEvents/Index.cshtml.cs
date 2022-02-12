using System;
using System.Collections.Generic;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Events.MyEvents
{
    public class IndexModel : PageModel
    {
        public static DateTime nowDt = DateTime.Now;
        public static string nowDtString = nowDt.ToString("yyyy-MM-dd HH:mm:ss");

        public long userId { get; set; }
        public dynamic LangData { get; set; }

        public static MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");

        public IEnumerable<dynamic> myUpcomingEvents { get; set; }


        private User CurrentUser { get; set; }

        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");
            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            userId = CurrentUser.Uid;
            myUpcomingEvents = eventDb.All($"Attendees like '%{CurrentUser.Uid}%' AND Date > '{nowDtString}'");
            return Page();
        }
    }
}