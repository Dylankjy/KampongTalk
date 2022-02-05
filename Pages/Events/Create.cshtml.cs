using System;
using System.Collections.Generic;
using System.Globalization;
using KampongTalk.Models;
using KampongTalk.Pages.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mighty;

namespace KampongTalk.Pages.Events
{
    public class CreateModel : PageModel
    {
        [BindProperty] public Event myEvent { get; set; } = new Event();

        private User CurrentUser { get; set; }


        public string timeErrMsg { get; set; }

        public MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");

        // public IEnumerable<SelectListItem> ListOfTimeIntervals = TimeEnum.getListOfTimeIntervals();
        public IEnumerable<SelectListItem> ListOfStartTimeIntervals { get; set; } = TimeEnum.getListOfTimeIntervals();
        public IEnumerable<SelectListItem> ListOfEndTimeIntervals { get; set; } = GetEndTimeEnum("06:00 AM");

        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");

            return Page();
        }

        public static IEnumerable<SelectListItem> GetEndTimeEnum(string startTime)
        {
            return TimeEnum.getListOfTimeIntervalsAfter(startTime);
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
                var startDateTime = DateTime.ParseExact(myEvent.StartTime, "hh:mm tt", CultureInfo.InvariantCulture);
                var endDateTime = DateTime.ParseExact(myEvent.EndTime, "hh:mm tt", CultureInfo.InvariantCulture);
                if (endDateTime > startDateTime)
                {
                    var startTimeSpan = startDateTime.TimeOfDay;
                    var endTimeSpan = endDateTime.TimeOfDay;

                    // TO render the correct DateTime & save in DB (Take the end time as the timing)
                    var dtDate = Convert.ToDateTime(myEvent.Date);
                    var renderedDate = dtDate.ToString("yyyy-MM-dd");
                    var dt = Convert.ToDateTime(renderedDate + " " + myEvent.EndTime);

                    myEvent.Date = dt;
                    myEvent.CreatorId = CurrentUser.Uid;
                    myEvent.Attendees = $"{myEvent.CreatorId};";
                    myEvent.Duration = Convert.ToString(endTimeSpan.TotalHours - startTimeSpan.TotalHours);
                    SearchApi.PutRelevancy($"{myEvent.Name} {myEvent.Description}", myEvent.Eid);
                    eventDb.Insert(myEvent);
                    return Redirect("/Events/MyEvents");
                }

                timeErrMsg = "End Time must be later than Start time";
                return Page();
            }

            return Page();
        }
    }
}