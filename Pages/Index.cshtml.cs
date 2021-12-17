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
        // Use dynamic here, not User. Because MightyORM does not return a User object
        public dynamic user { get; set; }

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Create User here (DONT SPECIFY Uid here)
            // user = new User()
            // {
            //     PhoneNumber = "91223198",
            //     Password = "tether",
            //     Name = "My",
            //     Bio = "Man",
            //     Interests = "Nothing",
            //     Challenges = "zero"
            // };
            // var db = new MightyOrm(connectionString, "Users");
            // db.Insert(user);



            // Retrieving & Updating User (SPECIFY Uid here)
            // var db = new MightyOrm(connectionString, "Users", "Uid");
            // db.Insert(user);
            // var p = db.Single(921304464873226240);
            // p.Name = "Barry";
            // db.Update(p);

            var db = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Users");
            user = db.Single(921304464873226240);
            //var p = db.New();
        }
    }
}
