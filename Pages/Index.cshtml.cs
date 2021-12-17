using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Mighty;
using KampongTalk.Models;
using IdGen;

namespace KampongTalk.Pages
{
    public class IndexModel : PageModel
    {
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


            var connectionString = "ProviderName=MySql.Data.MySqlClient;Server=175.156.158.50;Port=50000;Database=kampongtalk;Uid=kamponguser;Pwd=HelloWorld!!1";
            var db = new MightyOrm(connectionString, "Users", "Uid");
            // db.Insert(user);
            user = db.Single(921304464873226240);
        }
    }
}
