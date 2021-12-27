using System;
using System.ComponentModel.DataAnnotations;
using IdGen;
using Mighty;
using Newtonsoft.Json;

namespace KampongTalk.Models
{
    [DatabaseTable("Users")]
    public class User
    {
        [DatabasePrimaryKey] [DatabaseColumn] public long Uid { get; set; } = new IdGenerator(0).CreateId();

        [DatabaseColumn] [Required] public string PhoneNumber { get; set; }

        [DatabaseColumn] [Required] public string Name { get; set; }

        [DatabaseColumn] [Required] public string Password { get; set; }


        [DatabaseColumn] public string Bio { get; set; }
        [DatabaseColumn] public string AvatarImg { get; set; } = "default.jpg";
        [DatabaseColumn] public string Interests { get; set; }
        [DatabaseColumn] public string Challenges { get; set; }
        [DatabaseColumn] public DateTime DateOfBirth { get; set; }
        [DatabaseColumn] public bool IsVerified { get; set; }

        public void SendSms(string messageContent)
        {
            // TODO: Add Twilio API
        }

        public void SetPassword(string plainText)
        {
            Password = BCrypt.Net.BCrypt.HashPassword(plainText, 12, true);
        }

        public bool ComparePassword(string incomingText)
        {
            var isPasswordMatching = BCrypt.Net.BCrypt.EnhancedVerify(incomingText, Password);
            return isPasswordMatching;
        }

        public User ToUser(dynamic obj)
        {
            return new User
            {
                Uid = obj.Uid,
                PhoneNumber = obj.PhoneNumber,
                Name = obj.Name,
                Password = obj.Password,
                Bio = obj.Bio,
                AvatarImg = obj.AvatarImg,
                Interests = obj.Interests,
                Challenges = obj.Challenges,
                DateOfBirth = obj.DateOfBirth,
                IsVerified = obj.IsVerified
            };
        }

        // Usage for ToJson and FromJSON
        // To retrieve the data from session:
        // CurrentUser = new User().FromJson(HttpContext.Session.GetString("user"));
        // To set data to session:
        // HttpContext.Session.SetString("CurrentUser", NewUserAccount.ToJson());
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public User FromJson(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<User>(jsonString);
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }
    }
}