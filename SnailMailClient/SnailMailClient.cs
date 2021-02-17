using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SnailMailClient
{
    class SnailMailClient : SnailMailFunctionality
    {
        public static string ServerIp = "127.0.0.1";
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] != "")
            {
                ServerIp = args[0];
            }
            else
            {
                ServerIp = "127.0.0.1";
            }
            if (!Directory.Exists(@"Outbox/"))
            {
                Directory.CreateDirectory(@"Outbox/");
            }
            if (!Directory.Exists(@"Inbox/"))
            {
                Directory.CreateDirectory(@"Inbox/");
            }
            SnailMailGui snailMailGui = new SnailMailGui();
            snailMailGui.Start();
        }
    }
}
