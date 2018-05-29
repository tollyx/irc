using System;

namespace irclib {
    public class IRCMessage {
        public string Prefix = null;
        public string Command = null;
        public string[] Params = null;
        public string Trailing = null;

        public static IRCMessage ParseLine(string line) {

            IRCMessage msg = new IRCMessage();

            if (line.StartsWith(":")) {
                int p = line.IndexOf(' ');
                msg.Prefix = line.Substring(1, p-1);
                line = line.Substring(p + 1);
            }

            int i = line.IndexOf(' ');
            if (i > 0) {
                msg.Command = line.Substring(0, i);
                line = line.Substring(i + 1);

                int t = line.IndexOf(':');
                if (t >= 0) {
                    msg.Params = line.Substring(0, t).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    msg.Trailing = line.Substring(t + 1);
                }
                else {
                    msg.Params = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                }
            }
            else {
                msg.Command = line;
            }
            return msg;
        }

        public IRCMessage() {}

        public IRCMessage(string prefix, string command, string[] @params, string trailing) {
            Prefix = prefix;
            Command = command;
            Params = @params;
            Trailing = trailing;
        }

        public IRCMessage(string prefix, string command, string trailing) {
            Prefix = prefix;
            Command = command;
            Trailing = trailing;
        }

        public IRCMessage(string command, string[] @params, string trailing) {
            Command = command;
            Params = @params;
            Trailing = trailing;
        }

        public IRCMessage(string command, string[] @params) {
            Command = command;
            Params = @params;
        }

        public IRCMessage(string command, string trailing) {
            Command = command;
            Trailing = trailing;
        }

        public override string ToString() {
            string msg = Command;
            if (Prefix != null) {
                msg = $":{Prefix} {msg}";
            }
            if (Params != null) {
                msg = $"{msg} {string.Join(' ', Params)}";
            }
            if (Trailing != null) {
                msg = $"{msg} :{Trailing}";
            }
            
            return msg + "\r\n";
        }

        public static IRCMessage User(string fromnick, string username, string hostname, string servername, string realname) {
            return new IRCMessage(fromnick, "USER", new[] { username, hostname, servername }, realname);
        }

        public static IRCMessage User(string username, string hostname, string servername, string realname) {
            return new IRCMessage("USER", new[] { username, hostname, servername }, realname);
        }

        public static IRCMessage Ping(string msg) {
            return new IRCMessage("PING", msg);
        }

        public static IRCMessage Pong(string response) {
            return new IRCMessage("PONG", new[] { response });
        }

        public static IRCMessage Who(string what) {
            return new IRCMessage("WHO", new[] { what });
        }

        public static IRCMessage SendMessage(string to, string msg) {
            return new IRCMessage("PRIVMSG", new[] { to }, msg);
        }

        public static IRCMessage Nick(string nick) {
            return new IRCMessage("NICK", new[] { nick });
        }

        public static IRCMessage Join(string channels) {
            return new IRCMessage("JOIN", new[] { channels });
        }

        public static IRCMessage Join(string channels, string keys) {
            return new IRCMessage("JOIN", new[] { channels, keys });
        }

        public static IRCMessage Part(string channels) {
            return new IRCMessage("PART", new[] { channels });
        }
    }
}
