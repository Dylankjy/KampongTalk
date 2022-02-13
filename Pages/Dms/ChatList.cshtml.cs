using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;
using MoreLinq;
using MoreLinq.Extensions;

namespace KampongTalk.Pages.Dms
{
    public class ChatListModel : PageModel
    {
        public static MightyOrm userDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Users");
        public static MightyOrm chatDB { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "ChatMessages");

        public User CurrentUser { get; set; }
        public dynamic LangData { get; set; }
        public List<dynamic> ChatList { get; set; } = new List<dynamic>();


        public IActionResult OnGet()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null){ return Redirect("/Accounts/Login"); }
            LangData = UserPrefApi.GetLangByUid(CurrentUser);
            getChatList();
            return Page();
        }

        public void getChatList()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            dynamic subList1 = chatDB.All(where: $"SenderId = {CurrentUser.Uid}", orderBy: "Timestamp", columns: "RecipientId, Content, Timestamp")
                .GroupBy(c => c.RecipientId)
                .Select(g => g.First())
                .ToList(); 
            dynamic subList2 = chatDB.All(where: $"RecipientId = {CurrentUser.Uid}", orderBy: "Timestamp", columns: "SenderId, Content, Timestamp")
                .GroupBy(c => c.SenderId)
                .Select(g => g.First())
                .ToList();

            foreach (dynamic c in subList1) { c.OUid = c.RecipientId; }
            foreach (dynamic c in subList2) { c.OUid = c.SenderId; }

            if (subList1 != null) { ChatList.AddRange(subList1); }
            if (subList2 != null) { ChatList.AddRange(subList2); }

            ChatList = ChatList.GroupBy(c => c.OUid).Select(g => g.First()).OrderBy(c => c.Timestamp).ToList();

        }

    }
}
