using System;
using System.Collections.Generic;
using System.Text;
using SnailMailClient;
using SnailMailLibs;
using System.IO;
using Terminal.Gui;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace SnailMailClient
{
    public class SnailMailFunctionality
    {
        FileEncoding FileEncoding = new FileEncoding();
        Action<ListViewItemEventArgs> current_mode;
        protected void SendButton(TextView contentLabel, ListView contentListView, TextField ipIn)
        {
            contentListView.OpenSelectedItem -= current_mode;
            ipIn.CanFocus = true;
            ipIn.SetFocus();
            string ip = "127.0.0.1";
            ipIn.TextChanged += (a) => ip = a.ToString().Trim();
            current_mode = (a) => SendFile(a.Value.ToString(), ip);
            contentListView.OpenSelectedItem += current_mode;
            System.Diagnostics.Debug.WriteLine(ipIn.Text.ToString().Trim());
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + @"/Outbox");
            List<string> fileNames = new List<string>();
            List<string> contentList = new List<string>();

            if(files.Length != 0)
            {
                contentLabel.Text = "Select a file to send";
                contentListView.CanFocus = true;
                foreach(string i in files)
                {
                    contentList.Add(Path.GetFileName(i).ToString());
                }

                contentListView.SetSource(contentList);
                //contentListView.SetFocus();
            }
            else
            {
                contentLabel.Text = "No files in Outbox Folder";
            }
        }
        protected void SendFile(string path, string ip="127.0.0.1")
        {
            FileEncoding.FileStructure fs = new FileEncoding.FileStructure()
            {
                fileName = Path.GetFileName(@"Outbox/" + path),
                fileData = FileEncoding.FileToBytes(@"Outbox/" + path),
                dateSent = DateTime.UtcNow.ToString(),
                ipSender = IPAddress.Parse(ip)
            };
            byte[] buffer = FileEncoding.StreamPrepper(fs);
            var endpoint = new IPEndPoint(IPAddress.Parse(SnailMailClient.ServerIp), SnailMailClient.ServerPort);
            var sock = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            sock.Connect(endpoint);
            var ns = new NetworkStream(sock, true);
            ns.Write(BitConverter.GetBytes(1));
            ns.Write(buffer);
            //ns.Close();
        }
        protected void RecieveFileList(TextView contentLabel, ListView contentListView)
        {
            contentListView.OpenSelectedItem -= current_mode;
            current_mode = (a) => RetrieveFile(a.Value.ToString());
            contentListView.OpenSelectedItem += current_mode;
            var endpoint = new IPEndPoint(IPAddress.Parse(SnailMailClient.ServerIp), SnailMailClient.ServerPort);
            var sock = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(endpoint);

            var ns = new NetworkStream(sock, true);
            ns.Write(BitConverter.GetBytes(0));
            //Checks for code sent by server to see if it has files it can recieve
            byte[] youHaveMail = new byte[4];

            ns.Read(youHaveMail, 0, 4);
            if(BitConverter.ToInt32(youHaveMail) == 1)
            {
                contentLabel.Text = "You have no waiting Messages";
                ns.Close();
                

            }
            else if(BitConverter.ToInt32(youHaveMail) == 0)
            {
                byte[] lengthOfData = new byte[4];
                ns.Read(lengthOfData, 0, 4);
                byte[] dataBytes = new byte[BitConverter.ToInt32(lengthOfData)];
                ns.Read(dataBytes, 0, dataBytes.Length);
                contentLabel.Text = "Select a file to download if it is ready";
                
                List<string> fileList = ExtendedSerializerExtensions.Deserialize<List<string>>(dataBytes);
                //contentLabel.Text = fileList[0];
                contentListView.SetSource(fileList);
                contentListView.CanFocus = true;
                contentListView.SetFocus();
                //sock.Close();
            }
        }
        protected void RetrieveFile(string fileOption)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(SnailMailClient.ServerIp), SnailMailClient.ServerPort);
            var sock = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(endpoint);

            var ns = new NetworkStream(sock, true);

            ns.Write(BitConverter.GetBytes(2));

            TimeSpan timeSpan = TimeSpan.Parse(fileOption.Split(" - ")[1]);
            System.Diagnostics.Debug.WriteLine(timeSpan.Minutes);
            if(timeSpan.TotalSeconds <= 0)
            {

                byte[] key = Encoding.UTF8.GetBytes(fileOption.Split(" - ")[0]);
                List<byte> fullLoad = new List<byte>();

                fullLoad.AddRange(BitConverter.GetBytes(key.Length));
                fullLoad.AddRange(key);

                ns.Write(fullLoad.ToArray());

                FileEncoding.FileStructure fs = FileEncoding.FileStructureRetriever(ns);

                FileStream fstream = File.Create("Inbox/" + fs.fileName);
                fstream.Write(fs.fileData);
                fstream.Close();
            }
            else
            {
                
            }
        }
        protected void ClearListView(ListView contentListView, FrameView contentView, Button send, TextField ipIn)
        {
            contentListView.SetSource(new List<string>());
            send.SetFocus();
            contentListView.CanFocus = false;
            contentView.CanFocus = false;
            ipIn.Text = "";
            ipIn.CanFocus = false;
            
        }
    }
}
