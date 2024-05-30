using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Listening...");

        var serverObservable = Observable.FromAsync(listener.GetContextAsync)
            .Repeat()
            .SelectMany(async context =>
            {
                try
                {
                    Console.WriteLine("Received request for " + context.Request.Url);
                    HttpListenerResponse response = context.Response;
                    var urlSegments = context.Request.Url.Segments;

                    if (urlSegments.Length == 2 && urlSegments[1] != "/" && context.Request.Url.AbsolutePath != "/favicon.ico")
                    {
                        var parts = urlSegments[1].Split('-');
                        if (parts.Length == 2)
                        {
                            var query = parts[0];
                            var country = parts[1];

                            if (IsValidCategory(query))
                            {
                                if (IsValidCountry(country))
                                {

                                    var articles = await GetNewsArticlesAsync(query, country);

                                    var json = new JObject
                                    {
                                        ["totalArticles"] = articles.Count,
                                        ["articles"] = new JArray(articles.Select(a => new JObject
                                        {
                                            ["title"] = a.Title,
                                            ["description"] = a.Description,
                                            ["uniqueWordCount"] = a.UniqueWordCount,
                                            ["lowercaseWordCount"] = a.LowercaseWordCount
                                        }))
                                    };

                                    byte[] buffer = Encoding.UTF8.GetBytes(json.ToString());
                                    response.ContentLength64 = buffer.Length;
                                    response.ContentType = "application/json";
                                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                                }
								else { await ErrorIncounteredAsync(response, "Error: Invalid country.", "text/plain");}
                            }
                            else {await ErrorIncounteredAsync(response, "Error: Invalid category.", "text/plain");}
                        }
                        else { await ErrorIncounteredAsync(response, "Error: Invalid URL. Please provide a valid input.", "text/plain"); }
                    }
                    else { await ErrorIncounteredAsync(response, "Error.", "text/plain"); }

                    response.OutputStream.Close();
                    return context;
                }
                
                catch (Exception ex)
                {
                    Console.WriteLine("Error occurred: " + ex.ToString());
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                    throw;
                }
            });
    
        serverObservable.Subscribe(
            context => Console.WriteLine("Request handled."),
            ex => Console.WriteLine("Error occurred: " + ex.ToString())
        );

        Console.ReadLine();
        listener.Stop();
    }

    public static async Task ErrorIncounteredAsync(HttpListenerResponse response, string rString, string ctString)
	{
        string responseString = rString;
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.ContentType = ctString;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        return;
	}


    static async Task<List<Article>> GetNewsArticlesAsync(string keyword, string country)
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
    }

    static bool IsValidCountry(string country)
    {
        List<string> validCountries = new List<string> {
        "ae", "ar", "at", "au", "be", "bg", "br", "ca", "ch", "cn", "co", "cu",
        "cz", "de", "eg", "fr", "gb", "gr", "hk", "hu", "id", "ie", "il", "in",
        "it", "jp", "kr", "lt", "lv", "ma", "mx", "my", "ng", "nl", "no", "nz",
        "ph", "pl", "pt", "ro", "rs", "ru", "sa", "se", "sg", "si", "sk", "th",
        "tr", "tw", "ua", "us", "ve", "za"
    };

        return validCountries.Contains(country.ToLower());
    }

    static bool IsValidCategory(string category)
    {
        List<string> validCategories = new List<string> {
        "business", "entertainment","business", "entertainment", "general", "health", "science", "sports", "technology"
    };

        return validCategories.Contains(category.ToLower());
    }

    class Article
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int UniqueWordCount { get; set; }
        public int LowercaseWordCount { get; set; }
    }
}


