using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Events
{
    public class MyEventsModel : PageModel
    {
        public static long userId { get; set; } = 7;
        public static MightyOrm eventDb { get; set; } = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");
        
        public IEnumerable<dynamic> myCreatedEvents { get; set; } = eventDb.All($"CreatorId = {userId}");
        public IEnumerable<dynamic> mySignedUpEvents { get; set; } = eventDb.All();
        public IEnumerable<dynamic> myPastEvents { get; set; } = eventDb.All();

        public void OnGet()
        {

        }
    }
}
