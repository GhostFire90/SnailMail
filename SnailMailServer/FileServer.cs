using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using SnailMailLibs;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;


namespace SnailMailServer
{
    public class FileServer
    {
        FileEncoding FileEncoding = new FileEncoding();
        
        public void Start(IPAddress iP, int port = 9000)
        {
            if(iP == null)
            {
                iP = IPAddress.Loopback;
            }
            var endpoint = new IPEndPoint(iP, port);

            var sock = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sock.Bind(endpoint);
            sock.Listen(128);
            _ = Task.Run(() => DoFS(sock));
        }

        private async Task DoFS(Socket sock)
        {
            do
            {
                var clientSocket = await Task.Factory.FromAsync(
                    new Func<AsyncCallback, object, IAsyncResult>(sock.BeginAccept),
                    new Func<IAsyncResult, Socket>(sock.EndAccept), null).ConfigureAwait(false);
                Console.WriteLine("connected");
                
                using(var stream = new NetworkStream(clientSocket, true))
                {
                    byte[] modeBytes = new byte[4];
                    stream.Read(modeBytes, 0, modeBytes.Length);
                    int mode = BitConverter.ToInt32(modeBytes);
                    
                    //Send file to client
                    if(mode == 2)
                    {
                        Console.WriteLine("Send file to client");

                        string path = "ServerFiles/" + clientSocket.RemoteEndPoint.ToString().Split(':')[0] + "/";
                        //Dictionary<string, string> fileList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path + "Manager.json")); ;

                        byte[] packLength = new byte[4];

                        stream.Read(packLength, 0, 4);
                        byte[] keyBytes = new byte[BitConverter.ToInt32(packLength)];
                        stream.Read(keyBytes, 0, keyBytes.Length);

                        string key = Encoding.UTF8.GetString(keyBytes);
                        Console.WriteLine(key);
                        byte[] fileData = File.ReadAllBytes(path + key + ".dat");
                        /*var memstream = new MemoryStream();
                        memstream.Write(fileData);
                        memstream.Position = 0;*/
                        //FileEncoding.FileStructure fs = FileEncoding.FileStructureRetriever(memstream);
                        //Console.WriteLine(fs.fileName);
                        
                        stream.Write(fileData);                        
                    }
                    //Send list of files and times until able to be recieved
                    else if(mode == 0)
                    {
                        string path = "ServerFiles/" + clientSocket.RemoteEndPoint.ToString().Split(':')[0] + "/";
                        //Console.WriteLine(File.Exists(path+"Manager.json"));
                        Dictionary<string, string> fileList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path + "Manager.json")); ;
                        List<string> fileNames = new List<string>();
                        
                        try
                        {
                            
                            fileList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path + "Manager.json"));
                        }
                        catch
                        {
                            stream.Write(BitConverter.GetBytes(1));
                            clientSocket.Close();
                        }
                        stream.Write(BitConverter.GetBytes(0));
                        foreach(KeyValuePair<string, string> entry in fileList)
                        {
                            
                            DateTime endTime = DateTime.Parse(entry.Value).AddDays(7);
                            TimeSpan ts = endTime.Subtract(DateTime.UtcNow);
                            if(ts.CompareTo(TimeSpan.Zero) > 0)
                            {
                                fileNames.Add(entry.Key + " - " + ts.ToString(@"dd\:hh\:mm\:ss"));
                            }
                            else
                            {
                                ts = new TimeSpan(0, 0, 0, 0);
                                fileNames.Add(entry.Key + " - " + ts.ToString(@"dd\:hh\:mm\:ss"));
                            }
                            
                        }
                        /*foreach(string s in fileNames)
                        {
                            Console.WriteLine(s);
                        }*/
                        
                        byte[] dataToSend = ExtendedSerializerExtensions.Serialize(fileNames);
                        //List<string> yeetus = (List<string>)binForm.Deserialize(memStream);

                        //Console.WriteLine(yeetus[0]);
                        List<byte> fullLoad = new List<byte>();
                        fullLoad.AddRange(BitConverter.GetBytes(dataToSend.Length));
                        fullLoad.AddRange(dataToSend);
                        List<string> yeetus = ExtendedSerializerExtensions.Deserialize<List<string>>(dataToSend);
                        Console.WriteLine(yeetus[0]);
                        stream.Write(fullLoad.ToArray());
                    }
                    //Recieve file from client
                    else if(mode == 1)
                    {
                        FileEncoding.FileStructure fs = FileEncoding.FileStructureRetriever(stream);
                        byte[] bytes = FileEncoding.StreamPrepper(fs);
                        Dictionary<string, string> fileList;
                        string path = "ServerFiles/" + fs.ipSender.ToString() + "/";

                        if (!Directory.Exists("ServerFiles/" + fs.ipSender.ToString() + "/"))
                        {
                            
                            Directory.CreateDirectory(path);
                            FileStream stream1 = File.Create(path + "Manager.json");
                            stream1.Write(Encoding.UTF8.GetBytes("{}"));
                            stream1.Close();
                        }
                        try
                        {
                            fileList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path + "Manager.json"));
                        }
                        catch
                        {
                            fileList = new Dictionary<string, string>();
                        }
                        FileStream fstream = File.Create(path + fs.fileName + ".dat");
                        if (!fileList.TryGetValue(fs.fileName, out _))
                        {
                            fileList.Add(fs.fileName, fs.dateSent);
                        }
                        else
                        {
                            fileList[fs.fileName] = fs.dateSent;
                        }

                        File.WriteAllText(path + "Manager.json", JsonConvert.SerializeObject(fileList, Formatting.Indented));
                        fstream.Write(bytes);
                        fstream.Close();
                        Console.WriteLine(fs.ipSender.ToString());
                        clientSocket.Close();
                    }

                    
                }


            } while (true);
        }
        
    }
}
