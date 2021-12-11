using IdGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KampongTalk.Models
{
	public class Post
	{
		public static IdGenerator generator = new IdGenerator(0);
		public long Pid { get; set; } = generator.CreateId();

		public long Author { get; set; }
		
		public String Content { get; set; }

		public String Attachment_img { get; set; }

		public int Count_upvote { get; set; }

		// List of user ids 
		public String Liked_by { get; set; }
		
		public int Count_reshare { get; set; }

		public DateTime Timestamp { get; set; }

		// Will contain keywords from the content in the array
		public String Relevancy { get; set; }

		// List of Community FKeys
		public String In_community { get; set; }

		// If obj is post, leave as empty string
        // Else if obj is a comment, set as Pid
		public String Is_comment { get; set; }

	}
}
