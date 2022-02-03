using System;
using Flurl;
using Flurl.Http;

namespace KampongTalk.Models
{
    public class Relevancy
    {
        // This field can be any Id (Post, User, Event, Community, etc.)
        public string EntityId { get; set; }
        public string Keyword { get; set; }
        
        // Type of word
        // Noun/Verb
        public string Type { get; set; }
        
        public int Weight { get; set; }
        public DateTime Timestamp { get; set; }
    }
}