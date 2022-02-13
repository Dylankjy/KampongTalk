using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KampongTalk.Pages.DigitalVoidDeck
{
    public class Chatroom3Model : PageModel
    {

        public User CurrentUser { get; set; }

        public dynamic LangData { get; set; }

        public IActionResult OnGet()
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");

            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            return Page();
        }
    }
}
