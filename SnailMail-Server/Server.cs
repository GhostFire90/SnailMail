using System;
using SnailMailProtocol;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace SnailMail_Server
{
    class Server
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> config;
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            if (File.Exists("config.json"))
            {
                StringBuilder builder = new StringBuilder();
                using (StreamReader reader = File.OpenText("config.json"))
                {
                    while (!reader.EndOfStream) builder.AppendLine(reader.ReadLine());
                    config = JsonConvert.DeserializeObject<Dictionary<string, string>>(builder.ToString(), settings);

                }

            }
            else
            {
                config = new Dictionary<string, string>();
                config.Add("ip", "127.0.0.1");
                config.Add("port", "90");
                config.Add("delay-days", "7");

                string json = JsonConvert.SerializeObject(config, settings);
                using (StreamWriter writer = File.CreateText("config.json"))
                {
                    writer.Write(json);
                }

            }
            SnailMailServer server = new SnailMailServer(config["ip"], int.Parse(config["port"]), int.Parse(config["delay-days"]));
            server.Start();
        }
    }
}
