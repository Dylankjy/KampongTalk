using System;
using System.ComponentModel.DataAnnotations;
using IdGen;
using Mighty;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace KampongTalk.Models
{
    [DatabaseTable("Users")]
    public class User
    {
        [DatabasePrimaryKey] [DatabaseColumn] public long Uid { get; set; } = new IdGenerator(0).CreateId();
        [DatabaseColumn] [Required] public string Uid2 { get; set; }
        
        [DatabaseColumn] [Required] public string PhoneNumber { get; set; }

        [DatabaseColumn] [Required] public string Name { get; set; }

        [DatabaseColumn] [Required] public string Password { get; set; }


        [DatabaseColumn] public string Bio { get; set; }
        [DatabaseColumn] public string AvatarImg { get; set; } = "default.jpg";
        [DatabaseColumn] public string Interests { get; set; }
        [DatabaseColumn] public string Challenges { get; set; }
        [DatabaseColumn] public DateTime DateOfBirth { get; set; }
        [DatabaseColumn] public bool IsVerified { get; set; }
        [DatabaseColumn] public string TextSize { get; set; }
        [DatabaseColumn] public string Language { get; set; }
        [DatabaseColumn] public string SpeechGender { get; set; }
        

        public void SendSms(string messageContent)
        {
            string accountSid = ConfigurationManager.AppSetting["APIKeys:Twilio:SID"];
            string authToken = ConfigurationManager.AppSetting["APIKeys:Twilio:Secret"];

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: $"[KampongTalk]\n{messageContent}",
                from: new Twilio.Types.PhoneNumber("+19377876066"),
                to: new Twilio.Types.PhoneNumber($"+65{PhoneNumber}")
            );
        }

        public void SetNewUid2(string name)
        {
            // Check which variable to use, afterwards, trim spaces.
            name = name.Replace(" ", String.Empty);

            // Shorten names that are longer than 5 characters
            string part1 = name;
            if (name.Length > 5)
            {
                part1 = name[..5];
            }
            
            // Generate discriminator
            Random Rnd = new Random();
            string part2 = Rnd.Next(1000, 9999).ToString();

            Uid2 = $"{part1}_{part2}";
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
            if (obj == null)
            {
                return new User{ Uid = -1 };
            }
            
            return new User
            {
                Uid = obj.Uid,
                Uid2 = obj.Uid2,
                PhoneNumber = obj.PhoneNumber,
                Name = obj.Name,
                Password = obj.Password,
                Bio = obj.Bio,
                AvatarImg = obj.AvatarImg,
                Interests = obj.Interests,
                Challenges = obj.Challenges,
                DateOfBirth = obj.DateOfBirth,
                IsVerified = obj.IsVerified,
                TextSize = obj.TextSize,
                Language = obj.Language,
                SpeechGender = obj.SpeechGender,
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