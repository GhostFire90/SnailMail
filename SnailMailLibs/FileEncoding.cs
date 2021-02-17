using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;

using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;

namespace SnailMailLibs
{
    public class FileEncoding
    {
        //Used locally to turn a file from path to bytes for sending 
        public byte[] FileToBytes(string path)
        {
            byte[] bytes;
            var fileStream = File.OpenRead(path);
            var binaryReader = new BinaryReader(fileStream);
            bytes = binaryReader.ReadBytes((int)fileStream.Length);

            return bytes;
        }
        
        private ByteFileStructure fileStructurePrepper(FileStructure fs)
        {
            ByteFileStructure bfs = new ByteFileStructure()
            {
                fileName = Encoding.UTF8.GetBytes(fs.fileName),
                fileData = fs.fileData,
                dateSent = Encoding.UTF8.GetBytes(fs.dateSent),
                ipSender = fs.ipSender.GetAddressBytes()
            };
            return bfs;
        }
        private FileStructure fileStructurePrepper(ByteFileStructure bfs)
        {
            FileStructure fs = new FileStructure()
            {
                fileName = Encoding.UTF8.GetString(bfs.fileName),
                
                fileData = bfs.fileData,
                dateSent = Encoding.UTF8.GetString(bfs.dateSent),
                ipSender = new IPAddress(bfs.ipSender)
            };
            return fs;
        }
        
        private byte[] structureSerializer(ByteFileStructure bfs)
        {
            byte[] bytes;
            using (var memStream = new MemoryStream())
            {
                
                var bF = new BinaryFormatter();
                bF.Serialize(memStream, bfs);
                bytes = memStream.ToArray();
            }
            return bytes;
        }

         ByteFileStructure structureDeserializer(byte[] bytes)
        {
            using (var memStream = new MemoryStream())
            {
                var bF = new BinaryFormatter();
                memStream.Write(bytes, 0, bytes.Length);
                System.Diagnostics.Debug.WriteLine(memStream.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                ByteFileStructure bfs = (ByteFileStructure)bF.Deserialize(memStream);
                return bfs;   
            }
        }

        public byte[] StreamPrepper(FileStructure fs)
        {
            byte[] mainData = structureSerializer(fileStructurePrepper(fs));
            List<byte> fullLoad = new List<byte>();
            fullLoad.AddRange(BitConverter.GetBytes(mainData.Length));
            fullLoad.AddRange(mainData);
            return fullLoad.ToArray();
        }
        public FileStructure FileStructureRetriever(NetworkStream stream)
        {
            byte[] length = new byte[4];
            stream.Read(length, 0, 4);
            byte[] mainData = new byte[BitConverter.ToInt32(length, 0)];
            stream.Read(mainData, 0, mainData.Length);
            FileStructure fs = fileStructurePrepper(structureDeserializer(mainData));
            return fs;
        }
        public FileStructure FileStructureRetriever(MemoryStream stream)
        {
            byte[] length = new byte[4];
            stream.Read(length, 0, 4);
            byte[] mainData = new byte[BitConverter.ToInt32(length, 0)];
            stream.Read(mainData, 0, mainData.Length);
            FileStructure fs = fileStructurePrepper(structureDeserializer(mainData));
            return fs;
        }

        [Serializable]
        struct ByteFileStructure
        {
            public byte[] fileName;
            public byte[] fileData;
            public byte[] dateSent;
            public byte[] ipSender;
        }
        public struct FileStructure
        {
            public string fileName;
            public byte[] fileData;
            public string dateSent;
            /// <summary>
            /// The Ip of the reciever
            /// </summary>
            public IPAddress ipSender;
        }

    }
}
