using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Events
{
    public class ViewModel : PageModel
    {
        public long Eid { get; set; }
        public static MightyOrm eventDb { get; set; } = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");
        public dynamic myEvent { get; set; }

        public void OnGet(string eid)
        {
            Eid = Convert.ToInt64(eid);
            myEvent = eventDb.Single($"Eid = {Eid}");
        }
    }
}
