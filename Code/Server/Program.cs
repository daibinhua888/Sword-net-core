using Sword.Server;
using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server";

            ServerRuntime.Start(888);

            Console.WriteLine("Server started.");
            Console.ReadKey();
        }
    }
}
