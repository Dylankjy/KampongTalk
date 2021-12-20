using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Events
{
    public class IndexModel : PageModel
    {
        public static MightyOrm eventDb { get; set; } = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");
        public IEnumerable<dynamic> allEvents { get; set; } = eventDb.All();

        public void OnGet()
        {
        }
    }
}
