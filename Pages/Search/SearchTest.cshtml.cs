using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KampongTalk.Pages.Search
{
    public class SearchTest : PageModel
    {
        
        [BindProperty] public string SearchQuery { get; set; }
        public string SearchResult { get; set; }
        
        public void OnGet()
        {
            
        }

        public IActionResult OnPost()
        {
            // var result = SearchApi.GetKeyword(SearchQuery).Result;
            
            SearchApi.PutRelevancy(SearchQuery, 0);

            SearchResult = "OK";
            
            // SearchResult = $"Nouns: {String.Join(", ", result.nouns)} | Verbs: {String.Join(", ", result.verbs)}";

            return Page();
        }
    }
}