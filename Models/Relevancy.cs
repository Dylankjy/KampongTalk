using System;

namespace KampongTalk.Models
{
    public class Relevancy
    {
        // This field can be any Id (Post, User, Event, Community, etc.)
        public long EntityId { get; set; }
        public string Keyword { get; set; }
        public int Weight { get; set; }
        public DateTime Timestamp { get; set; }
    }
}