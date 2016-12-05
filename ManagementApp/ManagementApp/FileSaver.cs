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
        public string FILE_PATH_NODES= Path.Combine(Environment.CurrentDirectory, @"Data\", "Nodes.bin");
        public string FILE_PATH_NODECONNECTIONS = Path.Combine(Environment.CurrentDirectory, @"Data\", "NodeConnections.bin");
        public string FILE_PATH_DOMAINS = Path.Combine(Environment.CurrentDirectory, @"Data\", "Domains.bin");

        public void WriteToBinaryFile(List<Node> nodeList, List<NodeConnection> connectionList, List<Domain> domainList)
        {
            using (Stream stream = File.Open(FILE_PATH_NODES, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, nodeList);
            }

            using (Stream stream = File.Open(FILE_PATH_NODECONNECTIONS, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, connectionList);
            }

            using (Stream stream = File.Open(FILE_PATH_DOMAINS, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, domainList);
            }
        }

        public List<Node> ReadFromBinaryFileNodes()
        {
            using (Stream stream = File.Open(FILE_PATH_NODES, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (List<Node>)binaryFormatter.Deserialize(stream);
            }
        }

        public List<Node> ReadFromBinaryFileNodeConnections()
        {
            using (Stream stream = File.Open(FILE_PATH_NODECONNECTIONS, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (List<Node>)binaryFormatter.Deserialize(stream);
            }
        }

        public List<Node> ReadFromBinaryFileDomains()
        {
            using (Stream stream = File.Open(FILE_PATH_DOMAINS, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (List<Node>)binaryFormatter.Deserialize(stream);
            }
        }
    }
}
