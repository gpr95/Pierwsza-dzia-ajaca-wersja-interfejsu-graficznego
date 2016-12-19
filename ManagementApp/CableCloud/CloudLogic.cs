using ClientWindow;
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
        private const ConsoleColor ERROR_COLOR = ConsoleColor.Red;
        private const ConsoleColor ADMIN_COLOR = ConsoleColor.Green;
        private const ConsoleColor INFO_COLOR = ConsoleColor.Blue;

        /** TABLE WITH CONNECTION */
        private DataTable tableWithPorts;

        /** HANDLERS MAP - localPORT-Thread with connection to this port */
        private Dictionary<String, NodeConnectionThread> portToThreadMap;

        /** Avilable port list */
        List<int> portList;
        public CloudLogic()
        {
            tableWithPorts = new DataTable("Connections");
            portToThreadMap = new Dictionary<String, NodeConnectionThread>();
            tableWithPorts.Columns.Add("fromPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("virtualFromPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("toPort", typeof(int)).AllowDBNull = false;
            tableWithPorts.Columns.Add("virtualToPort", typeof(int)).AllowDBNull = false;
            portList = new List<int>();
            for (int i = 9000; i < 9300; i++)
                portList.Add(i);
            /** LOGS CONSOLE  */
            consoleWriter("Cloud start", ADMIN_COLOR);
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
                consoleWriter("ERROR: Cannot connect with window application.",ERROR_COLOR);
            }
            Thread clientThread = new Thread(new ParameterizedThreadStart(windowConnectionThread));
            clientThread.Start(connection);
        }

        private void windowConnectionThread(Object connection)
        {
            consoleWriter("Connected with window application",ADMIN_COLOR);
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
                                "virtual port:" + received_connection.VirtualPortFrom,INFO_COLOR);
                            continue;
                        }
                        try
                        {
                            connectToNodes(received_connection.LocalPortFrom, received_connection.VirtualPortFrom,
                               received_connection.LocalPortTo, received_connection.VirtualPortTo);
                        }
                        catch (SocketException ex)
                        {
                            consoleWriter("Connection can't be made on port " + received_connection.LocalPortFrom,ERROR_COLOR);
                            return;
                        }
                    }
                    else
                    {
                        consoleWriter(ERROR_MSG + "received from window application wrong data format.",ERROR_COLOR);
                    }
                }
                catch (IOException ex)
                {
                    break;
                }          
            }
        }

        public void connectToNodes(int fromPort, int virtualFromPort,
                                    int toPort, int virtualToPort) 
        {
            if (!portToThreadMap.ContainsKey(fromPort + ":" + virtualFromPort))
            {
                TcpClient connectionFrom = null;
                int minPort = portList.Min();
                try
                {
                    IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                    IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, minPort);
                    connectionFrom = new TcpClient(ipLocalEndPoint);
                    connectionFrom.Connect("localhost", fromPort);
                    portList.Remove(minPort);
                }
                catch (SocketException ex)
                {
                    consoleWriter("Connection can't be made on port " + toPort, ERROR_COLOR);
                    return;
                }
                String connection1Name = +fromPort +
                              "(virtual:" + virtualFromPort + ")-->" + toPort +
                               "(virtual:" + virtualToPort + ")";
                NodeConnectionThread fromThread = new NodeConnectionThread(ref connectionFrom,
                    ref portToThreadMap, tableWithPorts, connection1Name, fromPort, virtualFromPort,
                   toPort, virtualToPort, minPort, ref portList);

            }
            if (!portToThreadMap.ContainsKey(toPort + ":" + virtualToPort))
            {
                TcpClient connectionTo = null;
                int minPort = portList.Min();
                try
                {
                    IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                    IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, minPort);
                    connectionTo = new TcpClient(ipLocalEndPoint);
                    connectionTo.Connect("localhost", toPort);
                    portList.Remove(minPort);
                }
                catch (SocketException ex)
                {
                    consoleWriter("Connection can't be made on port " + toPort, ERROR_COLOR);
                    return;
                }
                String connection2Name = toPort +
                              "(virtual:" + virtualToPort + ")-->" + fromPort +
                               "(virtual:" + virtualFromPort + ")";
                NodeConnectionThread toThread = new NodeConnectionThread(ref connectionTo,
                    ref portToThreadMap, tableWithPorts, connection2Name, toPort, virtualToPort, fromPort, virtualFromPort, minPort, ref portList);
            }
        }


        private void deleteCable(int fromPort, int virtualFromPort)
        {
            int toPort = 0;
            int virtualToPort = 0;
            for (int i = tableWithPorts.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = tableWithPorts.Rows[i];
                if (dr["fromPort"].Equals(fromPort) && dr["virtualFromPort"].Equals(virtualFromPort))
                {
                    toPort = (int)dr["toPort"];
                    virtualToPort = (int)dr["virtualToPort"];
                    tableWithPorts.Rows.Remove(dr);
                    portList.Add(portToThreadMap[fromPort + ":" + virtualFromPort].tcpClientPort);
                    portToThreadMap[fromPort + ":" + virtualFromPort].executeConnection();
                    portToThreadMap.Remove(fromPort + ":" + virtualFromPort);
                }
            }
        }
        private void consoleWriter(String msg, ConsoleColor cc)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:" + msg);
            Console.Write(Environment.NewLine);
        }
    }
}
