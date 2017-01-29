using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ManagementApp;

namespace Management
{
    class ManagementPlane
    {
        // CONNECTIONS
        private AgentApplication agentApplication;
        private AgentNode agentNode;
        private AgentNCC agentNcc { get; set; }

        // CONSTS 
        private int APPLICATIONPORT = 7777;
        private int NODEPORT = 7778;
        private int NCCPORT = 7779;
        // LOGICAL VARIABLES
        private List<Node> nodeList = new List<Node>();
        private List<NodeConnection> connectionList = new List<NodeConnection>();
        private static ManagmentProtocol protocol = new ManagmentProtocol();

        public static Dictionary<string, string> conn = new Dictionary<string, string>();

        public ManagementPlane(string[] args)
        {
            int.TryParse(args[0], out this.APPLICATIONPORT);
            int.TryParse(args[1], out this.NODEPORT);
            int.TryParse(args[2], out this.NCCPORT);

            UserInterface.Management = this;

            // Connection to Application
            this.agentApplication = new AgentApplication(APPLICATIONPORT, this);
            // Listener for Nodes
            this.agentNode = new AgentNode(NODEPORT, nodeList, this);
            // Listener for NCC
            if (this.NCCPORT != 0)
                this.agentNcc = new AgentNCC(this.NCCPORT);
            else
                this.agentNcc = null;
            Thread.Sleep(100);
            Console.Title = "Management";
            if(APPLICATIONPORT != 7777)
                UserInterface.showDomain(APPLICATIONPORT);
            UserInterface.showMenu();
        }

        public void allocateNode(String nodeName, TcpClient nodePort, Thread nodeThreadHandle, BinaryWriter writer)
        {
            log("Node " + nodeName + " connected", ConsoleColor.Green);
            Node nodeBeingAllocated;
            if (nodeName.Contains("CN"))
                nodeBeingAllocated = new ClientNode(nodeName, 0);
            else
                nodeBeingAllocated = new NetNode(nodeName, 0);
            
            nodeBeingAllocated.ThreadHandle = nodeThreadHandle;
            nodeBeingAllocated.TcpClient = nodePort;
            nodeBeingAllocated.SocketWriter = writer;
            nodeList.Add(nodeBeingAllocated);
        }

        internal void getNodes(bool clientsOnly = false)
        {
            log("#DEBUG2", ConsoleColor.Magenta);
            UserInterface.nodeList(nodeList, clientsOnly);
        }

        public void getInterfaces(Node n)
        {
            log("#DEBUG4", ConsoleColor.Magenta);
            try
            {
                protocol.State = ManagmentProtocol.INTERFACEINFORMATION;
                string data = JSON.Serialize(JSON.FromValue(protocol));
                n.SocketWriter.Write(data);
            } catch (IOException e)
            {
                log("Error: " + e.Message, ConsoleColor.Red);
            }
        }

        public void log(String msg, ConsoleColor cc)
        {
            UserInterface.log(msg, cc);
        }

        public void stopRunning()
        {
            //run = false;
            //listener.Stop();
        }

        internal void removeNode(String clienttmp)
        {
            Node node = nodeList.Where(n => n.Name.Equals(clienttmp)).FirstOrDefault();
            if (node == default(Node))
                log("#DEBUG 5", ConsoleColor.Magenta);
            nodeList.Remove(node);
        }

        internal void sendEntry(Node n, string s)
        {
            String[] entry = s.Split('/');
            int[] val = new int[entry.Length];
            if(val.Length != 4)
            {
                log("Wrong entry.", ConsoleColor.Red);
                return;
            }
            for (int i = 0; i < entry.Length; i++)
            {
                int.TryParse(entry[i], out val[i]);
            }
            FIB f = new FIB(val[0], val[1], val[2], val[3]);
            try
            {
                protocol.State = ManagmentProtocol.ROUTINGENTRY;
                protocol.RoutingEntry = f;
                string data = JSON.Serialize(JSON.FromValue(protocol));
                n.SocketWriter.Write(data);
            }
            catch (IOException e)
            {
                log("Error: " + e.Message, ConsoleColor.Red);
            }
        }

        internal void sendTable(Node n, List<string> tableList)
        {
            List<FIB> listOfFibs = new List<FIB>();
            foreach(String s in tableList)
            {
                String[] entry = s.Split('/');
                int[] val = new int[entry.Length];
                if (val.Length != 4)
                {
                    log("Wrong entry.", ConsoleColor.Red);
                    return;
                }
                for (int i = 0; i < entry.Length; i++)
                {
                    int.TryParse(entry[i], out val[i]);
                }
                listOfFibs.Add(new FIB(val[0], val[1], val[2], val[3]));
            }

            try
            {
                protocol.State = ManagmentProtocol.ROUTINGTABLES;
                protocol.RoutingTable = listOfFibs;
                string data = JSON.Serialize(JSON.FromValue(protocol));
                n.SocketWriter.Write(data);
            }
            catch (IOException e)
            {
                log("Error: " + e.Message, ConsoleColor.Red);
            }
        }

        internal void sendShowTable(Node n)
        {
            try
            {
                Thread.Sleep(100);
                protocol.State = ManagmentProtocol.GETTABLE;
                string data = JSON.Serialize(JSON.FromValue(protocol));
                n.SocketWriter.Write(data);
            }
            catch (IOException e)
            {
                log("Error: " + e.Message, ConsoleColor.Red);
            }
        }

        internal void sendClear(Node n)
        {
            try
            {
                protocol.State = ManagmentProtocol.CLEARTABLE;
                string data = JSON.Serialize(JSON.FromValue(protocol));
                n.SocketWriter.Write(data);
            }
            catch (IOException e)
            {
                log("Error: " + e.Message, ConsoleColor.Red);
            }
        }

        internal void connectToOtherNcc(List<int> list)
        {
            agentNcc.sendInfoToOtherNcc(list);
        }

        internal void createSoft(string nodeStart, string end, int speed)
        {
            //Send info to NCC for creation soft pernament connecion;
            agentNcc.sendSoftPernament(nodeStart, end, speed);
        }

        internal void sendAllClients(List<string> list)
        {
            Dictionary<string, int> allClients = new Dictionary<string,int>();
            int i = 0;
            foreach(String s in list)
            {
                allClients.Add(s, i++);
                log(s, ConsoleColor.Cyan);
            }
            foreach(Node n in nodeList)
            {
                Address a = new Address(n.Name);
                if(a.type.Equals(192))
                {
                    ManagmentProtocol toSend = new ManagmentProtocol();
                    toSend.State = ManagmentProtocol.POSSIBLEDESITATIONS;
                    toSend.PossibleDestinations = allClients;
                    string data = JSON.Serialize(JSON.FromValue(toSend));
                    n.SocketWriter.Write(data);
                }
            }
        }

        public void getConnections()
        {
           foreach(var temp in conn)
           {
               Console.WriteLine("Connection id: "+temp.Key+ " to: "+temp.Value, ConsoleColor.Yellow);
           }
        }
    }
}
