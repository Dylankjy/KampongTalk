using System;
using System.IO;
using System.Linq;
using IdGen;
using KampongTalk.Models;
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

        private User CurrentUser { get; set; }

        public dynamic OwnerUser { get; set; }

        public static MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events",
                "Eid");

        public static MightyOrm userDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Users",
                "Uid");

        public dynamic myEvent { get; set; }
        public string eventDate { get; set; }

        // IS the current user the owner of the listing
        public bool amOwner { get; set; }

        // Has the current user added the listing to their list
        public bool hasAdded { get; set; }

        // Has the event ended?
        public bool isOver { get; set; }


        [BindProperty] public IFormFile eventImage { get; set; }

        public IActionResult OnGet(string eid)
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // TO retrieve the savedEvent object from db
            Eid = Convert.ToInt64(eid);
            myEvent = eventDb.Single($"Eid = {Eid}");
            OwnerUser = userDb.Single($"Uid = {myEvent.CreatorId}");

            var eventDateTime = Convert.ToDateTime(myEvent.Date);
            eventDate = eventDateTime.ToLongDateString();

            if (DateTime.Now >= myEvent.Date) isOver = true;

            if (CurrentUser != null)
            {
                if (myEvent.Attendees.Contains(CurrentUser.Uid.ToString())) hasAdded = true;
                if (myEvent.CreatorId == CurrentUser.Uid) amOwner = true;
            }

            return Page();
        }


        public IActionResult OnPost(IFormFile eventImage)
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            var savedEvent = eventDb.Single($"Eid = {Eid}");
            if (savedEvent.CreatorId == CurrentUser.Uid)
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

        public IActionResult OnPostAddOrRemove()
        {
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));
            Eid = Convert.ToInt64(Eid);
            myEvent = eventDb.Single($"Eid = {Eid}");

            if (CurrentUser != null)
            {
                // Don't allow creator to remove his own listing
                if (myEvent.CreatorId != CurrentUser.Uid)
                {
                    var currentUid = CurrentUser.Uid.ToString();
                    // If user has already added the event, we will remove it
                    if (myEvent.Attendees.Contains(currentUid))
                        myEvent.Attendees = myEvent.Attendees.Replace($"{currentUid};", "");
                    // If user hasn't added the event, we will add it
                    else
                        myEvent.Attendees = myEvent.Attendees + $"{currentUid};";
                    eventDb.Update(myEvent);
                }

                return Redirect($"/Events/View/{Eid}");
            }

            return Redirect("/Accounts/Login");
        }
    }
}