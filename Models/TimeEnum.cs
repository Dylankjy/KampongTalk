using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace KampongTalk.Models
{
    public class TimeEnum
    {
        public List<SelectListItem> list { get; set; }

        public static IEnumerable<SelectListItem> getListOfTimeIntervals()
        {
            var list = new List<SelectListItem>();

            // We want to display 16 hours worth. 16*2 + 1 = 33
            int timeRange = 33;

            // range of minutes to increment by, e.g. 30 min
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

                var textResult = result.ToString("HH:mm") + $" {amPm}";


                list.Add(new SelectListItem { Text = textResult, Value = textResult });
            }

            return list;
        }


        public static IEnumerable<SelectListItem> getListOfTimeIntervalsAfter(string userSelectedStartTime)
        {
            var list = new List<SelectListItem>();

            DateTime startDateTime = DateTime.ParseExact(userSelectedStartTime, "hh:mm tt", CultureInfo.InvariantCulture);

            // starting time, e.g. 0:00
            TimeSpan startTime = startDateTime.TimeOfDay;

            // Add 30 minutes (For the very first diplay for end time)
            startTime = startTime.Add(new TimeSpan(0, 30, 0));
            TimeSpan endTime = new TimeSpan(22, 0, 0);

            double hourDiff = endTime.TotalHours - startTime.TotalHours;
            // range of hours, multiplied by 2 (e.g. 24 hours = 48)
            int timeRange = Convert.ToInt32(hourDiff * 2);

            // range of minutes, e.g. 15 min
            int minuteRange = 30;

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

                var textResult = result.ToString("HH:mm") + $" {amPm}";


                list.Add(new SelectListItem { Text = textResult, Value = textResult });
            }

            return list;
        }
    }
}
