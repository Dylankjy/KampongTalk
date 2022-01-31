﻿using System;
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
        
        public static async void PutRelevancy(string text, long entityId)
        {
            // Database declarations
            var dbRel =
                new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"],
                    "Relevancy");

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
    }
}