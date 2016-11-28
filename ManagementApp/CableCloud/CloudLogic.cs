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
using System.Linq;

namespace CableCloud
{
    class CloudLogic
    {
        private DataTable table;
        public CloudLogic()
        {
            table = new DataTable("Connections");
            table.Columns.Add("fromAdr", typeof(String)).AllowDBNull = false;
            table.Columns.Add("fromPort", typeof(int)).AllowDBNull = false;
            table.Columns.Add("toAdr", typeof(String)).AllowDBNull = false;
            table.Columns.Add("toPort", typeof(int)).AllowDBNull = false;
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
            string received_data = reader.ReadString();
            JMessage received_object = JMessage.Deserialize(received_data);
            if (received_object.Type == typeof(NodeConnection))
            {
                NodeConnection received_connection = received_object.Value.ToObject<NodeConnection>();
                connectToNodes(received_connection.From.Name, received_connection.VirtualPortFrom,
                    received_connection.LocalPortFrom, received_connection.To.Name, 
                    received_connection.VirtualPortTo, received_connection.LocalPortTo);
            }
            else
            {
                Console.WriteLine("\n Connection with Window Application: WRONG DATA");
            }

            reader.Close();
        }

        public void connectToNodes(String virtualIPFrom, int virtualPortFrom, int realPortFrom,
                                    String virtualIPTo, int virtualPortTo, int realPortTo)
        {
            TcpClient connectionFrom = new TcpClient("localhost", realPortFrom);
            NodeConnectionThread fromArg = new NodeConnectionThread(ref connectionFrom, virtualIPFrom, virtualPortFrom);

            TcpClient connectionTo = new TcpClient("localhost", realPortTo);
            NodeConnectionThread toArg = new NodeConnectionThread(ref connectionFrom, virtualIPFrom, virtualPortFrom);

            addNewCable(virtualIPFrom, virtualPortFrom, virtualIPTo, virtualPortTo);
        }

        

        private void  addNewCable(String fromAdr, int fromPort, String toAdr, int toPort)
        {
            table.Rows.Add(fromAdr, fromPort, toAdr, toPort);
        }

        private void deleteCable(String fromAdr, String fromPort, String toAdr, String toPort)
        {
            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = table.Rows[i];
                if (dr["fromAdr"].Equals(fromAdr) && dr["fromPort"].Equals(fromPort)
                    && dr["toAdr"].Equals(toAdr) && dr["toPort"].Equals(toPort))
                    table.Rows.Remove(dr);
            }
        }

        private class NodeConnectionThread
        {
            private Thread thread;
            private String virtualIp;
            private int virtualPort;

            public NodeConnectionThread(ref TcpClient connection, String virtualIp, int virtualPort)
            {
                this.virtualIp = virtualIp;
                this.virtualPort = virtualPort;
                thread = new Thread(new ParameterizedThreadStart(nodeConnectionThread));
                thread.Start(connection);
            }

            private void nodeConnectionThread(Object connection)
            {
                TcpClient clienttmp = (TcpClient)connection;
                BinaryReader reader = new BinaryReader(clienttmp.GetStream());
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                // TODO Poki co string , potem bedzie tu klasa kontenera , przekazywanie dalej wiadomosci
                if (received_object.Type == typeof(String))
                {
                    String received_message = received_object.Value.ToObject<String>();

                }
                else
                {
                    Console.WriteLine("\n Connection with Node: WRONG DATA");
                }

                reader.Close();
            }
        
        }
    }
}
