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
        public static IdGenerator generator = new IdGenerator(0);

        public string Type { get; set; }

        public string Content { get; set; }

        public string SenderId { get; set; }

        public string MessageId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
