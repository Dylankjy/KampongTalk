using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Dms
{
    public class DmModel : PageModel
    {

        [BindProperty] public ChatMessage newChat { get; set; } = new ChatMessage();

        public User CurrentUser { get; set; }

        DateTime localDate = DateTime.Now;

        public dynamic ReceivingUser { get; set; }

        public dynamic LangData { get; set; }

        // public bool ShowUserNotFoundError { get; set; }


        public MightyOrm dbChat{get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "ChatMessages");


        public IEnumerable<dynamic> myChat { get; set; }


        public IActionResult OnGet(string to)
        {
            // Database declarations
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");



            //Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");

            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            myChat = dbChat.All($"(SenderID = '{CurrentUser.Uid}' AND RecipientId = '{Convert.ToInt64(to)}') OR (SenderID = '{Convert.ToInt64(to)}' AND RecipientId = '{CurrentUser.Uid}')");

            // Get user by PhoneNumber

            ReceivingUser = dbUsers.Single(new
            {
                Uid = Convert.ToInt64(to)
           });

            // If User doesn't exist, show error page

            if (ReceivingUser == null)
           {
               // ShowUserNotFoundError = true;
                return Page();
            }

            return Page();
        }


        public IActionResult OnPost(string to)
        {

            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");


            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            //CurrentUser = new User
            //{
            //   Uid = 932911279016771584,
            //  Name = "test1",
            //  AvatarImg = "default.jpg"
            //};

            newChat.SenderId = CurrentUser.Uid;

                newChat.Timestamp = localDate;

                newChat.RecipientId = Convert.ToInt64(to);

                dbChat.Insert(newChat);

                return Redirect ($"/Dms/Dm?to={to}");
            

            
        }

    }
}
