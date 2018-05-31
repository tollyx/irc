using irclib;
using System;
using System.Linq;

namespace irc
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Connecting to localhost port 6667...");
            IRCClient client = new IRCClient("localhost", 6667);
            client.OnMotd += msg => {
                client.Send(IRCMessage.Join("#home"));
            };
            client.OnJoin += (who, channel) => {

            };
            client.Listen();
        }
    }
}
