using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using BetterFileProtocol;
using System.Threading;

namespace SnailMailProtocol
{
    public class SnailMailServer
    {
        TcpListener listener;
        IPEndPoint endPoint;
        int days;

        ManualResetEvent connectionDone = new ManualResetEvent(false);

        public SnailMailServer(string ip = "0.0.0.0", int port = 90, int delay = 7)
        {
            IPAddress address;
            if(!IPAddress.TryParse(ip, out address))
            {
                throw new FormatException("Ip not valid");
            }
            endPoint = new IPEndPoint(address, port);
            days = delay;
        }

        public void Start()
        {
            listener = new TcpListener(endPoint);
            listener.Start();

            AServerLoop();
        }

        /// <summary>
        /// Handles Server & Client key synch
        /// </summary>
        /// <param name="client">The client, used to get IP Address for folder creation</param>
        /// <param name="stream">stream to act on</param>
        public void Handshake(TcpClient client, NetworkStream stream)
        {
            string dir = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(dir + "/public.key"))
            {
                SMHelpers.SendHCode(stream, HandshakeCodes.ServerHasKey); //0 means they have the public key

                HandshakeCodes code = SMHelpers.ReceiveHCode(stream);
                
                if (code == HandshakeCodes.KeyCreated)
                {
                    BFTP.Recieve(stream, 256, dir + '/');
                }
            }
            else
            {
                SMHelpers.SendHCode(stream, HandshakeCodes.ServerDoesntHaveKey); //1 means they do not

                BFTP.Recieve(stream, 256, dir + '/');

            }
        }



        /// <summary>
        /// Recieves files sent from clients
        /// </summary>
        /// <param name="stream">Network stream to act on</param>
        public void Recieve(NetworkStream stream)
        {
            byte[] inboxBufferLength = new byte[4];
            stream.Read(inboxBufferLength, 0, 4);
            byte[] inboxBuffer = new byte[BitConverter.ToInt32(inboxBufferLength)];

            stream.Read(inboxBuffer, 0, inboxBuffer.Length);

            string inbox = Encoding.UTF8.GetString(inboxBuffer);

            if (File.Exists($"{inbox}/public.key"))
            {
                SMHelpers.SendOCode(stream, OperationCodes.YesKey);
                //stream.Write(BitConverter.GetBytes((int)OperationCodes.YesKey));
                BFTP.Send(stream, $"{inbox}/public.key");
                BFTP.Recieve(stream, 256, $"{inbox}/");
            }
            else
            {
                SMHelpers.SendOCode(stream, OperationCodes.NoKey);
                OperationCodes code = SMHelpers.ReceiveOCode(stream);
                if (code == OperationCodes.AbortSend)
                {
                    return;
                }
                else if (code == OperationCodes.ContinueSend)
                {
                    string exitDir = BFTP.Recieve(stream, 256, $"{inbox}/");
                }
                else
                {
                    throw new Exception("Unknown code for this operation");
                }
            }
            
            

        }

        /// <summary>
        /// Sends the encrypted file that was recived from the client previously
        /// </summary>
        /// <param name="client">Client that is requesting the file (used for the ip)</param>
        /// <param name="stream">Stream to send on</param>
        public void Send(TcpClient client, NetworkStream stream)
        {
            string dir = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + '/';



            byte[] nameLenght = new byte[4];
            stream.Read(nameLenght, 0, 4);

            byte[] fileNameBytes = new byte[BitConverter.ToInt32(nameLenght)];
            stream.Read(fileNameBytes);

            string fileName = Encoding.UTF8.GetString(fileNameBytes);
            FileInfo fileInfo;
            if (File.Exists($"{dir}{fileName}"))
            {
                stream.Write(BitConverter.GetBytes((int)OperationCodes.NoKey));
                fileInfo = new FileInfo($"{dir}{fileName}");

            }
            else
            {
                stream.Write(BitConverter.GetBytes((int)OperationCodes.NoKey));
                fileInfo = new FileInfo($"{dir}{fileName}.crp");
            }

             
            fileInfo.Refresh();
            DateTime creation = fileInfo.LastWriteTime;

            TimeSpan howLong = DateTime.Now.Subtract(creation);
            if (howLong.TotalDays >= days)
            {
                SMHelpers.SendOCode(stream, OperationCodes.WaitTimeOver);            }
            else
            {
                SMHelpers.SendOCode(stream, OperationCodes.WaitTime);
                string ready = creation.AddDays(7).ToString("MM/dd/yyyy");

                byte[] timeBuffer = Encoding.UTF8.GetBytes(ready);
                byte[] timeLength = BitConverter.GetBytes(timeBuffer.Length);

                stream.Write(timeLength);
                stream.Write(timeBuffer);
                return;
            }
            

            if (File.Exists($"{dir}{fileName}.crp"))
            {
                SMHelpers.SendOCode(stream, OperationCodes.FileDoesExist);
                BFTP.Send(stream, $"{dir}{fileName}.crp");
            }
            else if (File.Exists($"{dir}{fileName}"))
            {
                SMHelpers.SendOCode(stream, OperationCodes.FileDoesExist);
                BFTP.Send(stream, $"{dir}{fileName}");
            }
            else
            {
                SMHelpers.SendOCode(stream, OperationCodes.FileDoesntExist);
                return;
            }


        }


