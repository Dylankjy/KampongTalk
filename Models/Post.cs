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

        public int CountUpvote { get; set; }

        // List of user ids 
        public string LikedBy { get; set; }

        public int CountReShare { get; set; }

        public DateTime Timestamp { get; set; }

        // List of Community FKeys
        public string InCommunity { get; set; }

        // If obj is post, leave as empty string
        // Else if obj is a comment, set as Pid
        public long IsComment { get; set; }
    }
}