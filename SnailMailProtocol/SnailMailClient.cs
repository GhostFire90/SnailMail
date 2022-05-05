using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BetterFileProtocol;
using System.Threading.Tasks;
using System.Threading;

namespace SnailMailProtocol
{
    public class SnailMailClient
    {
        TcpClient client;
        IPEndPoint endpoint;
        NetworkStream stream;
        public SnailMailClient(string ip = "127.0.0.1", int port = 90)
        {
            client = new TcpClient();
            IPAddress address;
            if(!IPAddress.TryParse(ip, out address))
            {
                throw new FormatException("ip is not a valid ip");
            }
            endpoint = new IPEndPoint(address, port);

        }

        public void Connect()
        {
            client.Connect(endpoint);
            stream = client.GetStream();
            Handshake();
        }

        /// <summary>
        /// Sends the server a signal saying the client is going to disconnect then closes the client
        /// </summary>
        public void Disconnect()
        {
            SMHelpers.SendSCode(stream, ServerCodes.ClientDisconnect);
            client.Close();
        }

        /// <summary>
        /// Handles Client Key sych with server
        /// </summary>
        void Handshake()
        {
            bool keyCreated = false;
            RSACryptoServiceProvider rsa;
            if (!File.Exists("public.key") || !File.Exists("private.key"))
            {
                rsa = new RSACryptoServiceProvider();

                FileStream fs = File.Open("public.key", FileMode.OpenOrCreate);

                fs.Write(rsa.ExportRSAPublicKey());

                fs.Close();

                fs = File.Open("private.key", FileMode.OpenOrCreate);
                fs.Write(rsa.ExportRSAPrivateKey());
                fs.Close();
                keyCreated = true;
            }

            HandshakeCodes code = SMHelpers.ReceiveHCode(stream);


            if (code == HandshakeCodes.ServerDoesntHaveKey)
            {

                BFTP.Send(stream, "public.key");

            }
            else if (keyCreated)
            {
                SMHelpers.SendHCode(stream, HandshakeCodes.KeyCreated);
                BFTP.Send(stream, "public.key");
            }
            else
            {
                SMHelpers.SendHCode(stream, HandshakeCodes.ServerHasKey);
            }

        }

