using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementApp
{
    class CloudCableHandler
    {
        //private 
        private List<NodeConnection> connections;
        private TcpClient client;
        private BinaryWriter writer;
                private    BinaryReader reader;

        public CloudCableHandler(List<NodeConnection> connections, int cloudPort)
        {
            this.connections = connections;
            TcpListener listener = new TcpListener(IPAddress.Parse("localhost"), cloudPort);
            listener.Start();
            client = listener.AcceptTcpClient();
            writer = new BinaryWriter(client.GetStream());
            reader = new BinaryReader(client.GetStream());
        }

        public void updateConnections()
        {
            foreach(NodeConnection con in connections)
            {
                String data = JSON.Serialize(JSON.FromValue(con));
                writer.Write(data);
            }
        }
        public void updateOneConnection()
        {
            String data = JSON.Serialize(JSON.FromValue(connections.Last()));
            writer.Write(data);
        }
    }
}
