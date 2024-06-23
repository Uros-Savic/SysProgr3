using ConsoleApp1;
using System;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5050/");
        listener.Start();
        Console.WriteLine("Listening...");

        var server = new Server(listener);
        server.Start();

        Console.ReadLine();
        listener.Stop();
    }
}
