using System;
using IdGen;

namespace KampongTalk.Models
{
    public class ChatMessage
    {
        public long MessageId { get; set; } = new IdGenerator(0).CreateId();
        
        public long RecipientId { get; set; }

        public string Content { get; set; }

        public long SenderId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}