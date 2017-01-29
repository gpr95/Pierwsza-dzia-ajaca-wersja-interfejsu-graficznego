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
    class NodeConnectionThread
    {
        private const string ERROR_MSG = "ERROR: ";
        private const ConsoleColor ERROR_COLOR = ConsoleColor.Red;
        private const ConsoleColor ADMIN_COLOR = ConsoleColor.Green;
        private const ConsoleColor INFO_COLOR = ConsoleColor.Blue;

        private Thread thread;
        private TcpClient connection;
        private DataTable table;
        private Dictionary<String, NodeConnectionThread> portToThreadMap;
        private BinaryWriter writer;
        private BinaryReader reader;
        private int fromPort;
        private int virtualFromPort;
        private int toPort;
        private int virtualToPort;
        private String name; 

        public NodeConnectionThread(ref TcpClient connection,
            ref Dictionary<String, NodeConnectionThread> portToThreadMap, DataTable table, String name, int fromPort, int virtualFromPort, int toPort, int virtualToPort)
        {
            this.connection = connection;
            this.portToThreadMap = portToThreadMap;
            this.table = table;
            this.name = name;
            this.fromPort = fromPort;
            this.virtualFromPort = virtualFromPort;
            this.toPort = toPort;
            this.virtualToPort = virtualToPort;

            thread = new Thread(nodeConnectionThread);
            thread.Start();
        }

        private void nodeConnectionThread()
        {
            consoleWriter("Initialize connection: " + name);
            writer = new BinaryWriter(connection.GetStream());
            reader = new BinaryReader(connection.GetStream());

            /** Add new cable to table */
            addNewCable(fromPort, virtualFromPort, toPort, virtualToPort);
            portToThreadMap.Add(fromPort + ":" + virtualFromPort, this);
            bool noError = true;
            while (noError)
            {
                string received_data = null;
                try
                {
                    received_data = reader.ReadString();
                }
                catch(IOException ex)
                {
                    consoleWriter("ERROR: Connection LOST: " + name);
                    deleteCable(fromPort, virtualFromPort);
                    consoleWriter("CONNECTION REMOVED FROM TABLE " + name);
                    return;
                }
                if (received_data == null || received_data.Length == 0)
                    continue;

                Signal signal = null;
                JSON received_object = null;
                try
                {
                    received_object = JSON.Deserialize(received_data);
                }
                catch (Exception e)
                {
                    continue;
                }
                if (received_object.Type == typeof(Signal))
                {
                    signal = received_object.Value.ToObject<Signal>();  

                    fromPort = ((IPEndPoint)connection.Client.RemoteEndPoint).Port;
                    virtualFromPort = signal.port;
                    
                    toPort = 0;
                    virtualToPort = 0;

                    for (int i = table.Rows.Count - 1; i >= 0; i--)
                    {
                        DataRow dr = table.Rows[i];
                        if (dr["fromPort"].Equals(fromPort) && dr["virtualFromPort"].Equals(virtualFromPort))
                        {
                            toPort = (int)dr["toPort"];
                            virtualToPort = (int)dr["virtualToPort"];
                            if(portToThreadMap.ContainsKey(toPort + ":" + virtualToPort))
                                consoleWriterWithKeepAliveChecking("Connection: " + name + " received data.", signal);
                        }
                    }
                    signal.port = virtualToPort;
                    if (portToThreadMap.ContainsKey(toPort + ":" + virtualToPort))
                        consoleWriterWithKeepAliveChecking("Connection: " + name + " sending data.", signal);
                    try
                    {
                        portToThreadMap[toPort + ":" + virtualToPort].sendSignal(signal, toPort);
                    }
                    catch(KeyNotFoundException ex)
                    {
                        noError = false;
                    }
                }
                else
                {
                    consoleWriterWithKeepAliveChecking(ERROR_MSG + "received from node wrong data format. Node PORT: " + ((IPEndPoint)connection.Client.RemoteEndPoint).Port, signal);
                }
            }
        }

        public void sendSignal(Signal toSend, int port)
        {
            String data = JSON.Serialize(JSON.FromValue(toSend));
            try
            {
                writer.Write(data);
            }
            catch(IOException ex)
            {
                consoleWriterWithKeepAliveChecking(ERROR_MSG + "Trying to send data failed", toSend);
                deleteCable(fromPort, virtualFromPort);
            }
        }
        private void addNewCable(int fromPort, int virtualFromPort, int toPort, int virtualToPort)
        {
            try
            {
                table.Rows.Add(fromPort, virtualFromPort, toPort, virtualToPort);
                consoleWriter("Made connection: from-" + fromPort + "(" + virtualFromPort + ")" + " to-" +
                                  toPort + "(" + virtualToPort + ")");
            }
            catch(ArgumentException ex)
            {
                consoleWriter("Connection already existed in table.");
            }
        }

        private void deleteCable(int fromPort, int virtualFromPort)
        {
            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                DataRow drFrom = table.Rows[i];
                if (drFrom["fromPort"].Equals(fromPort) && drFrom["virtualFromPort"].Equals(virtualFromPort))
                {
                    try
                    {
                        table.Rows.Remove(drFrom);
                    }
                    catch(RowNotInTableException ex)
                    {
                        consoleWriter("Element already deleted from table.");
                    }
                    portToThreadMap.Remove(fromPort + ":" + virtualFromPort);
                }
            }
        }
        private void consoleWriter(String msg)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:" + msg);
        }

        private void consoleWriterWithKeepAliveChecking(String msg,Signal sig)
        {
            if(sig.stm1 != null)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:" + msg);
            }
        }

    }
}
