using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Mighty;
using KampongTalk.Models;
using IdGen;
using Microsoft.Extensions.Configuration;

namespace KampongTalk.Pages
{
    public class IndexModel : PageModel
    {
        public User user { get; set; }

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            user = new User()
            {
                PhoneNumber = "91223198",
                Password = "tet",
                Name = "Nn",
                Bio = "tt",
                Interests = "ttt",
                Challenges = "e"
            };

            var db = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Users");
            db.Insert(user);
            //var p = db.New();
        }
    }
}
