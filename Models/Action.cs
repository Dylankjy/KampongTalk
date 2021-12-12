using System;
using IdGen;

namespace KampongTalk.Models
{
    public class Action
    {
        public long ActionId { get; set; } = new IdGenerator(0).CreateId();

        // Set as the user who performed or relates to the action. If the system performed it, it will be 0
        public long UserId { get; set; } = 0;

        // Set as the action performed
        public string ActionExecuted { get; set; }

        // This field should be human readable. Avoid using this field for processing as content may differ
        public string Info { get; set; }

        // This field determines whether this action should be shown in the notification view of the user
        // Set this as false after the user has read it
        public bool IsActiveNotification { get; set; } = false;

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}