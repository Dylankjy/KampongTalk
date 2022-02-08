using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using KampongTalk.Models;
using Mighty;

namespace KampongTalk.Pages.Search
{
    public static class SearchApi
    {
        public static async Task<dynamic> GetKeyword(string text)
        {
            var encodedText = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            var res = await "http://localhost:5010".SetQueryParams(new {text = encodedText}).GetJsonAsync();
            
            return res;
        }

        public static string GetEntityTypeByEid(long eid)
        {
            return GetEntityTypeByEid(eid.ToString());
        }

        public static string GetEntityTypeByEid(string eid)
        {
            var dbPost =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Post");
            var dbEvents =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Events");
            var dbUsers =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Users");
            var dbCommunities =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Communities");

            string eidDataType = null;
            
            try
            {
                long tryConvert = long.Parse(eid);
                eidDataType = "peu";
            }
            catch
            {
                eidDataType = "c";
            }

            if (eidDataType == "peu")
            {
                var post = dbPost.Single(new {Pid = eid});
                var events = dbEvents.Single(new {Eid = eid});
                var user = dbUsers.Single(new {Uid = eid});

                if (post != null)
                {
                    return "post";
                }
                if (events != null)
                {
                    return "event";
                }
                if (user != null)
                {
                    return "user";
                }
            }
            else
            {
                var community = dbCommunities.Single(new {Cid = eid});
                if (community != null)
                {
                    return "community";
                }
            }

            return null;
        }

        public static void PutKeyword(string keyword, int weight, long eid)
        {
            PutKeyword(keyword, weight, eid.ToString());
        }

        public static void PutKeyword(string keyword, int weight, string eid)
        {
            // Database declarations
            var dbRel =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Relevancy");
            
            // Clear all existing relevancy data before populating with new
#pragma warning disable CS4014
            dbRel.Delete($"EntityId = '{eid}' AND type = 'manual'");
#pragma warning restore CS4014

            dbRel.InsertAsync(new Relevancy
            {
                EntityId = eid,
                Keyword = keyword.ToLower(),
                Weight = weight,
                Type = "manual",
                Timestamp = DateTime.Now,
            });
        }

        public static void PutRelevancy(string text, long entityId)
        {
            PutRelevancy(text, entityId.ToString());
        }
        
        public static async void PutRelevancy(string text, string entityId)
        {
            // Database declarations
            var dbRel =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Relevancy");
            
            // Clear all existing relevancy data before populating with new
#pragma warning disable CS4014
            await dbRel.DeleteAsync($"EntityId = '{entityId}' AND type != 'manual'");
#pragma warning restore CS4014

            var res = await GetKeyword(text);
            
            if (res.status == 200)
            {
                // Put all dynamic into List<T> so that I can use distinct()
                List<string> listOfNouns = new List<string>();
                List<string> listOfVerbs = new List<string>();
                foreach (var noun in res.nouns)
                {
                    listOfNouns.Add(noun);
                }
                foreach (var verb in res.verbs)
                {
                    listOfVerbs.Add(verb);
                }


                foreach (string noun in listOfNouns.Distinct().ToList())
                {
                    var currentNoun = new Relevancy
                    {
                        EntityId = entityId,
                        Keyword = noun,
                        Timestamp = DateTime.Now,
                        Type = "noun",
                        Weight = listOfNouns.Count(s => s!=null && s.Equals(noun))
                    };

#pragma warning disable CS4014
                    dbRel.InsertAsync(currentNoun);
#pragma warning restore CS4014
                }

                foreach (var verb in listOfVerbs.Distinct().ToList())
                {
                    // Count number of words to set weight
                    var numberOfOccurrences = Regex.Matches(text, verb).Count;

                    var currentVerb = new Relevancy
                    {
                        EntityId = entityId,
                        Keyword = verb,
                        Timestamp = DateTime.Now,
                        Type = "verb",
                        Weight = listOfVerbs.Count(s => s!=null && s.Equals(verb))
                    };

#pragma warning disable CS4014
                    dbRel.InsertAsync(currentVerb);
#pragma warning restore CS4014
                }
            }
            else
            {
                Console.WriteLine(new Exception("KeywordAPI service responded with an error."));
            }
        }

        public static async Task<dynamic> GetSearchResults(string searchQuery)
        {
            // Database declarations
            var dbRel =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Relevancy");

            // Get keywords from API
            var keywordResultsRaw = await GetKeyword(searchQuery);
            List<string> keywordResults = new List<string>();
            foreach (var noun in keywordResultsRaw.nouns)
            {
                keywordResults.Add(noun);
            }

            foreach (var verb in keywordResultsRaw.verbs)
            {
                keywordResults.Add(verb);
            }

            // Contain search results
            var search = new List<dynamic>();
            
            // Database all
            // var dbRelAll = dbRel.All();

            foreach (var word in keywordResults)
            {
                foreach (var row in dbRel.All())
                {
                    if (word.Contains(row.Keyword))
                    {
                        search.Add(row);
                    }
                }
            }

            return search;
        }
    }
}