using IdGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KampongTalk.Models
{
    public class Event
    {
        public long Eid { get; set; } = new IdGenerator(2).CreateId();

        public long CreatorId { get; set; }

        public string Attendees { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public String Time { get; set; }

        [Required]
        public string Duration { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string Description { get; set; }

        public string Img { get; set; } = "default.jpg";


    }
}
