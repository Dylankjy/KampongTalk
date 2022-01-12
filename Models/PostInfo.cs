using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KampongTalk.Models
{
    public class PostInfo
    {

        public long Pid { get; set; }
        public string Content { get; set; }
        public string AttachmentImg { get; set; }
        public DateTime Timestamp { get; set; }
        public long InCommunity { get; set; }
        public string CommunityName { get; set; }
        public long IsComment { get; set; }
        public string TaggedUsers { get; set; }
        public long Uid { get; set; }
        public string UserName { get; set; }
        public string UserPfp { get; set; }
        public int likeCount { get; set; } = 0;
        public string likeString { get; set; }
        // IsLiked
    }
}