        private class timeComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                FileInfo infoX = new FileInfo(x);
                FileInfo infoY = new FileInfo(y);

                return infoX.LastWriteTime.CompareTo(infoY.LastWriteTime);

            }
        }


        public void ListInbox(TcpClient client, NetworkStream stream)
        {
            string dir = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            string[] names = Directory.EnumerateFiles(dir).Where(fn => !fn.EndsWith(".key")).ToArray();

            Array.Sort(names, new timeComparer());

            int namesCount = names.Length;
            stream.Write(BitConverter.GetBytes(namesCount));

            foreach (string name in names)
            {
                //Console.WriteLine(name);

                string nameTrimmed; 
                if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    nameTrimmed = name.Split("\\")[1];
                else
                {
                    nameTrimmed = name.Split("/")[1]; 
                }

                nameTrimmed = nameTrimmed.Replace(".crp", "");

                int nameLength = nameTrimmed.Length;
                byte[] nameLengthBytes = BitConverter.GetBytes(nameLength);
                stream.Write(nameLengthBytes);

                byte[] nameBytes = Encoding.UTF8.GetBytes(nameTrimmed);
                stream.Write(nameBytes);
            }
        }

        [Obsolete("This does not work with multiple clients, use AServerLoop")]
        private void ServerLoop()
        {
            while (true)
            {
                if (listener.Pending())
                {
                    bool disconnected = false;
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    Handshake(client, stream);
                    while (client.Connected)
                    {
                        byte[] codeBuffer = new byte[4];
                        stream.Read(codeBuffer);
                        ServerCodes code = (ServerCodes)BitConverter.ToInt32(codeBuffer);
                        switch (code)
                        {
                            case ServerCodes.ClientSend:
                                Recieve(stream);
                                break;
                            case ServerCodes.ClientRecieve:
                                Send(client, stream);
                                break;
                            case ServerCodes.ClientRequestInbox:
                                ListInbox(client, stream);
                                break;
                            case ServerCodes.ClientDisconnect:
                                disconnected = true;
                                break;
                        }
                        if (disconnected)
                        {
                            client.Close();
                            break;
                        }
                    }
                    Console.WriteLine("Client Disconnected!");
                }
            }
        }
        private void AServerLoop()
        {
            while (true)
            {
                if (listener.Pending())
                {
                    connectionDone.Reset();
                    listener.BeginAcceptTcpClient(new AsyncCallback(acceptCallback), listener);
                    connectionDone.WaitOne();
                }
            }
        }
        private void acceptCallback(IAsyncResult ar)
        {
            connectionDone.Set();
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);
            NetworkStream stream = client.GetStream();
            bool disconnected = false;
            Handshake(client, stream);
            while (client.Connected)
            {
                try
                {
                    ServerCodes code = SMHelpers.ReceiveSCode(stream);
                    switch (code)
                    {
                        case ServerCodes.ClientSend:
                            Recieve(stream);
                            break;
                        case ServerCodes.ClientRecieve:
                            Send(client, stream);
                            break;
                        case ServerCodes.ClientRequestInbox:
                            ListInbox(client, stream);
                            break;
                        case ServerCodes.ClientDisconnect:
                            disconnected = true;
                            break;
                    }
                    if (disconnected)
                    {
                        client.Close();
                        break;
                    }
                }
                catch(System.IO.IOException e)              
                {
                    Console.WriteLine($"Client disconneted: {e.Message}");
                    break;
                }
                
            }
            Console.WriteLine("Client Disconnected!");
        }
    }

}
