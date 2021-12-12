using System;
using IdGen;

namespace KampongTalk.Models
{
    public class User
    {
        public long Uid { get; set; } = new IdGenerator(0).CreateId();
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public string AvatarImg { get; set; }
        public string Interests { get; set; }
        public string Challenges { get; set; }
    }
}