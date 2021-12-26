using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KampongTalk.Models
{
    public class TimeEnum
    {
        public List<SelectListItem> list { get; set; }

        public static IEnumerable<SelectListItem> getListOfTimeIntervals()
        {
            var list = new List<SelectListItem>();

            // We want to display 16 hours worth. 16*2 + 1 = 33
            var timeRange = 33;

            // range of minutes to increment by, e.g. 30 min
            var minuteRange = 30;

            // starting time, e.g. 0:00
            var startTime = new TimeSpan(6, 0, 0);

            // placeholder
            list.Add(new SelectListItem {Text = "Choose a time", Value = "0", Disabled = true});

            // get standard ticks
            var startDate = new DateTime(DateTime.MinValue.Ticks);

            // create time format based on range above
            for (var i = 0; i < timeRange; i++)
            {
                var minutesAdded = minuteRange * i;
                var timeAdded = new TimeSpan(0, minutesAdded, 0);
                var tm = startTime.Add(timeAdded);

                var hours = tm.Hours;
                var amPm = "AM";

                if (hours == 12)
                {
                    amPm = "PM";
                }

                else if (hours > 12)
                {
                    var timeMinus = new TimeSpan(-12, 0, 0);
                    tm = tm.Add(timeMinus);
                    amPm = "PM";
                }

                var result = startDate + tm;

                var textResult = result.ToString("HH:mm") + $" {amPm}";


                list.Add(new SelectListItem {Text = textResult, Value = textResult});
            }

            return list;
        }


        public static IEnumerable<SelectListItem> getListOfTimeIntervalsAfter(string userSelectedStartTime)
        {
            var list = new List<SelectListItem>();

            var startDateTime = DateTime.ParseExact(userSelectedStartTime, "hh:mm tt", CultureInfo.InvariantCulture);

            // starting time, e.g. 0:00
            var startTime = startDateTime.TimeOfDay;

            // Add 30 minutes (For the very first diplay for end time)
            startTime = startTime.Add(new TimeSpan(0, 30, 0));
            var endTime = new TimeSpan(22, 0, 0);

            var hourDiff = endTime.TotalHours - startTime.TotalHours;
            // range of hours, multiplied by 2 (e.g. 24 hours = 48)
            var timeRange = Convert.ToInt32(hourDiff * 2);

            // range of minutes, e.g. 15 min
            var minuteRange = 30;

            // placeholder
            list.Add(new SelectListItem {Text = "Choose a time", Value = "0", Disabled = true});

            // get standard ticks
            var startDate = new DateTime(DateTime.MinValue.Ticks);

            // create time format based on range above
            for (var i = 0; i < timeRange; i++)
            {
                var minutesAdded = minuteRange * i;
                var timeAdded = new TimeSpan(0, minutesAdded, 0);
                var tm = startTime.Add(timeAdded);

                var hours = tm.Hours;
                var amPm = "AM";

                if (hours == 12)
                {
                    amPm = "PM";
                }

                else if (hours > 12)
                {
                    var timeMinus = new TimeSpan(-12, 0, 0);
                    tm = tm.Add(timeMinus);
                    amPm = "PM";
                }

                var result = startDate + tm;

                var textResult = result.ToString("HH:mm") + $" {amPm}";


                list.Add(new SelectListItem {Text = textResult, Value = textResult});
            }

            return list;
        }
    }
}