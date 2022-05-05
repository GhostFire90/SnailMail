using System.Threading.Tasks;
using System.Collections;
using System;

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

}
