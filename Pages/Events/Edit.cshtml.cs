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
    public class EditModel : PageModel
    {
        [BindProperty] public Event myEvent { get; set; }

        public dynamic savedEvent { get; set; }

        [BindProperty] public double Eid { get; set; }

        public dynamic LangData { get; set; }

        public string eventDate { get; set; }

        public string timeErrMsg { get; set; }

        public MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events",
                "Eid");

        private User CurrentUser { get; set; }

        // public IEnumerable<SelectListItem> ListOfTimeIntervals = TimeEnum.getListOfTimeIntervals();
        public IEnumerable<SelectListItem> ListOfStartTimeIntervals { get; set; } = TimeEnum.getListOfTimeIntervals();
        public IEnumerable<SelectListItem> ListOfEndTimeIntervals { get; set; } = GetEndTimeEnum("06:00 AM");

        public static IEnumerable<SelectListItem> GetEndTimeEnum(string startTime)
        {
            return TimeEnum.getListOfTimeIntervalsAfter(startTime);
        }

        public IActionResult OnGet(string eid)
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");

            LangData = UserPrefApi.GetLangByUid(CurrentUser); 

            Eid = Convert.ToInt64(eid);
            savedEvent = eventDb.Single($"Eid = {Eid}");

            if (savedEvent.CreatorId == CurrentUser.Uid)
            {
                // Don't allow users to edit events that are already over
                if (DateTime.Now >= savedEvent.Date) return Redirect($"/Events/View/{eid}");

                DateTime dt = Convert.ToDateTime(savedEvent.Date);
                eventDate = dt.ToString("yyyy-MM-dd");
                return Page();
            }

            return Redirect($"/Events/View/{eid}");
        }

        public IActionResult OnPost()
        {
            // Ensure user is authorized to edit
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");
            Eid = Convert.ToInt64(Eid);
            savedEvent = eventDb.Single($"Eid = {Eid}");
            if (savedEvent.CreatorId != CurrentUser.Uid) return Redirect("/Accounts/Login");

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
                    savedEvent.Description = myEvent.Description;

                    // TO render the correct DateTime & save in DB (Take the end time as the timing)
                    var dtDate = Convert.ToDateTime(myEvent.Date);
                    var renderedDate = dtDate.ToString("yyyy-MM-dd");
                    var dt = Convert.ToDateTime(renderedDate + " " + myEvent.EndTime);
                    savedEvent.Date = dt;
                    savedEvent.Duration = Convert.ToString(endTimeSpan.TotalHours - startTimeSpan.TotalHours);
                    
                    // SearchApi.PutKeyword(savedEvent.Name, 5, savedEvent.Eid);
                    // SearchApi.PutRelevancy(savedEvent.Description, savedEvent.Eid);
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