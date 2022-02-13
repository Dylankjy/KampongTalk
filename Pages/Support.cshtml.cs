using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Threading.Tasks;

namespace KampongTalk.Pages
{
    public class SupportModel : PageModel
    {
        public IActionResult OnGet()
        {
            //Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            if (CurrentUser == null) return Redirect("/Accounts/Login");

            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            return Page();
        }


        [BindProperty]
        public Email sendmail { get; set; }

        public User CurrentUser { get; set; }

        public dynamic LangData { get; set; }

        public IActionResult OnPost()
        {
            
            string Subject = sendmail.Subject;
            string Body = sendmail.Body;
            
            MailMessage mm = new MailMessage();

            mm.To.Add("projectkampongtalk@gmail.com");
            mm.Subject = Subject;
            mm.Body= " ENQUIRY: \n  " + Body;
            mm.IsBodyHtml = false;
            mm.From = new MailAddress("projectkampongtalk@gmail.com");

            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.Port = 587;
            smtp.UseDefaultCredentials = true;
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential("projectkampongtalk@gmail.com", "yesyes123!");
            smtp.SendMailAsync(mm);
            

            return Redirect("/Success");

        }



    }
}