using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
namespace SnailMailServer
{
    class SnailMailServer
    {
        
        public static void Main(string[] args)
        {
            
            if (!Directory.Exists(@"ServerFiles/"))
            {
                Directory.CreateDirectory(@"ServerFiles/");
            }
            
            FileServer fs = new FileServer();
            fs.Start(IPAddress.Parse("0.0.0.0"));
            Console.ReadLine();
        }
    }
}
