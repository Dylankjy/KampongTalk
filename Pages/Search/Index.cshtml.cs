using System.Collections.Generic;
using System.Linq;
using KampongTalk.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KampongTalk.Pages.Search
{
    public class Index : PageModel
    {
        // Search result prop
        public List<dynamic> SearchResultPosts { get; set; } = new List<dynamic>();
        public List<dynamic> SearchResultCommunities { get; set; } = new List<dynamic>();
        public string SearchField { get; set; }

        public void OnGet(string q)
        {
            var result = SearchApi.GetSearchResults(q).Result;
            SearchField = q;

            List<SearchResultEntry> calculatedWeightResults = CalculateWeight(result);
            calculatedWeightResults = calculatedWeightResults.OrderBy(i => i.Weight).Reverse().ToList();

            foreach (var entity in calculatedWeightResults)
                try
                {
                    SearchResultPosts.Add(PostApi.GetPostByPid(long.Parse(entity.EntityId)));
                }
                catch
                {
                    SearchResultCommunities.Add(CommunityApi.GetCommunityById(entity.EntityId));
                }
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