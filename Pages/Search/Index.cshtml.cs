using System;
using System.Collections.Generic;
using System.Linq;
using KampongTalk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Search
{
    public class Index : PageModel
    {
        // Current user prop
        public User CurrentUser { get; set; }

        // User preferences prop
        public dynamic LangData { get; set; }

        // Search result prop
        public List<dynamic> SearchResultPosts { get; set; } = new List<dynamic>();
        public List<dynamic> SearchResultCommunities { get; set; } = new List<dynamic>();
        public List<dynamic> SearchResultEvents { get; set; } = new List<dynamic>();
        public List<dynamic> SearchResultUsers { get; set; } = new List<dynamic>();
        public string SearchField { get; set; }

        public IActionResult OnGet(string q)
        {
            // Get current user
            CurrentUser = new User().FromJson(HttpContext.Session.GetString("CurrentUser"));

            // Get user preferences
            LangData = UserPrefApi.GetLangByUid(CurrentUser);

            SearchField = q;
            
            // If searchfield null
            if (SearchField == null)
            {
                return Page();
            }

            // Detect whether kampongid 
            if (q.StartsWith("@")) return Redirect($"/Profile?u={q.Normalize().Replace("@", "")}");
            // Detect whether community id
            if (q.StartsWith("#")) return Redirect($"/Community?c={q.Normalize().Replace("#", "")}");

            var result = SearchApi.GetSearchResults(q).Result;

            List<SearchResultEntry> calculatedWeightResults = CalculateWeight(result);
            calculatedWeightResults = calculatedWeightResults.OrderBy(i => i.Weight).Reverse().ToList();

            foreach (var entityObj in calculatedWeightResults)
            {
                var thisObjectType = SearchApi.GetEntityTypeByEid(entityObj.EntityId);

                if (thisObjectType == "post")
                {
                    var thisPost = PostApi.GetPostByPid(long.Parse(entityObj.EntityId));
                    if (thisPost.InCommunity != null)
                    {
                        SearchResultPosts.Add(thisPost);
                    } else if (RelApi.IsAccessible(CurrentUser.Uid, thisPost.Author))
                    {
                        SearchResultPosts.Add(thisPost);
                    }
                }

                if (thisObjectType == "community")
                    SearchResultCommunities.Add(CommunityApi.GetCommunityById(entityObj.EntityId));

                if (thisObjectType == "event")
                    SearchResultEvents.Add(EventApi.GetEventById(long.Parse(entityObj.EntityId)));
            }

            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");

            var sanitizedQuery = q.Replace("'", "").Replace("\"", "").Replace("--", "").Replace("%", "");

            foreach (var user in dbUsers.Query($"Select * from Users where Name LIKE '{sanitizedQuery}%'"))
                SearchResultUsers.Add(UserApi.GetUserById(user.Uid));

            // Sort by length
            SearchResultUsers = SearchResultUsers.OrderByDescending(user =>
            {
                string username = user.Name;
                return username.Length;
            }).ToList();

            return Page();
        }

        public static string TrimIfTooLong(string text)
        {
            if (text.Length <= 115) return text;
            return text.Substring(0, 115) + "...";
        }

        private List<SearchResultEntry> CalculateWeight(List<dynamic> relList)
        {
            var distinctRows = relList.Select(row => row.EntityId).Distinct().ToList();
            var searchResultList = new List<SearchResultEntry>();

            foreach (var distinctRow in distinctRows)
            {
                var currentWeight = 0.0;

                foreach (var relRow in relList)
                    if (distinctRow == relRow.EntityId)
                        try
                        {
                            long.Parse(distinctRow);
                            currentWeight += relRow.Weight;
                        }
                        catch
                        {
                            currentWeight += relRow.Weight * 3;
                        }

                // Additional weightage for community, add number of posts
                if (SearchApi.GetEntityTypeByEid(distinctRow) == "community")
                    currentWeight += CommunityApi.GetPostCountByCid(distinctRow) / 8;

                searchResultList.Add(new SearchResultEntry
                {
                    EntityId = distinctRow,
                    Weight = currentWeight
                });
            }

            return searchResultList;
        }
    }

    public class SearchResultEntry
    {
        public string EntityId { get; set; }
        public double Weight { get; set; }
    }
}