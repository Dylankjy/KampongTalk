using System;
using System.ComponentModel.DataAnnotations;
using IdGen;

namespace KampongTalk.Models
{
    public class Community
    {
        public string Cid { get; set; }
        [Required] public string Name { get; set; }
        public long CreatorId { get; set; }
        public string IconImg { get; set; } = "default_community.png";
        [Required] public string Description { get; set; }
        public DateTime TimeCreated { get; set; } = DateTime.Now;

        public void SetCid()
        {
            Cid = $"{Name.Replace(" ", "").ToLower()}";
        }
    }
}