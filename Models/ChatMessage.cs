using System;
using IdGen;

namespace KampongTalk.Models
{
    public class ChatMessage
    {
        public long MessageId { get; set; } = new IdGenerator(0).CreateId();

        //message type
        public string Type { get; set; }

        public string Content { get; set; }

        public string SenderId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}