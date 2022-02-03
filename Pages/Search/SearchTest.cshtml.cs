using System;
using KampongTalk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KampongTalk.Pages.Search
{
    public class SearchTest : PageModel
    {
        
        [BindProperty] public string SearchQuery { get; set; }
        public string SearchResult { get; set; } = String.Empty;
        
        public void OnGet()
        {
            
        }

        public IActionResult OnPost()
        {
            // var result = SearchApi.GetKeyword(SearchQuery).Result;
            
            // SearchApi.PutRelevancy(SearchQuery, 0);

            var result = SearchApi.GetSearchResults(SearchQuery).Result;

            foreach (var i in result)
            {
                try
                {
                    SearchResult += $"{i.EntityId} | {i.Keyword} | {PostApi.GetPostByPid(long.Parse(i.EntityId)).Content}<br>";
                }
                catch (Exception e)
                {
                    SearchResult += $"{i.EntityId} | {i.Keyword}<br>";
                }
                
                // try
                // {
                //     Console.WriteLine("Post");
                //     long pid = long.Parse(i.EntityId);
                    // SearchQuery += $"| {PostApi.GetPostByPid(long.Parse(i.EntityId)).Content}<br>";
                // }
                // catch
                // {
                //     Console.WriteLine("Others");
                //     SearchQuery += "<br>";
                // }
            }

            // SearchResult = "OK";
            
            // SearchResult = $"Nouns: {String.Join(", ", result.nouns)} | Verbs: {String.Join(", ", result.verbs)}";

            return Page();
        }
    }
}