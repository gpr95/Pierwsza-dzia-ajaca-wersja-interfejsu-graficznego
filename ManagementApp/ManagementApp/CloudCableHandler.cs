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
        private List<NodeConnection> allConnections;
        private TcpClient clientCableCloud;
        private BinaryWriter writerCableCloud;
        private BinaryReader listenerCableCloud;
        private TcpListener listenerForCableCloudConnection;
        private Thread threadCloudCableHandler;

        public CloudCableHandler(List<NodeConnection> connections, int cloudPort)
        {
            this.allConnections = connections;
            listenerForCableCloudConnection = new TcpListener(IPAddress.Parse("127.0.0.1"), cloudPort);
            threadCloudCableHandler = new Thread(new ThreadStart(listenForCloud));
            threadCloudCableHandler.Start();
            String parameters = "" + cloudPort;
            System.Diagnostics.Process.Start("CableCloud.exe", parameters);
        }

        private void listenForCloud()
        {
            listenerForCableCloudConnection.Start();

            clientCableCloud = listenerForCableCloudConnection.AcceptTcpClient();
            writerCableCloud = new BinaryWriter(clientCableCloud.GetStream());
            listenerCableCloud = new BinaryReader(clientCableCloud.GetStream());
        }

        public void updateConnections(List<NodeConnection> connections)
        {
            String data = JSON.Serialize(JSON.FromValue(connections.Select(n => n.Prop).ToList()));
            try
            {
                writerCableCloud.Write(data);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            //connectionList.Add(new NodeConnection(from, portFrom, to, portTo, from.Name + "-" + to.Name));
            //for (int i = 0; i < connections.Count; i++)
            //{
            //    String data = JSON.Serialize(JSON.FromValue(connections[i].Prop));
            //    try
            //    {
            //        writerCableCloud.Write(data);
            //    }
            //    catch (SocketException e)
            //    {
            //        Console.WriteLine(e.StackTrace);
            //    }
            //}
        }
        public void updateOneConnection()
        {
            String data = JSON.Serialize(JSON.FromValue(allConnections.Last().Prop));
            try
            {
                writerCableCloud.Write(data);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            Console.WriteLine("Conn send" + DateTime.Now.ToLongTimeString());
        }

        public void deleteConnection(NodeConnection con)
        {
            ConnectionProperties conProp = con.Prop;
            int tempVPortTo = conProp.VirtualPortTo;
            int tempLPortTo = conProp.LocalPortTo;
            conProp.VirtualPortTo = 0;
            conProp.LocalPortTo = 0;
            String data = JSON.Serialize(JSON.FromValue(conProp));
            try
            {
                writerCableCloud.Write(data);
                conProp = con.Prop;
                conProp.VirtualPortFrom = tempVPortTo;
                conProp.LocalPortFrom = tempLPortTo;
                data = JSON.Serialize(JSON.FromValue(conProp));
                writerCableCloud.Write(data);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            Console.WriteLine("Conn send" + DateTime.Now.ToLongTimeString());
        }

        public void stopRunning()
        {
            threadCloudCableHandler.Interrupt();
        }
    }
}