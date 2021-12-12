using IdGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KampongTalk.Models
{
	public class Post
	{
		public long Pid { get; set; } = new IdGenerator(1).CreateId();

		public long Author { get; set; }
		
		public String Content { get; set; }

		public String AttachmentImg { get; set; }

		public int CountUpvote { get; set; }

		// List of user ids 
		public String LikedBy { get; set; }
		
		public int CountReShare { get; set; }

		public DateTime Timestamp { get; set; }

		// Will contain keywords from the content in the array
		public String Relevancy { get; set; }

		// List of Community FKeys
		public String InCommunity { get; set; }

		// If obj is post, leave as empty string
        // Else if obj is a comment, set as Pid
		public String IsComment { get; set; }

	}
}
