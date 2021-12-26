using System;
using IdGen;

namespace KampongTalk.Models
{
    public class Challenges
    {
        public long Cid { get; set; } = new IdGenerator(3).CreateId();

        public DateTime Date { get; set; }

        public string ChallengePool { get; set; }

        public string SelectedChallenge { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}