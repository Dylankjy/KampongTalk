using IdGen;

namespace KampongTalk.Models
{
    public class ChatRooms
    {
        public long ChatId { get; set; } = new IdGenerator(0).CreateId();


        //uid of authorised chat users
        public string UsersId { get; set; }
    }
}