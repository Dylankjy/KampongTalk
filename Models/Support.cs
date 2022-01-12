using Microsoft.AspNetCore.Mvc;

namespace KampongTalk.Models
{
    public class Support : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}