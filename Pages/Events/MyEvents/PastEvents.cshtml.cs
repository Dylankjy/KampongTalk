using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Events.MyEvents
{
    public class PastEventsModel : PageModel
    {
        public static long userId { get; set; } = 8;
        public static MightyOrm eventDb { get; set; } = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");

        public static DateTime nowDt = DateTime.Now;
        public static string nowDtString = nowDt.ToString("yyyy-MM-dd HH:mm:ss");

        public IEnumerable<dynamic> myPastEvents { get; set; } = eventDb.All($"Attendees like '%{userId}%' AND Date <= '{nowDtString}'");
        public void OnGet()
        {
        }
    }
}