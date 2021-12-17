using IdGen;
using Microsoft.VisualBasic;
using Mighty;

namespace KampongTalk.Models
{
    [DatabaseTable("Users")]
    public class User
    {
        [DatabasePrimaryKey]
        [DatabaseColumn]
        public long Uid { get; set; } = new IdGenerator(0).CreateId();
        [DatabaseColumn]
        public string PhoneNumber { get; set; }
        [DatabaseColumn]
        public string Password { get; set; }
        [DatabaseColumn]
        public string Name { get; set; }
        [DatabaseColumn]
        public string Bio { get; set; }
        [DatabaseColumn]
        public string AvatarImg { get; set; } = "default.jpg";
        [DatabaseColumn]
        public string Interests { get; set; }
        [DatabaseColumn]
        public string Challenges { get; set; }
    }
}