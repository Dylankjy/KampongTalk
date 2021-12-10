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
        public static IdGenerator generator = new IdGenerator(0);

        public long Eid { get; set; } = generator.CreateId();

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

        public string img { get; set; } = "default.jpg";


    }
}
