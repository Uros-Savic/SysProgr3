using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
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
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
        }

        public void Start()
        {
            var serverObservable = Observable.FromAsync(_listener.GetContextAsync)
                .Repeat()
                .SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOn(TaskPoolScheduler.Default)
                .SelectMany(async context =>
                {
                    try
                    {
                        LogRequest(context.Request.Url);

                        HttpListenerResponse response = context.Response;
                        var urlSegments = context.Request.Url.Segments;

                        if (urlSegments.Length == 2 && urlSegments[1] != "/" && context.Request.Url.AbsolutePath != "/favicon.ico")
                        {
                            var parts = urlSegments[1].Split('-');
                            if (parts.Length == 2)
                            {
                                var query = parts[0];
                                var country = parts[1];


                                if (Validator.IsValidCategory(query))
                                {
                                    if (Validator.IsValidCountry(country))
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

                                        LogResponseSuccess(context.Request.Url, articles.Count);
                                    }
                                    else
                                    {
                                        ErrorEncountered(response, "Error: Invalid country.", "text/plain");
                                        LogResponseError(context.Request.Url, "Invalid country.");
                                    }
                                }
                                else
                                {
                                    ErrorEncountered(response, "Error: Invalid category.", "text/plain");
                                    LogResponseError(context.Request.Url, "Invalid category.");
                                }
                            }
                            else
                            {
                                ErrorEncountered(response, "Error: Invalid URL. Please provide a valid input.", "text/plain");
                                LogResponseError(context.Request.Url, "Invalid URL.");
                            }
                        }
                        else
                        {
                            ErrorEncountered(response, "Error.", "text/plain");
                            LogResponseError(context.Request.Url, "Discarded request.");
                        }

                        response.OutputStream.Close();
                        return context;
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
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

        private void LogRequest(Uri url)
        {
            Console.WriteLine($"Received request for {url}");
        }

        private void LogResponseSuccess(Uri url, int articleCount)
        {
            Console.WriteLine($"Successfully processed request for {url}. Retrieved {articleCount} articles.");
        }

        private void LogResponseError(Uri url, string errorMessage)
        {
            Console.WriteLine($"Error processing request for {url}: {errorMessage}");
        }

        private void LogException(Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex}");
        }

        private void ErrorEncountered(HttpListenerResponse response, string rString, string ctString)
        {
            string responseString = rString;
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.ContentType = ctString;
            response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
