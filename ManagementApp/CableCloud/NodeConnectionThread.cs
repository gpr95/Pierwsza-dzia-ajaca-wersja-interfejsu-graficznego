using ClientNode;
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
    class NodeConnectionThread
    {
        private const string ERROR_MSG = "ERROR: ";

        private Thread thread;
        private TcpClient connection;
        private DataTable table;
        private Dictionary<String, NodeConnectionThread> portToThreadMap;
        private BinaryWriter writer;
        private BinaryReader reader;
        private String name;

        public NodeConnectionThread(ref TcpClient connection,
            ref Dictionary<String, NodeConnectionThread> portToThreadMap, DataTable table, String name)
        {
            this.connection = connection;
            this.portToThreadMap = portToThreadMap;
            this.table = table;
            this.name = name;

            writer = new BinaryWriter(connection.GetStream());
            reader = new BinaryReader(connection.GetStream());

            thread = new Thread(nodeConnectionThread);
            thread.Start();
        }

        private void nodeConnectionThread()
        {
            while (true)
            {
                string received_data = null;
                try
                {
                    received_data = reader.ReadString();
                }
                catch(IOException ex)
                {
                    consoleWriter("ERROR: Connection LOST.");
                    return;
                }
                if (received_data == null || received_data.Length == 0)
                    continue;

                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type == typeof(Signal))
                {
                    Signal signal = received_object.Value.ToObject<Signal>();  

                    var fromPort = ((IPEndPoint)connection.Client.RemoteEndPoint).Port;
                    int virtualFromPort = signal.port;
                    
                    int toPort = 0;
                    int virtualToPort = 0;

                    for (int i = table.Rows.Count - 1; i >= 0; i--)
                    {
                        DataRow dr = table.Rows[i];
                        if (dr["fromPort"].Equals(fromPort) && dr["virtualFromPort"].Equals(virtualFromPort))
                        {
                            toPort = (int)dr["toPort"];
                            virtualToPort = (int)dr["virtualToPort"];
                            consoleWriter("Connection: " + name + " received data.");
                        }
                    }
                    signal.port = virtualToPort;
                    consoleWriter("Connection: " + name + " sending data.");
                    portToThreadMap[toPort + ":" + virtualToPort].sendSignal(signal, toPort);
                }
                else
                {
                    consoleWriter(ERROR_MSG + "received from node wrong data format. Node PORT: "+ ((IPEndPoint)connection.Client.RemoteEndPoint).Port);
                }
                Thread.Sleep(150);
            }
        }

        public void sendSignal(Signal toSend, int port)
        {
            String data = JSON.Serialize(JSON.FromValue(toSend));
            writer.Write(data);
        }

        private void consoleWriter(String msg)
        {
            Console.WriteLine();
            Console.Write("#" + DateTime.Now.ToLongTimeString() + DateTime.Now.ToLongDateString() + "#:" + msg);
        }

    }
}
