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
        private string pathToLogFile;
        private TcpClient connection;
        private DataTable table;
        private Dictionary<int, NodeConnectionThread> portToThreadMap;
        private BinaryWriter writer;
        private BinaryReader reader;

        public NodeConnectionThread(ref TcpClient connection,
            ref Dictionary<int, NodeConnectionThread> portToThreadMap, DataTable table, string pathToLogFile)
        {
            this.connection = connection;
            this.portToThreadMap = portToThreadMap;
            this.table = table;
            this.pathToLogFile = pathToLogFile;

            writer = new BinaryWriter(connection.GetStream());
            reader = new BinaryReader(connection.GetStream());

            thread = new Thread(nodeConnectionThread);
            thread.Start();
        }

        private void nodeConnectionThread()
        {
            writeToLog("Connection made with: " + ((IPEndPoint)connection.Client.RemoteEndPoint).Port);
            while (true)
            {
                string received_data = reader.ReadString();
                if (received_data == null || received_data.Length == 0)
                    continue;

                writeToLog("Connection: " + ((IPEndPoint)connection.Client.RemoteEndPoint).Port + " received object.");
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
                            writeToLog("Connection: " + ((IPEndPoint)connection.Client.RemoteEndPoint).Port + " received object:"+
                                fromPort + ":" + virtualFromPort + "-" + toPort + ":" + virtualToPort);
                        }
                    }
                    signal.port = virtualToPort;
                    portToThreadMap[toPort].sendSignal(signal, toPort);
                }
                else
                {
                    writeToLog(ERROR_MSG + "received from node wrong data format. Node TCP PORT: "+ ((IPEndPoint)connection.Client.RemoteEndPoint).Port);
                }
            }
        }

        public void sendSignal(Signal toSend, int port)
        {
            toSend.port = port;
            String data = JSON.Serialize(JSON.FromValue(toSend));

            writer.Write(data);
            writeToLog("Connection: " + ((IPEndPoint)connection.Client.RemoteEndPoint).Port + " sended data OUT.");
        }

        private void writeToLog(String logMsg)
        {
            StreamWriter writer = File.AppendText(pathToLogFile);
            writer.WriteLine("#" + DateTime.Now.ToLongTimeString() +
                DateTime.Now.ToLongDateString() + "#:" + logMsg);
        }

    }
}
