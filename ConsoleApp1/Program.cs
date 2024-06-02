using ConsoleApp1;
using System;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5050/");
        listener.Start();
        Console.WriteLine("Listening...");

        var server = new Server(listener);
        await server.StartAsync();

        Console.ReadLine();
        listener.Stop();
    }
}