        /// <summary>
        /// Sends the selected file to the chosen recipient
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <param name="reciever">IP of the recipient</param>
        public void Send(string path, string reciever)
        {
            SMHelpers.SendSCode(stream, ServerCodes.ClientSend);
            //stream.Write(BitConverter.GetBytes((int)ServerCodes.ClientSend));
            string dir = $".keys/{reciever}/";
            if (!Directory.Exists(".keys")) Directory.CreateDirectory(".keys");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            stream.Write(BitConverter.GetBytes(reciever.Length));
            stream.Write(Encoding.UTF8.GetBytes(reciever));

            BFTP.Recieve(stream, 256, dir);

            Aes aes = Aes.Create();
            ICryptoTransform transform = aes.CreateEncryptor();

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            FileStream keyStream = File.OpenRead($"{dir}public.key");

            byte[] keyBuffer = new byte[keyStream.Length];
            keyStream.Read(keyBuffer, 0, keyBuffer.Length);

            rsa.ImportRSAPublicKey(keyBuffer, out _);
            keyStream.Close();

            byte[] ivLenghth = BitConverter.GetBytes(aes.IV.Length);
            byte[] iv = aes.IV;

            byte[] keyEncrypted = rsa.Encrypt(aes.Key, false);
            byte[] keyLength = BitConverter.GetBytes(keyEncrypted.Length);

            using (FileStream copy = File.Open(path + ".crp", FileMode.OpenOrCreate))
            {
                copy.Write(ivLenghth);
                copy.Write(iv);

                copy.Write(keyLength);
                copy.Write(keyEncrypted);

                using (CryptoStream copyEncrypted = new CryptoStream(copy, transform, CryptoStreamMode.Write))
                {
                    // = chunkSize;

                    byte[] buffer = new byte[aes.BlockSize];
                    int bytesRead = 0;

                    using (FileStream inFs = File.OpenRead(path))
                    {
                        while ((bytesRead = inFs.Read(buffer, 0, aes.BlockSize)) != 0)
                        {
                            copyEncrypted.Write(buffer, 0, bytesRead);
                        }
                    }
                    copyEncrypted.FlushFinalBlock();
                }

            }
            BFTP.Send(stream, path + ".crp");
            File.Delete(path + ".crp");


        }
        public delegate Task<bool> NoKey();
        public async Task Send(string path, string reciever, IProgress<float> progress, NoKey noKey)
        {
            SMHelpers.SendSCode(stream, ServerCodes.ClientSend);
            string dir = $".keys/{reciever}/";
            if (!Directory.Exists(".keys")) Directory.CreateDirectory(".keys");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            stream.Write(BitConverter.GetBytes(reciever.Length));
            stream.Write(Encoding.UTF8.GetBytes(reciever));

            OperationCodes code = SMHelpers.ReceiveOCode(stream);

            Progress<float> progress1 = new((float value) =>
            {
                progress.Report(value);
            });

            bool cont = false;
            bool trigger = await noKey();
            if(code == OperationCodes.NoKey)
            {
                cont = (trigger);
                if (cont)
                {
                    SMHelpers.SendOCode(stream, OperationCodes.ContinueSend);
                    await BFTP.Send(stream, path, 256, progress1);
                }
                else
                {
                    SMHelpers.SendOCode(stream, OperationCodes.AbortSend);
                    return;
                }
            }
            else
            {
                BFTP.Recieve(stream, 256, dir);

                Aes aes = Aes.Create();
                ICryptoTransform transform = aes.CreateEncryptor();

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                FileStream keyStream = File.OpenRead($"{dir}public.key");

                byte[] keyBuffer = new byte[keyStream.Length];
                keyStream.Read(keyBuffer, 0, keyBuffer.Length);

                rsa.ImportRSAPublicKey(keyBuffer, out _);
                keyStream.Close();

                byte[] ivLenghth = BitConverter.GetBytes(aes.IV.Length);
                byte[] iv = aes.IV;

                byte[] keyEncrypted = rsa.Encrypt(aes.Key, true);
                byte[] keyLength = BitConverter.GetBytes(keyEncrypted.Length);

                using (FileStream copy = File.Open(path + ".crp", FileMode.OpenOrCreate))
                {
                    copy.Write(ivLenghth);
                    copy.Write(iv);

                    copy.Write(keyLength);
                    copy.Write(keyEncrypted);

                    using (CryptoStream copyEncrypted = new CryptoStream(copy, transform, CryptoStreamMode.Write))
                    {
                        // = chunkSize;

                        byte[] buffer = new byte[aes.BlockSize];
                        int bytesRead = 0;

                        using (FileStream inFs = File.OpenRead(path))
                        {
                            while ((bytesRead = inFs.Read(buffer, 0, aes.BlockSize)) != 0)
                            {
                                copyEncrypted.Write(buffer, 0, bytesRead);
                            }
                        }
                        copyEncrypted.FlushFinalBlock();
                    }

                }
                await BFTP.Send(stream, path + ".crp", 256, progress1);
                
                File.Delete(path + ".crp");
            }
            
        }
        

