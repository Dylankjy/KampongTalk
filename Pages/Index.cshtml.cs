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

            var connectionString = "ProviderName=MySql.Data.MySqlClient;Server=175.156.158.50;Port=50000;Database=kampongtalk;Uid=kamponguser;Pwd=HelloWorld!!1";
            // var connectionString = "ProviderName=System.Data.SqlClient;data source =175.156.158.50:50000;database =kampongtalk;user id =kamponguser;password = HelloWorld!!1;persist security info = false;";
            // var connectionString = "ProviderName=System.Data.SqlClient;Data Source=175.156.158.50:50000;Initial Catalog = kampongtalk; User ID = kamponguser; Password = HelloWorld!!1";
            var db = new MightyOrm(connectionString, "Users", "Uid");
            db.Insert(user);
            //var p = db.New();
        }
    }
}
