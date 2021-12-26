using System;
using System.Collections.Generic;
using System.Globalization;
using KampongTalk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mighty;

namespace KampongTalk.Pages.Events
{
    public class EditModel : PageModel
    {
        [BindProperty] public Event myEvent { get; set; }

        public dynamic savedEvent { get; set; }

        [BindProperty] public double Eid { get; set; }

        public string eventDate { get; set; }

        public string timeErrMsg { get; set; }

        public MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events",
                "Eid");

        // public IEnumerable<SelectListItem> ListOfTimeIntervals = TimeEnum.getListOfTimeIntervals();
        public IEnumerable<SelectListItem> ListOfStartTimeIntervals { get; set; } = TimeEnum.getListOfTimeIntervals();
        public IEnumerable<SelectListItem> ListOfEndTimeIntervals { get; set; } = GetEndTimeEnum("06:00 AM");

        public void OnGet(string eid)
        {
            Eid = Convert.ToInt64(eid);
            savedEvent = eventDb.Single($"Eid = {Eid}");
            DateTime dt = Convert.ToDateTime(savedEvent.Date);
            eventDate = dt.ToString("yyyy-MM-dd");
        }

        public static IEnumerable<SelectListItem> GetEndTimeEnum(string startTime)
        {
            return TimeEnum.getListOfTimeIntervalsAfter(startTime);
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                var startDateTime = DateTime.ParseExact(myEvent.StartTime, "hh:mm tt", CultureInfo.InvariantCulture);
                var endDateTime = DateTime.ParseExact(myEvent.EndTime, "hh:mm tt", CultureInfo.InvariantCulture);
                var startTimeSpan = startDateTime.TimeOfDay;
                var endTimeSpan = endDateTime.TimeOfDay;
                if (endDateTime > startDateTime)
                {
                    savedEvent = eventDb.Single($"Eid = {Eid}");
                    savedEvent.Name = myEvent.Name;
                    savedEvent.Location = myEvent.Location;
                    savedEvent.StartTime = myEvent.StartTime;
                    savedEvent.EndTime = myEvent.EndTime;

                    // TO render the correct DateTime & save in DB (Take the end time as the timing)
                    var dtDate = Convert.ToDateTime(myEvent.Date);
                    var renderedDate = dtDate.ToString("yyyy-MM-dd");
                    var dt = Convert.ToDateTime(renderedDate + " " + myEvent.EndTime);

                    savedEvent.Date = dt;

                    savedEvent.Duration = Convert.ToString(endTimeSpan.TotalHours - startTimeSpan.TotalHours);
                    eventDb.Update(savedEvent);
                    return Redirect($"/Events/View/{savedEvent.Eid}");
                }
                else
                {
                    timeErrMsg = "End Time must be later than Start time";
                    savedEvent = eventDb.Single($"Eid = {Eid}");
                    DateTime dt = Convert.ToDateTime(savedEvent.Date);
                    eventDate = dt.ToString("yyyy-MM-dd");
                    return Page();
                }
            }

            return Page();
        }
    }
}