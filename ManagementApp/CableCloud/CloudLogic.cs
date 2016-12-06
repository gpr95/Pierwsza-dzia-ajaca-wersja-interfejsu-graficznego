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
        private const string ERROR_MSG = "ERROR: ";


        /** TABLE WITH CONNECTION */
        private DataTable tableWithPorts;

        /** HANDLERS MAP - localPORT-Thread with connection to this port */
        private Dictionary<int, NodeConnectionThread> portToThreadMap;

        /** Path to file with logs */
        private String pathToLogFile;

        public CloudLogic()
        {
            tableWithPorts = new DataTable("Connections");
            portToThreadMap = new Dictionary<int, NodeConnectionThread>();
            tableWithPorts.Columns.Add("fromPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("virtualFromPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("toPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("virtualToPort", typeof(int)).AllowDBNull = false;

            /** LOGS FILE (create DIR-logs if doesn't exist) */
            pathToLogFile = Path.Combine(Environment.CurrentDirectory, @"logs\", "cloudLog.txt");
            System.IO.Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, @"logs\"));
            writeToLog("Cloud start");
        }

        private void writeToLog(String logMsg)
        {
            StreamWriter writer = File.AppendText(pathToLogFile);
            writer.WriteLine("#" + DateTime.Now.ToLongTimeString() + 
                DateTime.Now.ToLongDateString()+"#:" +logMsg);
        }

        public void connectToWindowApplication(int port)
        {
            TcpClient connection = new TcpClient("localhost", port);
            Thread clientThread = new Thread(new ParameterizedThreadStart(windowConnectionThread));
            clientThread.Start(connection);
        }

        private void windowConnectionThread(Object connection)
        {
            writeToLog("Connected with window application");
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
                    {
                        deleteCable(received_connection.LocalPortFrom, received_connection.VirtualPortFrom);
                        writeToLog("Deleted connection: real port:" + received_connection.LocalPortFrom +
                            "virtual port:" + received_connection.VirtualPortFrom);
                    }
                    connectToNodes(received_connection.LocalPortFrom, received_connection.VirtualPortFrom,
                       received_connection.LocalPortTo, received_connection.VirtualPortTo);
                }
                else
                {
                    writeToLog(ERROR_MSG + "received from window application wrong data format.");
                }
            }
        }

        public void connectToNodes(int fromPort, int virtualFromPort,
                                    int toPort, int virtualToPort)
        {

            TcpClient connectionFrom = new TcpClient("localhost", fromPort);
            writeToLog("Initialize connection: real port:" + toPort +
                          " virtual port:" + virtualToPort + " TCP PORT: " + ((IPEndPoint)connectionFrom.Client.RemoteEndPoint).Port);
            NodeConnectionThread fromThread = new NodeConnectionThread(ref connectionFrom, 
                ref portToThreadMap, tableWithPorts, pathToLogFile);

            portToThreadMap.Add(((IPEndPoint)connectionFrom.Client.RemoteEndPoint).Port, fromThread);


            TcpClient connectionTo = new TcpClient("localhost", toPort);
            writeToLog("Initialize connection: real port:" + toPort +
                           " virtual port:" + virtualToPort + " TCP PORT: " + ((IPEndPoint)connectionTo.Client.RemoteEndPoint).Port);
            NodeConnectionThread toThread = new NodeConnectionThread(ref connectionTo,
                ref portToThreadMap, tableWithPorts, pathToLogFile);

            portToThreadMap.Add(((IPEndPoint)connectionTo.Client.RemoteEndPoint).Port, toThread);


            /** Add new cable to table */
            addNewCable(((IPEndPoint)connectionFrom.Client.RemoteEndPoint).Port, virtualFromPort,
                ((IPEndPoint)connectionTo.Client.RemoteEndPoint).Port, virtualToPort);
        }

        
        private void  addNewCable(int fromPort, int virtualFromPort, int toPort, int virtualToPort)
        {
            tableWithPorts.Rows.Add(fromPort, virtualFromPort, toPort, virtualToPort);
        }

        private void deleteCable(int fromPort, int virtualFromPort)
        {
            for (int i = tableWithPorts.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = tableWithPorts.Rows[i];
                if (dr["fromPort"].Equals(fromPort) && dr["virtualFromPort"].Equals(virtualFromPort))
                {
                    tableWithPorts.Rows.Remove(dr);
                    portToThreadMap.Remove(fromPort);
                }
            }
        }
    }
}
