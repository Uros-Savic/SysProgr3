using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    public static class NewsApiClient
    {
        public static IObservable<List<Article>> GetNewsArticlesAsync(string keyword, string country)
        {
            return Observable.FromAsync(async () =>
            {
                string apiKey = "584ac99cfd6d4cdfab4693b27b26a9e0";
                string requestUri = $"https://newsapi.org/v2/top-headlines?category={keyword}&country={country}&pageSize=100&apiKey={apiKey}";

                Console.WriteLine("Request URI: " + requestUri);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("ConsoleApp/1.0");

                    HttpResponseMessage response = await client.GetAsync(requestUri);
                    Console.WriteLine("Response status code: " + response.StatusCode);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Error body: " + errorBody);
                    }

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(responseBody);
                    var articles = json["articles"]
                        .Select(a => new Article
                        {
                            Title = (string)a["title"],
                            Description = (string)a["description"]
                        }).ToList();

                    foreach (var article in articles)
                    {
                        var words = article.Description?.Split(new[] { ' ', '.', ',', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                        article.UniqueWordCount = words.Distinct(StringComparer.OrdinalIgnoreCase).Count();
                        article.LowercaseWordCount = words.Count(word => char.IsLower(word.FirstOrDefault()));
                    }

                    return articles.OrderByDescending(a => a.UniqueWordCount).ToList();
                }
            });
        }
    }

    public class Article
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int UniqueWordCount { get; set; }
        public int LowercaseWordCount { get; set; }
    }
}
