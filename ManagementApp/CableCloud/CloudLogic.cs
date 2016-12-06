using ClientNode;
using ManagementApp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CableCloud
{
    class CloudLogic
    {
        /** TABLE WITH CONNECTION */
        private DataTable table;

        /** HANDLERS MAP - localPORT-Thread with connection to this port */
        private Dictionary<int, NodeConnectionThread> portToThreadMap;

        public CloudLogic()
        {
            table = new DataTable("Connections");
            portToThreadMap = new Dictionary<int, NodeConnectionThread>();
            table.Columns.Add("fromPort", typeof(int)).AllowDBNull = false;
            table.Columns.Add("virtualFromPort", typeof(int)).AllowDBNull = false;
            table.Columns.Add("toPort", typeof(int)).AllowDBNull = false;
            table.Columns.Add("virtualToPort", typeof(int)).AllowDBNull = false;
        }

        public void connectToWindowApplication(int port)
        {
            TcpClient connection = new TcpClient("localhost", port);
            Thread clientThread = new Thread(new ParameterizedThreadStart(windowConnectionThread));
            clientThread.Start(connection);
        }

        private void windowConnectionThread(Object connection)
        {
            TcpClient clienttmp = (TcpClient)connection;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            while (true)
            {
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type == typeof(NodeConnection))
                {
                    NodeConnection received_connection = received_object.Value.ToObject<NodeConnection>();
                    if (received_connection.LocalPortTo == 0 && received_connection.VirtualPortTo == 0)
                        deleteCable(received_connection.LocalPortFrom, received_connection.VirtualPortFrom);
                    connectToNodes(received_connection.LocalPortFrom, received_connection.VirtualPortFrom,
                       received_connection.LocalPortTo, received_connection.VirtualPortTo);
                }
                else
                {
                    Console.WriteLine("\n Connection with Window Application: WRONG DATA");
                }
            }
        }

        public void connectToNodes(int fromPort, int virtualFromPort,
                                    int toPort, int virtualToPort)
        {

            TcpClient connectionFrom = new TcpClient("localhost", fromPort);
            NodeConnectionThread fromThread = new NodeConnectionThread(ref connectionFrom, ref portToThreadMap, virtualToPort, table);
            portToThreadMap.Add(((IPEndPoint)connectionFrom.Client.RemoteEndPoint).Port, fromThread);
            Console.WriteLine("\n MAKING CLOUD IN/OUT CONNECTION: PORT:" + ((IPEndPoint)connectionFrom.Client.RemoteEndPoint).Port);
            TcpClient connectionTo = new TcpClient("localhost", toPort);
            NodeConnectionThread toThread = new NodeConnectionThread(ref connectionTo, ref portToThreadMap, virtualFromPort, table);
            portToThreadMap.Add(((IPEndPoint)connectionTo.Client.RemoteEndPoint).Port, toThread);
            Console.WriteLine("\n MAKING CLOUD IN/OUT CONNECTION: PORT:" + ((IPEndPoint)connectionTo.Client.RemoteEndPoint).Port);
            addNewCable(((IPEndPoint)connectionFrom.Client.RemoteEndPoint).Port, virtualFromPort,
                ((IPEndPoint)connectionTo.Client.RemoteEndPoint).Port, virtualToPort);
        }

        

        private void  addNewCable(int fromPort, int virtualFromPort, int toPort, int virtualToPort)
        {
            table.Rows.Add(fromPort, virtualFromPort, toPort, virtualToPort);
        }

        private void deleteCable(int fromPort, int virtualFromPort)
        {
            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = table.Rows[i];
                if (dr["fromPort"].Equals(fromPort) && dr["virtualFromPort"].Equals(virtualFromPort))
                {
                    table.Rows.Remove(dr);
                    portToThreadMap.Remove(fromPort);
                }
            }
        }

        private class NodeConnectionThread
        {
            private Thread thread;

            private TcpClient connection;
            private int virtualToPort;
            private DataTable table;
            private Dictionary<int, NodeConnectionThread> portToThreadMap;
            private BinaryWriter writer;
            private BinaryReader reader;

            public NodeConnectionThread(ref TcpClient connection, 
                ref  Dictionary<int,NodeConnectionThread> portToThreadMap,  int virtualToPort, DataTable table)
            {
                this.virtualToPort = virtualToPort;
                this.portToThreadMap = portToThreadMap;
                this.connection = connection;
                this.table = table;
                writer = new BinaryWriter(connection.GetStream());
                reader = new BinaryReader(connection.GetStream());
                thread = new Thread(nodeConnectionThread);
                thread.Start();
            }

            private void nodeConnectionThread()
            {
                Console.Write("THREAD STARTED\n");
                while (true)
                {
                    string received_data = reader.ReadString();
                    if (received_data == null || received_data.Length == 0)
                        continue;
                    Console.Write("THREAD RECEIVED:\n"+received_data);
                    JMessage received_object = JMessage.Deserialize(received_data);
                    if (received_object.Type == typeof(Signal))
                    {
                        Signal signal = received_object.Value.ToObject<Signal>();
                        int virtualFromPort = signal.port;
                        var fromPort = ((IPEndPoint)connection.Client.RemoteEndPoint).Port;
                        int virtualToPort = 0;
                        int toPort = 0;
                        for (int i = table.Rows.Count - 1; i >= 0; i--)
                        {
                            DataRow dr = table.Rows[i];
                            if (dr["fromPort"].Equals(fromPort) && dr["virtualFromPort"].Equals(virtualFromPort))
                            {
                                toPort = (int)dr["toPort"];
                                virtualToPort = (int)dr["virtualToPort"];
                            }
                        }
                        signal.port = virtualToPort;
                        portToThreadMap[toPort].sendSignal(signal, toPort);
                    }
                    else
                    {
                        Console.WriteLine("\n Connection with Node: WRONG DATA");
                    }
                }
            }

            public void sendSignal(Signal toSend, int port)
            {
                toSend.port = port;
                String data = JSON.Serialize(JSON.FromValue(toSend));
                writer.Write(data);
                Console.WriteLine("Sended data OUT");
            }
        
        }
    }
}
