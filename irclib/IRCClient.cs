using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace irclib {

    public class IRCClient
    {
        public delegate void MotdEvent(string msg);
        public delegate void NickEvent(string from, string to);
        public delegate void PingEvent(string msg);
        public delegate void PongEvent(string msg);
        public delegate void JoinEvent(string channel);
        public delegate void PrivMsgEvent(string from, string to, string msg);
        public delegate void UnknownEvent(IRCMessage message);
        public delegate bool InterceptEvent(IRCMessage message);

        string hostname;
        int port;
        string nick;
        TcpClient client;
        byte[] readbuf;
        int readsize;
        byte[] sendbuf;
        int sendsize;

        public event MotdEvent OnMotd;
        public event NickEvent OnNick;
        public event PingEvent OnPing;
        public event PongEvent OnPong;
        public event JoinEvent OnJoin;
        public event PrivMsgEvent OnMsg;
        public event InterceptEvent Intercept;
        public event UnknownEvent OnUnknown;

        public IRCClient(string hostname, int port) {
            this.hostname = hostname;
            this.port = port;
        }

        bool init = false;
        public bool Connected { get => init && client.Connected; }

        public bool Connect() {
            if (Connected) return true;
            if (client == null) {
                client = new TcpClient();
                client.Connect(hostname, port);
                readbuf = new byte[client.ReceiveBufferSize];
                sendbuf = new byte[client.SendBufferSize];
            }

            Send(IRCMessage.Nick("PlankBoat"));
            Send(IRCMessage.User("PlankBoat", "pi.tollyx.net", hostname, "A Wierd bot by tollyx"));
            Flush();
            init = true;
            return true;
        }

        public void Disconnect() {
            client?.Close();
            client = null;
            Console.WriteLine($"Disconnected from '{hostname}'");
        }

        public void Listen() {
            Connect();
            while (Connected) {
                var lines = GetMessages();
                if (lines != null) {
                    foreach (var line in lines) {
                        Parse(line);
                    }
                }
                Flush();
                System.Threading.Thread.Sleep(1);
            }
            Disconnect();
        }

        public void Parse(string line) {
            IRCMessage msg = IRCMessage.ParseLine(line);

            if (Intercept?.Invoke(msg) ?? false) {
                return;
            }

            switch (msg.Command) {
                case "PING":
                    Send(IRCMessage.Pong(msg.Trailing));
                    OnPing?.Invoke(msg.Trailing);
                    break;
                case "PONG":
                    OnPong?.Invoke(msg.Trailing);
                    break;
                case "422": // MOTD
                    OnMotd?.Invoke(msg.Trailing);
                    break;
                case "JOIN":
                    OnJoin?.Invoke(msg.Params[0]);
                    break;
                case "NICK":
                    OnNick?.Invoke(msg.Prefix, msg.Params[0]);
                    break;
                case "PRIVMSG":
                    LogIn($"<{msg.Prefix}> {msg.Trailing}");
                    OnMsg?.Invoke(msg.Prefix, msg.Params[0], msg.Trailing);
                    break;
                default:
                    LogIn("??? " + msg);
                    OnUnknown?.Invoke(msg);
                    break;
            }
        }

        public string[] GetMessages() {
            if (!client.Connected) return null;

            var stream = client.GetStream();
            if (!stream.DataAvailable) return null;

            var amount = stream.Read(readbuf, 0, readbuf.Length);
            var msg = Encoding.UTF8.GetString(readbuf.Take(amount).ToArray());
            return msg.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        }

        public void Send(IRCMessage message) {
            var buf = Encoding.UTF8.GetBytes($"{message}\r\n".ToArray());
            if (sendsize + buf.Length < sendbuf.Length) {
                Flush();
            }
            buf.CopyTo(sendbuf, sendsize);
            sendsize += buf.Length;
        }

        public string Flush() {
            if (sendsize <= 0 || !client.Connected) return null;
            var stream = client.GetStream();
            int sent = sendsize;
            stream.Write(sendbuf, 0, sendsize);
            sendsize = 0;
            var msg = Encoding.UTF8.GetString(sendbuf.Take(sent).ToArray());
            LogOut(msg);
            return msg;
        }

        static void LogIn(string input) {
            if (!string.IsNullOrEmpty(input)) {
                foreach (var item in input.Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries)) {
                    var now = DateTime.Now;
                    Console.WriteLine($"[{now.Hour:D2}:{now.Minute:D2}:{now.Second:D2}] <- {item}");
                }
            }
        }

        static void LogOut(string input) {
            if (!string.IsNullOrEmpty(input)) {
                foreach (var item in input.Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries)) {
                    var now = DateTime.Now;
                    Console.WriteLine($"[{now.Hour:D2}:{now.Minute:D2}:{now.Second:D2}] -> {item}");
                }
            }
        }
    }
}
