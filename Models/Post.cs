using System;
using IdGen;

namespace KampongTalk.Models
{
    public class Post
    {
        public long Pid { get; set; } = new IdGenerator(1).CreateId();

        public long Author { get; set; }

        public string Content { get; set; }

        public string AttachmentImg { get; set; }

        public DateTime Timestamp { get; set; }
        
        // This is plural.
        public string TaggedUsers { get; set; }

        // Community Id
        public long InCommunity { get; set; }

        // If obj is post, leave as empty string
        // Else if obj is a comment, set as Pid
        public long IsComment { get; set; }
    }
}