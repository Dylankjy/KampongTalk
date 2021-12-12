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
        public static IdGenerator generator = new IdGenerator(0);

        public string ChatId { get; set; }

        //uid of authorised chat users
        public string UsersId { get; set; }
    }
}
