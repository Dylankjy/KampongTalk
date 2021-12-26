using System;
using System.IO;
using System.Linq;
using IdGen;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Events
{
    public class ViewModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public ViewModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [BindProperty] public long Eid { get; set; }

        [BindProperty] public long UserId { get; set; } = 8;

        public static MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events",
                "Eid");

        public dynamic myEvent { get; set; }
        public string eventDate { get; set; }


        [BindProperty] public IFormFile eventImage { get; set; }

        public void OnGet(string eid)
        {
            // TO retrieve the savedEvent object from db
            Eid = Convert.ToInt64(eid);
            myEvent = eventDb.Single($"Eid = {Eid}");
            var eventDateTime = Convert.ToDateTime(myEvent.Date);
            eventDate = eventDateTime.ToLongDateString();
        }


        public IActionResult OnPost(IFormFile eventImage)
        {
            var savedEvent = eventDb.Single($"Eid = {Eid}");
            if (savedEvent.CreatorId == UserId)
            {
                var imgExt = eventImage.FileName.Split('.').Last();
                var genImgNum = new IdGenerator(2).CreateId();
                var genImgName = genImgNum + "." + imgExt;
                var file = Path.Combine(_environment.ContentRootPath, "wwwroot/imgs", genImgName);
                using (var fileStream = new FileStream(file, FileMode.Create))
                {
                    eventImage.CopyTo(fileStream);
                }

                // if current image is not default.jpg, we will delete on our end
                if (savedEvent.Img != "default.jpg")
                {
                    var currentImgPath = Path.Combine(_environment.ContentRootPath, "wwwroot/imgs", savedEvent.Img);
                    System.IO.File.Delete(currentImgPath);
                }

                savedEvent.Img = genImgName;
                eventDb.Update(savedEvent);
                return Redirect($"/Events/View/{Eid}");
            }

            return Page();
        }
    }
}