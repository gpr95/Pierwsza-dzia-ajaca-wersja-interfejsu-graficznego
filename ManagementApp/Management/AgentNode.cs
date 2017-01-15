using ManagementApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Management
{
    class AgentNode
    {
        private ManagementPlane management;
        private bool run = true;
        private int managementPort;
        private Thread thread;
        private TcpListener listener;
        //private List<Node> nodeList;

        public AgentNode(int port, List<Node> nodeList, ManagementPlane management)
        {
            this.management = management;
            this.managementPort = port;
            //this.nodeList = nodeList;

            listener = new TcpListener(IPAddress.Any, managementPort);
            thread = new Thread(new ParameterizedThreadStart(Listen));
            thread.Start(management);
        }
        private class ThreadPasser
        {
            public ManagementPlane management;
            public TcpClient client;
        }

        private void Listen(Object managementlP)
        {
            listener.Start();
            ThreadPasser tp = new ThreadPasser();
            tp.management = (ManagementPlane)managementlP;

            while (run)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    tp.client = client;
                    Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
                    clientThread.Start(tp);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        private static void ListenThread(Object threadPasser)
        {
            ThreadPasser tp = (ThreadPasser) threadPasser;
            TcpClient clienttmp = tp.client;
            BinaryWriter writer = new BinaryWriter(clienttmp.GetStream());

            ManagmentProtocol toSend = new ManagmentProtocol();
            toSend.State = ManagmentProtocol.WHOIS;
            string data = JSON.Serialize(JSON.FromValue(toSend));
            writer.Write(data);

            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            string received_data = reader.ReadString();
            JSON received_object = JSON.Deserialize(received_data);
            ManagmentProtocol received_Protocol = received_object.Value.ToObject<ManagmentProtocol>();
            String nodeName = received_Protocol.Name;
            tp.management.allocateNode(nodeName, clienttmp, Thread.CurrentThread, writer);

            try
            {
                while (true)
                {
                    received_data = reader.ReadString();
                    received_object = JSON.Deserialize(received_data);
                    received_Protocol = received_object.Value.ToObject<ManagmentProtocol>();
                    if(received_Protocol.State == ManagmentProtocol.INTERFACEINFORMATION)
                        UserInterface.showInterfaces(received_Protocol.Interfaces);
                    if (received_Protocol.State == ManagmentProtocol.GETTABLE)
                        UserInterface.showTable(received_Protocol.RoutingTable);
                }
            }
            catch (SocketException e)
            {
                UserInterface.log("Error: " + e.Message, ConsoleColor.Red);
                tp.management.removeNode(nodeName);
            }
            catch (IOException e)
            {
                UserInterface.log("Error: " + e.Message, ConsoleColor.Red);
                tp.management.removeNode(nodeName);
            }
        }
    }
}
