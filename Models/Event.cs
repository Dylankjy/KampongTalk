using System;
using System.ComponentModel.DataAnnotations;
using IdGen;
using Mighty;

namespace KampongTalk.Models
{
    public class Event
    {
        public long Eid { get; set; } = new IdGenerator(2).CreateId();

        public long CreatorId { get; set; }

        public string Attendees { get; set; }

        [Required] public string Name { get; set; }

        [Required] public DateTime Date { get; set; }

        [Required] public string StartTime { get; set; }

        [Required] public string EndTime { get; set; }

        public string Duration { get; set; }

        [Required] public string Location { get; set; }

        [Required] public string Description { get; set; }

        public string Img { get; set; } = "default.jpg";
    }
    
    public static class EventApi 
    {
        public static dynamic GetEventById(long id)
        {
            var dbEvents =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Events");

            return dbEvents.Single(new {Eid = id});
        }
    }
}