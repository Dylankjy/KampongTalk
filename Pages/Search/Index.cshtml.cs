using System.Collections.Generic;
using System.Linq;
using KampongTalk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mighty;

namespace KampongTalk.Pages.Search
{
    public class Index : PageModel
    {
        // Search result prop
        public List<dynamic> SearchResultPosts { get; set; } = new List<dynamic>();
        public List<dynamic> SearchResultCommunities { get; set; } = new List<dynamic>();
        public List<dynamic> SearchResultEvents { get; set; } = new List<dynamic>();
        public List<dynamic> SearchResultUsers { get; set; } = new List<dynamic>();
        public string SearchField { get; set; }

        public IActionResult OnGet(string q)
        {
            SearchField = q;

            // Detect whether kampongid 
            if (q.StartsWith("@")) return Redirect($"/Profile?u={q.Normalize().Replace("@", "")}");

            var result = SearchApi.GetSearchResults(q).Result;

            List<SearchResultEntry> calculatedWeightResults = CalculateWeight(result);
            calculatedWeightResults = calculatedWeightResults.OrderBy(i => i.Weight).Reverse().ToList();

            foreach (var entityObj in calculatedWeightResults)
            {
                var thisObjectType = SearchApi.GetEntityTypeByEid(entityObj.EntityId);

                if (thisObjectType == "post")
                    SearchResultPosts.Add(PostApi.GetPostByPid(long.Parse(entityObj.EntityId)));

                if (thisObjectType == "community")
                    SearchResultCommunities.Add(CommunityApi.GetCommunityById(entityObj.EntityId));

                if (thisObjectType == "event")
                    SearchResultEvents.Add(EventApi.GetEventById(long.Parse(entityObj.EntityId)));
            }

            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");

            foreach (var user in dbUsers.Query($"Select * from Users where Name LIKE '{q.Normalize()}%'"))
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