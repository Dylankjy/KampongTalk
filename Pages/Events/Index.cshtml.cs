using System;
using System.Collections.Generic;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Events
{
    public class IndexModel : PageModel
    {
        public static DateTime nowDt = DateTime.Now;
        public static string nowDtString = nowDt.ToString("yyyy-MM-dd HH:mm:ss");

        public static MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");

        public IEnumerable<dynamic> allEvents { get; set; } = eventDb.All($"Date > '{nowDtString}'");

        private User CurrentUser { get; set; }
        public dynamic LangData { get; set; }
        
        public void OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            LangData = UserPrefApi.GetLangByUid(CurrentUser);
        }
    }
}