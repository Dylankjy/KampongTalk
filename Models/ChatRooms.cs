using IdGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KampongTalk.Models
{
    public class ChatRooms
    {
        public long ChatId { get; set; } = new IdGenerator(0).CreateId();


        //uid of authorised chat users
        public string UsersId { get; set; }
    }
}
