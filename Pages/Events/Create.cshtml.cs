using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mighty;

namespace KampongTalk.Pages.Events
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public Event myEvent { get; set; } = new Event();

        public MightyOrm eventDb { get; set; } = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");
        public IEnumerable<SelectListItem> ListOfTimeIntervals = TimeEnum.getListOfTimeIntervals();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                myEvent.CreatorId = 8;
                myEvent.Attendees = "";
                eventDb.Insert(myEvent);
                return Redirect("/Events/MyEvents");
            }
            return Page();
        }
    }
}
