using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KampongTalk.Pages.Accounts
{
    public class Logout : PageModel
    {
        public IActionResult OnGet()
        {
            return Redirect("/");
        }
        
        public IActionResult OnPost()
        {
            HttpContext.Session.Clear();
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            return Redirect("/");
        }
    }
}