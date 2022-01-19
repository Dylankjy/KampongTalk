using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KampongTalk.Pages.DigitalVoidDeck
{
    public class ChatroomModel : PageModel
    {

        public User CurrentUser { get; set; }

        public void OnGet()
        {
        }
    }
}
