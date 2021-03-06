using System;
using System.Collections.Generic;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Events.MyEvents
{
    public class MyCreatedEventsModel : PageModel
    {
        public static DateTime nowDt = DateTime.Now;
        public static string nowDtString = nowDt.ToString("yyyy-MM-dd HH:mm:ss");
        private User CurrentUser { get; set; }
        public dynamic LangData { get; set; }
        public static MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");

        public IEnumerable<dynamic> myCreatedEvents { get; set; }


        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");
            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            myCreatedEvents = eventDb.All($"CreatorId = {CurrentUser.Uid}");
            return Page();
        }
    }
}