using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    class FileSaver
    {
        private string path;
        private string filePathNodes;
        private string filePathNodeConnection;
        private string filePathDomains;
        private string filePathTrails;

        public FileSaver(String path)
        {
            this.path = path;
            this.filePathNodes = path + "_NODES.bin";
            this.filePathNodeConnection = path + "_NODE_CONNECTIONS.bin";
            this.filePathDomains = path + "_DOMAINS.bin";
            this.filePathTrails = path + "_TRAILS";
        }
        public void WriteToBinaryFile(List<Node> nodeList, List<NodeConnection> connectionList, List<Domain> domainList)
        {
            using (Stream stream = File.Open(path, FileMode.Create)) { }
            using (Stream stream = File.Open(filePathNodes, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, nodeList);
            }

            using (Stream stream = File.Open(filePathNodeConnection, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, connectionList);
            }

            using (Stream stream = File.Open(filePathDomains, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, domainList);
            }

     /*       using (Stream stream = File.Open(filePathTrails, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, trailList);
            } */
        }

        public List<Node> ReadFromBinaryFileNodes()
        {
            if (!File.Exists(filePathNodes))
                return new List<Node>();
            using (Stream stream = File.Open(filePathNodes, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (List<Node>)binaryFormatter.Deserialize(stream);
            }
        }

        public List<NodeConnection> ReadFromBinaryFileNodeConnections()
        {
            if (!File.Exists(filePathNodeConnection))
                return new List<NodeConnection>();
            using (Stream stream = File.Open(filePathNodeConnection, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (List<NodeConnection>)binaryFormatter.Deserialize(stream);
            }
        }

        public List<Domain> ReadFromBinaryFileDomains()
        {
            if (!File.Exists(filePathDomains))
                return new List<Domain>();
            using (Stream stream = File.Open(filePathDomains, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (List<Domain>)binaryFormatter.Deserialize(stream);
            }
        }

        //public List<Trail> ReadFromBinaryFileTrails()
        //{
        //    if (!File.Exists(filePathTrails))
        //        return new List<Trail>();
        //    using (Stream stream = File.Open(filePathTrails, FileMode.Open))
        //    {
        //        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //        return (List<Trail>)binaryFormatter.Deserialize(stream);
        //    }
        //}
    }
}
