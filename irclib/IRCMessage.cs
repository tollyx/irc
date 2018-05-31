using System;
using System.Collections.Generic;
using System.Linq;

namespace irclib {
    public class IRCMessage {
        public string Prefix = null;
        public string Command = null;
        public string[] Args = null;

        public static IRCMessage ParseLine(string line) {
            IRCMessage msg = new IRCMessage();
            string[] trailsplit = line.Split(" :", 2);
            IEnumerable<string> args = trailsplit[0].Split(' ');
            if (trailsplit.Length == 2) {
                args = args.Append(trailsplit[1]);
            }
            if (args.First().StartsWith(':')) {
                msg.Prefix = args.First().Substring(1);
                args = args.Skip(1);
            }
            msg.Command = args.First();
            args = args.Skip(1);
            msg.Args = args.ToArray();
            return msg;
        }

        public IRCMessage() {}

        public IRCMessage(string prefix, string command, params string[] args) {
            Prefix = prefix;
            Command = command;
            Args = args;
        }

        public override string ToString() {
            string msg = Command;
            if (Prefix != null) {
                msg = $":{Prefix} {msg}";
            }
            if (Args != null && Args.Length > 0) {
                var temp = (string[])Args.Clone();
                if (Args[Args.Length - 1].Any(c => c == ' ' || c == ':')) {
                    temp[Args.Length - 1] = ":" + temp[Args.Length - 1];
                }
                msg += " " + string.Join(' ', temp);
            }
            
            return msg + "\r\n";
        }

        public static IRCMessage User(string fromnick, string username, string hostname, string servername, string realname) {
            return new IRCMessage(fromnick, "USER", username, hostname, servername, realname);
        }

        public static IRCMessage User(string username, string hostname, string servername, string realname) {
            return new IRCMessage(null, "USER", username, hostname, servername, realname);
        }

        public static IRCMessage Ping(string msg) {
            return new IRCMessage(null, "PING", msg);
        }

        public static IRCMessage Pong(string response) {
            return new IRCMessage(null, "PONG", response);
        }

        public static IRCMessage Who(string what) {
            return new IRCMessage(null, "WHO", what);
        }

        public static IRCMessage SendMessage(string to, string msg) {
            return new IRCMessage(null, "PRIVMSG", to, msg);
        }

        public static IRCMessage Nick(string nick) {
            return new IRCMessage(null, "NICK", nick);
        }

        public static IRCMessage Join(string channels) {
            return new IRCMessage(null, "JOIN", channels);
        }

        public static IRCMessage Join(string channels, string keys) {
            return new IRCMessage("JOIN", channels, keys);
        }

        public static IRCMessage Part(string channels) {
            return new IRCMessage("PART", channels);
        }
    }
}
