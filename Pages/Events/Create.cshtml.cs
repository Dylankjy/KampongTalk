using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KampongTalk.Pages.Events
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public Event myEvent { get; set; }

        public IEnumerable<SelectListItem> ListOfTimeIntervals
        {
            get
            {
                var list = new List<SelectListItem>();

                // range of hours, multiplied by 4 (e.g. 24 hours = 96)
                int timeRange = 33;

                // range of minutes, e.g. 15 min
                int minuteRange = 30;

                // starting time, e.g. 0:00
                TimeSpan startTime = new TimeSpan(6, 0, 0);

                // placeholder
                list.Add(new SelectListItem { Text = "Choose a time", Value = "0", Disabled = true });

                // get standard ticks
                DateTime startDate = new DateTime(DateTime.MinValue.Ticks);

                // create time format based on range above
                for (int i = 0; i < timeRange; i++)
                {
                    int minutesAdded = minuteRange * i;
                    TimeSpan timeAdded = new TimeSpan(0, minutesAdded, 0);
                    TimeSpan tm = startTime.Add(timeAdded);

                    var hours = tm.Hours;
                    var amPm = "AM";

                    if (hours == 12)
                        amPm = "PM";

                    else if (hours > 12)
                    {
                        var timeMinus = new TimeSpan(-12, 0, 0);
                        tm = tm.Add(timeMinus);
                        amPm = "PM";
                    }

                    DateTime result = startDate + tm;

                    var textResult = result.ToString("HH:mm") + amPm;


                    list.Add(new SelectListItem { Text = textResult, Value = textResult });
                }

                return list;
            }
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine(myEvent);
            }
            return Page();
        }
    }
}
