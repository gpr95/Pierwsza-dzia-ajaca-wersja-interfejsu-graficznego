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

        // CONSTS 
        private int APPLICATIONPORT = 7777;
        private int MANAGMENTPORT = 7778;

        // LOGICAL VARIABLES
        private List<Node> nodeList = new List<Node>();
        private List<NodeConnection> connectionList = new List<NodeConnection>();
        private static ManagmentProtocol protocol = new ManagmentProtocol();

        public ManagementPlane(string[] args)
        {
            int.TryParse(args[0], out this.APPLICATIONPORT);
            int.TryParse(args[1], out this.MANAGMENTPORT);

            UserInterface.Management = this;

            // Connection to Application
            this.agentApplication = new AgentApplication(APPLICATIONPORT, this);
            // Listener for Nodes
            this.agentNode = new AgentNode(MANAGMENTPORT, nodeList, this);

            Thread.Sleep(100);

            UserInterface.showMenu();
        }

        public void allocateNode(String nodeName, TcpClient nodePort, Thread nodeThreadHandle, BinaryWriter writer)
        {
            log("Node " + nodeName + " connected", ConsoleColor.Blue);
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

        internal void getNodes()
        {
            log("#DEBUG2", ConsoleColor.Magenta);
            UserInterface.nodeList(nodeList);
        }

        public void getInterfaces(Node n)
        {
            log("#DEBUG4", ConsoleColor.Magenta);
            protocol.State = ManagmentProtocol.INTERFACEINFORMATION;
            string data = JSON.Serialize(JSON.FromValue(protocol));
            n.SocketWriter.Write(data);
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
    }
}
