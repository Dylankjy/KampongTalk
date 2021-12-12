using System;
using IdGen;

namespace KampongTalk.Models
{
    public class Community
    {
        public long Cid { get; set; } = new IdGenerator(0).CreateId();
        public string Name { get; set; }
        public long CreatorId { get; set; }
        public string IconImg { get; set; }
        public string Description { get; set; }
        public DateTime TimeCreated { get; set; } = DateTime.Now;
    }
}