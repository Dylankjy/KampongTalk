using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Mighty;

namespace KampongTalk.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        // Use dynamic here, not User. Because MightyORM does not return a User object
        public dynamic user { get; set; }
        public string textSize { get; set; }

        public void OnGet()
        {
            // Create User here (DONT SPECIFY Uid in MightyOrm Constructor here)
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

            var db = new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                "Users");

            textSize = "larger";
        }
    }
}