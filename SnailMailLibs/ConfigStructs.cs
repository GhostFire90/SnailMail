using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnailMailLibs
{
    public struct ClientConfig
    {
        public string defaultIp;
        public int defaultPort;

    }
    public struct ServerConfig
    {
        public int port;
        public int days_delayed;
    }
    public static class DefaultConfigs
    {
        public static ClientConfig ClientConfig = new ClientConfig()
        {
            defaultIp = "127.0.0.1",
            defaultPort = 9000
        };
        public static ServerConfig ServerConfig = new ServerConfig()
        {
            port = 900,
            days_delayed = 7
        };
    }
}
