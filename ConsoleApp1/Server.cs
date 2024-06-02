using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    public class Server
    {
        private readonly HttpListener _listener;

        public Server(HttpListener listener)
        {
            _listener = listener;
        }

        public async Task StartAsync()
        {
            var serverObservable = Observable.FromAsync(_listener.GetContextAsync)
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

                                if (await Validator.IsValidCategory(query))
                                {
                                    if (await Validator.IsValidCountry(country))
                                    {
                                        var articles = await NewsApiClient.GetNewsArticlesAsync(query, country);

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
                                    else { await ErrorHandler.ErrorEncounteredAsync(response, "Error: Invalid country.", "text/plain"); }
                                }
                                else { await ErrorHandler.ErrorEncounteredAsync(response, "Error: Invalid category.", "text/plain"); }
                            }
                            else { await ErrorHandler.ErrorEncounteredAsync(response, "Error: Invalid URL. Please provide a valid input.", "text/plain"); }
                        }
                        else { await ErrorHandler.ErrorEncounteredAsync(response, "Error.", "text/plain"); }

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
        }
    }

    public static class ErrorHandler
    {
        public static async Task ErrorEncounteredAsync(HttpListenerResponse response, string rString, string ctString)
        {
            string responseString = rString;
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.ContentType = ctString;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
