using IdGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

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
