using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using IdGen;
using KampongTalk.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace KampongTalk.Pages.Profile
{
    public class Index : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public Index(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // Current user prop
        public User CurrentUser { get; set; }

        // Profile props
        public dynamic ViewingUser { get; set; }
        public string JoinDate { get; set; }
        public bool IsCurrentUserOwnPage { get; set; }

        // Profile image prop
        [BindProperty] public IFormFile ProfileImage { get; set; }
        [BindProperty] public IFormFile BannerImage { get; set; }

        // Posts
        public IEnumerable<dynamic> PostsToDisplay { get; set; }
        public int PostCount { get; set; }
        
        // Page number
        public int PageNo { get; set; }
        public int PreviousPageNo { get; set; }
        public int NextPageNo { get; set; }

        // Profile edit props
        [BindProperty] [Required] public string EditName { get; set; }
        [BindProperty] public string EditBio { get; set; }
        [BindProperty] public string EditBirthday { get; set; }
        
        // Friend props
        public List<dynamic> FriendList { get; set; } = new List<dynamic>();
        public List<dynamic> FriendListPendingOutgoing { get; set; } = new List<dynamic>();
        public List<dynamic> FriendListPendingIncoming { get; set; } = new List<dynamic>();
        public int FriendCount { get; set; }
        [BindProperty] public string FriendAction { get; set; }
        [BindProperty] public string FriendActionOtherUid { get; set; }
        public string FriendActionToDisplay { get; set; }
        public bool IsOpenModalOnGet { get; set; }

        // Error handling props
        public bool ShowUserNotFoundError { get; set; } = false;

        public IActionResult OnGet(string u, int p, int showFriendsModal)
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // Database declarations
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");

            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");

            var dbPost =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Post");
            
            var dbRelation =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Relationships");

            // Detect whether to open friend modal automatically
            try
            {
                IsOpenModalOnGet = Convert.ToBoolean(showFriendsModal);
            }
            catch
            {
                IsOpenModalOnGet = false;
            }
            

            // Get user by PhoneNumber
            ViewingUser = dbUsers.Single(new
            {
                Uid2 = u
            });

            // If User doesn't exist, show error page
            if (ViewingUser == null)
            {
                ShowUserNotFoundError = true;
                return Page();
            }

            DateTime joinDateRaw = dbActionLogs.Single(new
            {
                ViewingUser.Uid,
                ActionExecuted = "account_create"
            }).Timestamp;

            string[] months =
            {
                "January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
                "November", "December"
            };

            JoinDate = $"{months[joinDateRaw.Month - 1]} {joinDateRaw.Year}";

            // Set owner flag
            if (CurrentUser != null && CurrentUser.Uid.ToString() == ViewingUser.Uid.ToString())
                IsCurrentUserOwnPage = true;

            // Get posts by this user from database
            var postsByThisUser = dbPost.All(where: $"Author = {ViewingUser.Uid} AND IsComment = 0");

            var numberOfObjectsPerPage = 10;
            var posts = postsByThisUser.ToList();
            PostsToDisplay = posts.Skip(numberOfObjectsPerPage * p)
                .Take(numberOfObjectsPerPage).OrderBy(post => post.Timestamp);
            PostCount = posts.Count();

            // Set default value for textarea, Bio
            EditBio = ViewingUser.Bio;
            
            // Set page number
            PageNo = p;
            PreviousPageNo = p - 1;
            NextPageNo = p + 1;
            
            // Get friend list
            var friendList =
                dbRelation.Query(
                    $"Select * from Relationships where (UserA = '{ViewingUser.Uid}' or UserB = '{ViewingUser.Uid}') AND Status = 'friends'");

            if (friendList != null) {
                foreach (var relationRow in friendList)
                {
                    if (relationRow.UserA != ViewingUser.Uid)
                    {
                        FriendList.Add(UserApi.GetUserById(relationRow.UserA));
                    }
                    else
                    {
                        FriendList.Add(UserApi.GetUserById(relationRow.UserB));
                    }
                }
                // ReSharper disable once PossibleMultipleEnumeration
                FriendCount = friendList.Count();
            }
            
            var friendListPendingOutgoing =
                dbRelation.Query(
                    $"Select * from Relationships where UserA = '{ViewingUser.Uid}' AND Status = 'pending'");
            
            if (friendListPendingOutgoing != null) {
                foreach (var relationRow in friendListPendingOutgoing)
                {
                    if (relationRow.UserA != ViewingUser.Uid)
                    {
                        FriendListPendingOutgoing.Add(UserApi.GetUserById(relationRow.UserA));
                    }
                    else
                    {
                        FriendListPendingOutgoing.Add(UserApi.GetUserById(relationRow.UserB));
                    }
                }
            }
            
            var friendListPendingIncoming =
                dbRelation.Query(
                    $"Select * from Relationships where UserB = '{ViewingUser.Uid}' AND Status = 'pending'");
            
            if (friendListPendingIncoming != null) {
                foreach (var relationRow in friendListPendingIncoming)
                {
                    if (relationRow.UserA != ViewingUser.Uid)
                    {
                        FriendListPendingIncoming.Add(UserApi.GetUserById(relationRow.UserA));
                    }
                    else
                    {
                        FriendListPendingIncoming.Add(UserApi.GetUserById(relationRow.UserB));
                    }
                }
            }

            // Show friend control buttons
            if (!IsCurrentUserOwnPage && CurrentUser != null)
            {
                var whereSelfIsInvoker = dbRelation.Single(new {UserA = CurrentUser.Uid, UserB = ViewingUser.Uid});
                var whereOtherIsInvoker = dbRelation.Single(new {UserB = CurrentUser.Uid, UserA = ViewingUser.Uid});

                // No relations
                if (whereOtherIsInvoker == null && whereSelfIsInvoker == null)
                {
                    FriendActionToDisplay = "SHOW_ADD";
                } else {
                    // Where the other user is the invoker
                    if (whereOtherIsInvoker != null && whereSelfIsInvoker == null)
                    {
                        if (whereOtherIsInvoker.Status == "pending") {
                            FriendActionToDisplay = "SHOW_PENDING_ACCEPTABLE";
                        }
                        
                        if (whereOtherIsInvoker.Status == "friends") {
                            FriendActionToDisplay = "SHOW_ADDED";
                        }
                    }
                    // Where self is the invoker
                    if (whereOtherIsInvoker == null && whereSelfIsInvoker != null)
                    {
                        if (whereSelfIsInvoker.Status == "pending")
                        {
                            FriendActionToDisplay = "SHOW_PENDING";
                        }
                        
                        if (whereSelfIsInvoker.Status == "friends") {
                            FriendActionToDisplay = "SHOW_ADDED";
                        }
                    }
                }
            }

            return Page();
        }

        public IActionResult OnPost(string u)
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // If current user null, don't bother
            // Check account existence
            if (CurrentUser == null) return RedirectToPage("/Accounts/Login");

            // Database declarations
            var dbActionLogs =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "ActionLogs");
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users", "Uid");
            
            // Very stupid workaround for this shit because SQL doesn't want to add because "MISSING RID". Please shoot me!
            var dbRelationAdd =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Relationships");
            var dbRelation =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Relationships", "Rid");

            // Get current user dynamic object from database
            var currentUserFromDb = dbUsers.Single(new {CurrentUser.Uid, CurrentUser.Uid2});

            // Check account existence
            if (currentUserFromDb == null) return RedirectToPage("/Accounts/Login");
            
            // Check whether if this is friend form
            if (FriendAction != null)
            {
                // Check whether FriendActionOtherUid is valid
                try
                {
                    if (UserApi.GetUserById(Convert.ToInt64(FriendActionOtherUid)) == null)
                    {
                        return Redirect($"/Profile?u={u}");
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    return Redirect($"/Profile?u={u}");
                }
                
                
                switch (FriendAction)
                {
                    case "ADD":
                        var existingRelation = dbRelation.Single(new { UserA = Convert.ToInt64(FriendActionOtherUid), UserB = CurrentUser.Uid });

                        if (existingRelation != null)
                        {
                            existingRelation.Status = "friends";
                            dbRelation.Update(existingRelation);
                        }
                        else
                        {
                            dbRelationAdd.Insert(new Relationships
                            {
                                UserA = CurrentUser.Uid,
                                UserB = Convert.ToInt64(FriendActionOtherUid),
                            });
                        }
                        break;
                    case "CANCEL_REQ":
                        dbRelation.Delete($"UserA = {CurrentUser.Uid} AND UserB = {FriendActionOtherUid} AND Status = 'pending'");
                        break;
                    case "REMOVE_FRIEND":
                        dbRelation.Delete($"UserA = {CurrentUser.Uid} AND UserB = {FriendActionOtherUid} AND Status = 'friends'");
                        break;
                }

                return Redirect($"/Profile?u={u}");
            }

            // Modify user
            // ReSharper disable once PossibleNullReferenceException
            // Linting suggestion addressed above.
            currentUserFromDb.Name = EditName;
            currentUserFromDb.Bio = EditBio;

            // Check if birthday is already set, if it hasn't set it
            if (currentUserFromDb.DateOfBirth == null) currentUserFromDb.DateOfBirth = EditBirthday;

            // Profile image
            if (ProfileImage != null)
            {
                var imgExt = ProfileImage.FileName.Split('.').Last();
                var genImgNum = new IdGenerator(64).CreateId();
                var genImgName = genImgNum + "." + imgExt;
                var file = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", genImgName);
                var genImgNameOriginal = genImgNum + "-original." + imgExt;
                var fileOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", genImgNameOriginal);

                using (var fileStream = new FileStream(fileOriginal, FileMode.Create))
                {
                    ProfileImage.CopyTo(fileStream);
                }

                using (var image = Image.Load(fileOriginal))
                {
                    var ratio = Convert.ToDouble(image.Height) / Convert.ToDouble(image.Width);
                    var width = 384;
                    var height = (int) Math.Round(384 * ratio, 0);
                    image.Mutate(x => x.Resize(width, height));

                    image.Save(file);
                }

                // if current image is not default.jpg, we will delete on our end
                if (currentUserFromDb.AvatarImg != "default.jpg")
                {
                    string oldAvatarImg = currentUserFromDb.AvatarImg;
                    var oldAvatarImgOriginal =
                        oldAvatarImg.Split(".").First() + "-original." + oldAvatarImg.Split(".").Last();

                    var currentImgPath = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", oldAvatarImg);
                    var currentImgPathOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata",
                        oldAvatarImgOriginal);
                    System.IO.File.Delete(currentImgPath);
                    System.IO.File.Delete(currentImgPathOriginal);
                }

                currentUserFromDb.AvatarImg = genImgName;
            }

            if (BannerImage != null)
            {
                var imgExt = BannerImage.FileName.Split('.').Last();
                var genImgNum = new IdGenerator(65).CreateId();
                var genImgName = genImgNum + "-banner." + imgExt;
                var file = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", genImgName);
                var genImgNameOriginal = genImgNum + "-banner-original." + imgExt;
                var fileOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", genImgNameOriginal);

                using (var fileStream = new FileStream(fileOriginal, FileMode.Create))
                {
                    BannerImage.CopyTo(fileStream);
                }

                using (var image = Image.Load(fileOriginal))
                {
                    var ratio = Convert.ToDouble(image.Height) / Convert.ToDouble(image.Width);
                    var width = 1080;
                    var height = (int) Math.Round(1080 * ratio, 0);
                    image.Mutate(x => x.Resize(width, height));

                    image.Save(file);
                }

                // if current image is not default.jpg, we will delete on our end
                if (currentUserFromDb.BannerImg != "default-banner.png")
                {
                    string oldBannerImg = currentUserFromDb.BannerImg;
                    var oldBannerImgOriginal =
                        oldBannerImg.Split(".").First() + "-original." + oldBannerImg.Split(".").Last();

                    var currentImgPath = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata", oldBannerImg);
                    var currentImgPathOriginal = Path.Combine(_environment.ContentRootPath, "wwwroot/userdata",
                        oldBannerImgOriginal);
                    System.IO.File.Delete(currentImgPath);
                    System.IO.File.Delete(currentImgPathOriginal);
                }

                currentUserFromDb.BannerImg = genImgName;
            }

            // Add audit log
            var editLog = new ActionLog
            {
                Uid = currentUserFromDb.Uid,
                ActionExecuted = "profile_edit",
                Metadata = null,
                Info = "Your profile page was edited."
            };
            dbActionLogs.Insert(editLog);

            // Commit profile changes
            dbUsers.Update(currentUserFromDb);

            return Redirect($"/Profile?u={currentUserFromDb.Uid2}");
        }
    }
}