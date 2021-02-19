using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SnailMailLibs;
using Newtonsoft.Json;
namespace SnailMailClient
{
    class SnailMailClient
    {
        public static string ServerIp;
        public static int ServerPort;
        public static void Main(string[] args)
        {
            if (File.Exists("config.json"))
            {
                ClientConfig clientConfig = JsonConvert.DeserializeObject<ClientConfig>(File.ReadAllText("config.json"));
                ServerIp = clientConfig.defaultIp;
                ServerPort = clientConfig.defaultPort;
            }
            else
            {
                ClientConfig clientConfig = DefaultConfigs.ClientConfig;
                File.Create("config.json").Close();
                File.WriteAllText("config.json", JsonConvert.SerializeObject(clientConfig, Formatting.Indented));
                ServerIp = clientConfig.defaultIp;
                ServerPort = clientConfig.defaultPort;
            }
            if (args.Length > 0 && args[0] != "")
            {
                ServerIp = args[0];
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
