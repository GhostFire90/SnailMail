using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using SnailMailLibs;
using Newtonsoft.Json;
namespace SnailMailServer
{
    class SnailMailServer
    {
        static int port;
        public static int days_delayed;
        public static void Main(string[] args)
        {
            if (File.Exists("config.json"))
            {
                ServerConfig serverConfig = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText("config.json"));
                port = serverConfig.port;
                days_delayed = serverConfig.days_delayed;
            }
            else
            {
                ServerConfig serverConfig = DefaultConfigs.ServerConfig;
                File.Create("config.json").Close();
                File.WriteAllText("config.json", JsonConvert.SerializeObject(serverConfig, Formatting.Indented));
                port = serverConfig.port;
                days_delayed = serverConfig.days_delayed;
            }
            if (!Directory.Exists(@"ServerFiles/"))
            {
                Directory.CreateDirectory(@"ServerFiles/");
            }
            
            FileServer fs = new FileServer();
            fs.Start(IPAddress.Parse("0.0.0.0"), port);
            Console.ReadLine();
        }
    }
}
