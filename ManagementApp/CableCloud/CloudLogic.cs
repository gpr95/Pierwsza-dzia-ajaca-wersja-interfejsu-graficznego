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
        private Dictionary<String, NodeConnectionThread> portToThreadMap;


        public CloudLogic()
        {
            tableWithPorts = new DataTable("Connections");
            portToThreadMap = new Dictionary<String, NodeConnectionThread>();
            tableWithPorts.Columns.Add("fromPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("virtualFromPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("toPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("virtualToPort", typeof(int)).AllowDBNull = false;

            /** LOGS CONSOLE  */
            consoleWriter("Cloud start");
        }

        public void connectToWindowApplication(int port)
        {
            TcpClient connection = null;
            try
            {
                connection = new TcpClient("localhost", port);
            }
            catch(SocketException ex)
            {
                consoleWriter("ERROR: Cannot connect with window application.");
            }
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
                string received_data = null;
                try
                {
                    received_data = reader.ReadString();
                    JMessage receivedMessage = JMessage.Deserialize(received_data);
                    if (receivedMessage.Type == typeof(ConnectionProperties))
                    {
                        ConnectionProperties received_connection = receivedMessage.Value.ToObject<ConnectionProperties>();
                        if (received_connection.LocalPortTo == 0 && received_connection.VirtualPortTo == 0)
                        {
                            deleteCable(received_connection.LocalPortFrom, received_connection.VirtualPortFrom);
                            consoleWriter("Deleted connection: real port:" + received_connection.LocalPortFrom +
                                "virtual port:" + received_connection.VirtualPortFrom);
                        }
                        try
                        {
                            connectToNodes(received_connection.LocalPortFrom, received_connection.VirtualPortFrom,
                               received_connection.LocalPortTo, received_connection.VirtualPortTo);
                        }
                        catch (SocketException ex)
                        {
                            consoleWriter("Connection can't be made on port " + received_connection.LocalPortFrom);
                            return;
                        }
                    }
                    else
                    {
                        consoleWriter(ERROR_MSG + "received from window application wrong data format.");
                    }
                }
                catch (IOException ex)
                {
                    break;
                }
                Thread.Sleep(150);            
            }
        }

        public void connectToNodes(int fromPort, int virtualFromPort,
                                    int toPort, int virtualToPort) 
        {
            TcpClient connectionFrom = new TcpClient("localhost", fromPort);
            String connection1Name = +fromPort +
                          "(virtual:" + virtualFromPort + ")-->" + toPort +
                           "(virtual:" + virtualToPort + ")";
        //    consoleWriter("Initialize connection: " + connection1Name);
            NodeConnectionThread fromThread = new NodeConnectionThread(ref connectionFrom, 
                ref portToThreadMap, tableWithPorts, connection1Name);

            portToThreadMap.Add(fromPort + ":" + virtualFromPort, fromThread);

            TcpClient connectionTo = new TcpClient("localhost", toPort);
            try
            {
                connectionTo = new TcpClient("localhost", toPort);
            }
            catch (SocketException ex)
            {
                consoleWriter("Connection can't be made on port " + toPort);
                return;
            }
            String connection2Name = toPort +
                          "(virtual:" + virtualToPort + ")-->" + fromPort +
                           "(virtual:" + virtualFromPort + ")";
       //     consoleWriter("Initialize connection: " + connection2Name);
            NodeConnectionThread toThread = new NodeConnectionThread(ref connectionTo,
                ref portToThreadMap, tableWithPorts, connection2Name);

            portToThreadMap.Add(toPort + ":" + virtualToPort, toThread);

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
                    portToThreadMap.Remove(fromPort + ":" + virtualFromPort);
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
