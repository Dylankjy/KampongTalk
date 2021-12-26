using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Events.MyEvents
{
    public class IndexModel : PageModel
    {
        public static DateTime nowDt = DateTime.Now;
        public static string nowDtString = nowDt.ToString("yyyy-MM-dd HH:mm:ss");
        public static long userId { get; set; } = 8;

        public static MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");

        public IEnumerable<dynamic> myUpcomingEvents { get; set; } =
            eventDb.All($"Attendees like '%{userId}%' AND Date > '{nowDtString}'");

        public IEnumerable<dynamic> mySignedUpEvents { get; set; } = eventDb.All();
        public IEnumerable<dynamic> myPastEvents { get; set; } = eventDb.All();

        public void OnGet()
        {
        }
    }
}