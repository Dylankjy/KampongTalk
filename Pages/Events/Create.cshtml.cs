using System;
using System.Collections.Generic;
using System.Globalization;
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

        public string timeErrMsg { get; set; }

        public MightyOrm eventDb { get; set; } = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");
        // public IEnumerable<SelectListItem> ListOfTimeIntervals = TimeEnum.getListOfTimeIntervals();
        public IEnumerable<SelectListItem> ListOfStartTimeIntervals { get; set; } = TimeEnum.getListOfTimeIntervals();
        public IEnumerable<SelectListItem> ListOfEndTimeIntervals { get; set; } = GetEndTimeEnum("06:00 AM");

        public void OnGet()
        {
        }

        public static IEnumerable<SelectListItem> GetEndTimeEnum(string startTime)
        {
            return TimeEnum.getListOfTimeIntervalsAfter(startTime);
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {

                DateTime startDateTime = DateTime.ParseExact(myEvent.StartTime, "hh:mm tt", CultureInfo.InvariantCulture);
                DateTime endDateTime = DateTime.ParseExact(myEvent.EndTime, "hh:mm tt", CultureInfo.InvariantCulture);
                if (endDateTime > startDateTime)
                {
                    TimeSpan startTimeSpan = startDateTime.TimeOfDay;
                    TimeSpan endTimeSpan = endDateTime.TimeOfDay;

                    // TO render the correct DateTime & save in DB (Take the end time as the timing)
                    DateTime dtDate = Convert.ToDateTime(myEvent.Date);
                    string renderedDate = dtDate.ToString("yyyy-MM-dd");
                    DateTime dt = Convert.ToDateTime(renderedDate + " " + myEvent.EndTime);

                    myEvent.Date = dt;
                    myEvent.CreatorId = 8;
                    myEvent.Attendees = "";
                    myEvent.Duration = Convert.ToString(endTimeSpan.TotalHours - startTimeSpan.TotalHours);
                    eventDb.Insert(myEvent);
                    return Redirect("/Events/MyEvents");
                }
                else
                {
                    timeErrMsg = "End Time must be later than Start time";
                    return Page();
                }
            }
            return Page();
        }
    }
}
