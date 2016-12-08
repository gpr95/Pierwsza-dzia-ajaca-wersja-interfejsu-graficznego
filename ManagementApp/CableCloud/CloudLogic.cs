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


        public CloudLogic()
        {
            tableWithPorts = new DataTable("Connections");
            portToThreadMap = new Dictionary<int, NodeConnectionThread>();
            tableWithPorts.Columns.Add("fromPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("virtualFromPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("toPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("virtualToPort", typeof(int)).AllowDBNull = false;

            /** LOGS CONSOLE  */
            consoleWriter("Cloud start");
        }

        public void connectToWindowApplication(int port)
        {
            TcpClient connection = new TcpClient("localhost", port);
            Thread clientThread = new Thread(new ParameterizedThreadStart(windowConnectionThread));
            clientThread.Start(connection);
        }

        private void windowConnectionThread(Object connection)
        {
            consoleWriter("Connected with window application");
            TcpClient clienttmp = (TcpClient)connection;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            while (true)
            {
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                if (received_object.Type == typeof(ConnectionProperties))
                {
                    ConnectionProperties received_connection = received_object.Value.ToObject<ConnectionProperties>();
                    if (received_connection.LocalPortTo == 0 && received_connection.VirtualPortTo == 0)
                    {
                        deleteCable(received_connection.LocalPortFrom, received_connection.VirtualPortFrom);
                        consoleWriter("Deleted connection: real port:" + received_connection.LocalPortFrom +
                            "virtual port:" + received_connection.VirtualPortFrom);
                    }
                    connectToNodes(received_connection.LocalPortFrom, received_connection.VirtualPortFrom,
                       received_connection.LocalPortTo, received_connection.VirtualPortTo);
                }
                else
                {
                    consoleWriter(ERROR_MSG + "received from window application wrong data format.");
                }
            }
        }

        public void connectToNodes(int fromPort, int virtualFromPort,
                                    int toPort, int virtualToPort)
        {

            TcpClient connectionFrom = new TcpClient("localhost", fromPort);
            consoleWriter("Initialize connection: real port:" + fromPort +
                          " virtual port:" + virtualFromPort);
            NodeConnectionThread fromThread = new NodeConnectionThread(ref connectionFrom, 
                ref portToThreadMap, tableWithPorts);

            portToThreadMap.Add(fromPort, fromThread);

            TcpClient connectionTo = new TcpClient("localhost", toPort);
            consoleWriter("Initialize connection: real port:" + toPort +
                           " virtual port:" + virtualToPort);
            NodeConnectionThread toThread = new NodeConnectionThread(ref connectionTo,
                ref portToThreadMap, tableWithPorts);

            portToThreadMap.Add(toPort, toThread);

            /** Add new cable to table */
            addNewCable(fromPort, virtualFromPort,
               toPort, virtualToPort);
        }

        
        private void  addNewCable(int fromPort, int virtualFromPort, int toPort, int virtualToPort)
        {
            tableWithPorts.Rows.Add(fromPort, virtualFromPort, toPort, virtualToPort);
            tableWithPorts.Rows.Add(toPort, virtualToPort, fromPort, virtualFromPort);
            consoleWriter("Made connection: from-" + fromPort + "(" + virtualFromPort + ")" + " to-" +
                              toPort + "(" + virtualToPort + ")");
            consoleWriter("Made connection: from-" + toPort + "(" + virtualToPort + ")" + " to-" +
                              fromPort + "(" + virtualFromPort + ")");
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
        private void consoleWriter(String msg)
        {
            Console.WriteLine();
            Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:" + msg);
        }
    }
}