        /// <summary>
        /// Recieves the file from the server
        /// </summary>
        /// <param name="fileName">The name of the file wished to be recieved</param>
        public string Recieve(string fileName)
        {
            SMHelpers.SendSCode(stream, ServerCodes.ClientRecieve);
            if (!Directory.Exists("inbox")) Directory.CreateDirectory("inbox");
            byte[] nameLength = BitConverter.GetBytes(fileName.Length);
            byte[] name = Encoding.UTF8.GetBytes(fileName);

            stream.Write(nameLength);
            stream.Write(name);


            OperationCodes code = SMHelpers.ReceiveOCode(stream);

            string exitDir = "";

            if (code == OperationCodes.WaitTimeOver) ;
            else if (code == OperationCodes.WaitTime)
            {
                byte[] timeLength = new byte[4];
                stream.Read(timeLength);

                byte[] timeBuffer = new byte[BitConverter.ToInt32(timeLength)];
                stream.Read(timeBuffer);


                //Console.WriteLine($"{fileName} will be ready on {Encoding.UTF8.GetString(timeBuffer)}");
                return $"{fileName} will be ready on {Encoding.UTF8.GetString(timeBuffer)}";
            }
            else
            {
                throw new Exception("Unknown code for this opperation");
            }

            code = SMHelpers.ReceiveOCode(stream);

            if (code == OperationCodes.FileDoesExist)
            {
                exitDir = BFTP.Recieve(stream, 256, "inbox/");
            }
            else if (code == OperationCodes.FileDoesntExist)
            {
                return $"{fileName} Does not exist on this server";
            }
            else
            {
                throw new Exception("Unknown code for this opperation");
            }

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

            FileStream keyStream = File.OpenRead($"private.key");

            byte[] keyBuffer = new byte[keyStream.Length];
            keyStream.Read(keyBuffer, 0, keyBuffer.Length);

            rsa.ImportRSAPrivateKey(keyBuffer, out _);
            keyStream.Close();

            using (FileStream inFs = File.Open(exitDir, FileMode.Open))
            {
                byte[] ivLenghth = new byte[4];
                inFs.Read(ivLenghth, 0, 4);

                byte[] iv = new byte[BitConverter.ToInt32(ivLenghth)];
                inFs.Read(iv, 0, iv.Length);


                byte[] keyLength = new byte[4];
                inFs.Read(keyLength, 0, 4);

                byte[] keyEncrypted = new byte[BitConverter.ToInt32(keyLength)];
                inFs.Read(keyEncrypted, 0, keyEncrypted.Length);

                byte[] keyDecrypted = rsa.Decrypt(keyEncrypted, true);

                Aes aes = Aes.Create();

                aes.Key = keyDecrypted;
                aes.KeySize = BitConverter.ToInt32(keyLength);

                aes.IV = iv;

                ICryptoTransform transform = aes.CreateDecryptor(keyDecrypted, iv);

                string correctFile = exitDir.Substring(0, exitDir.Length - 4);
                using (FileStream outFs = File.Open(correctFile, FileMode.OpenOrCreate))
                {
                    using (CryptoStream outDec = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                    {
                        int bytesRead = 0;
                        byte[] buffer = new byte[aes.BlockSize];

                        while ((bytesRead = inFs.Read(buffer, 0, aes.BlockSize)) != 0)
                        {
                            outDec.Write(buffer, 0, bytesRead);
                        }
                        outDec.FlushFinalBlock();

                    }
                }

            }
            File.Delete(exitDir);
            return "";
        }

        /// <summary>
        /// Requests a list of file names from the server, sorted from oldest to newest
        /// </summary>
        /// <returns>list of file names</returns>
        public string[] RequestInbox()
        {
            SMHelpers.SendSCode(stream, ServerCodes.ClientRequestInbox);

            byte[] namesCountBytes = new byte[4];
            stream.Read(namesCountBytes);
            int namesCount = BitConverter.ToInt32(namesCountBytes);

            string[] names = new string[namesCount];

            for (int i = 0; i < namesCount; i++)
            {
                byte[] nameLengthBytes = new byte[4];
                stream.Read(nameLengthBytes);
                int nameLength = BitConverter.ToInt32(nameLengthBytes);

                byte[] nameBytes = new byte[nameLength];
                stream.Read(nameBytes);

                string name = Encoding.UTF8.GetString(nameBytes);

                names[i] = name;
            }

            return names;
        }

        public string IpAsString()
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        }

        ~SnailMailClient()
        {
            Disconnect();
        }
    }

}
