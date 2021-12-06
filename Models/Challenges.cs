using IdGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KampongTalk.Models
{
    public class Challenges
    {
        public static IdGenerator generator = new IdGenerator(0);

        public long Cid { get; set; } = generator.CreateId();

        public DateTime Date { get; set; }

        public string ChallengePool { get; set; }

        public string SelectedChallenge { get; set; }
    }
}
