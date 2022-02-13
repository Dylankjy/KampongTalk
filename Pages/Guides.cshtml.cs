using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages
{
    public class GuidesModel : PageModel
    {
        public User CurrentUser { get; set; }
        public dynamic LangData { get; set; }

        public IActionResult OnGet()
        {
            var db = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                "Users");

            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null)
            {
                //CurrentUser = new User();
                return Redirect("/Accounts/Login");
            }
            LangData = UserPrefApi.GetLangByUid(CurrentUser);
            return Page();
        }
    }
}
