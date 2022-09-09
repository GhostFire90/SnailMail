using System.Threading.Tasks;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Net;

namespace SnailMailProtocol
{
    public enum ServerCodes { ClientSend, ClientRecieve, ClientDisconnect, ClientRequestInbox }
    public enum OperationCodes { WaitTimeOver, WaitTime, FileDoesntExist, FileDoesExist, NoKey, YesKey, AbortSend, ContinueSend }
    public enum HandshakeCodes { ServerHasKey, ServerDoesntHaveKey, KeyCreated }
    
    public static class SMHelpers
    {
        public static ServerCodes ReceiveSCode(System.Net.Sockets.NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer);
            return (ServerCodes)BitConverter.ToInt32(buffer);
        } 
        public static OperationCodes ReceiveOCode(System.Net.Sockets.NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer);
            return (OperationCodes)BitConverter.ToInt32(buffer);
        }
        public static HandshakeCodes ReceiveHCode(System.Net.Sockets.NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer);
            return (HandshakeCodes)BitConverter.ToInt32(buffer);
        }

        public static void SendSCode(System.Net.Sockets.NetworkStream stream, ServerCodes code)
        {
            byte[] buffer = BitConverter.GetBytes((int)code);
            stream.Write(buffer);
        }
        public static void SendOCode(System.Net.Sockets.NetworkStream stream, OperationCodes code)
        {
            byte[] buffer = BitConverter.GetBytes((int)code);
            stream.Write(buffer);
        }
        public static void SendHCode(System.Net.Sockets.NetworkStream stream, HandshakeCodes code)
        {
            byte[] buffer = BitConverter.GetBytes((int)code);
            stream.Write(buffer);
        }
    }
    public class SMAddress
    {
        private bool isDirty = true;

        public IPAddress Address
        {
            set
            {
                address = value;
                isDirty = true;
            }
            get
            {
                return address;
            }
        }
        private IPAddress address;

        public string Username
        {
            set
            {
                username = value;
                isDirty = true;
            }
        }
        
        private string username;

        public string Data
        {
            get
            {
                if (isDirty)
                {
                    data = $"{address.ToString()}:{username}";
                    isDirty = false;
                }
                return data;
            }
        }
        private string data;


        public SMAddress(IPAddress addr, string username)
        {
            Address = addr;
            Username = username;
        }
        public SMAddress(string fullAddr)
        {
            string[] addrs = fullAddr.Split(':');
            Address = IPAddress.Parse(addrs[0]);
            if(addrs.Length > 1)
            {
                Username = addrs[1];
            }
            
        }


    }
}
