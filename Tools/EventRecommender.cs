using Flurl;
using Flurl.Http;
using Mighty;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KampongTalk.Tools
{
    public static class ICollectionExtensions
    {
        public static IEnumerable<TSource> SortLike<TSource, TKey>(this ICollection<TSource> source, IEnumerable<TKey> sortOrder)
        {
            var cloned = sortOrder.ToArray();
            var sourceArr = source.ToArray();
            Array.Sort(cloned, sourceArr);
            return sourceArr.Reverse();
        }
    }

    public interface IEventRecommender
    {
        Task<dynamic> GetEventRecommendation(string source_text, List<String> target_texts);
        Task<IEnumerable<long>> Recommend(string source_text);
    } 

    public class EventRecommender: IEventRecommender
    {
        public static DateTime nowDt = DateTime.Now;
        public static string nowDtString = nowDt.ToString("yyyy-MM-dd HH:mm:ss");

        public static MightyOrm eventDb { get; set; } =
            new MightyOrm(ConfigurationManager.AppSetting["ConnectionStrings:KampongTalkDbConnection"], "Events");

        public IEnumerable<dynamic> allEvents { get; set; } = eventDb.All($"Date > '{nowDtString}'");

        public async Task<dynamic> GetEventRecommendation(string source_text, List<String> target_texts)
        {
            var jsonData = new
            {
                inputs = new
                {
                    source_sentence = source_text,
                    sentences = target_texts
                }
            };

            var resp =  "https://api-inference.huggingface.co/models/sentence-transformers/multi-qa-MiniLM-L6-cos-v1"
                .WithHeader("Authorization", ConfigurationManager.AppSetting["HuggingFace:API_KEY"])
                .PostJsonAsync(jsonData).ReceiveString().Result;

            return resp;
        }


        public async Task<IEnumerable<long>> Recommend(string source_text)
        {
            var target_texts = new List<String>();
            var target_text_ids = new List<long>();

            foreach (var myEvent in allEvents)
            {
                target_texts.Add($"{myEvent.Name} {myEvent.Description}");
                target_text_ids.Add(myEvent.Eid);
            }

            // Get the list of scores
            Task<dynamic> respTask = GetEventRecommendation(source_text, target_texts);
            var result = await respTask;
            var resultStripped = result.Substring(1, result.Length - 2);
            List<String> scoreStrList = new List<String>(resultStripped.Split(','));
            // Contains the scores (In float format)
            List<float> scoreList = new List<float>();
            foreach (var elem in scoreStrList)
            {
                scoreList.Add(float.Parse(elem));
            }

            var sorted_ids = target_text_ids.SortLike(scoreList);
            return sorted_ids;
        }
    }
}
