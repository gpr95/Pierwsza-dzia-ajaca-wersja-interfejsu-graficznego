using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ManagementApp
{
    [Serializable()]
    public class Node
    {
        protected int ManagmentPort = 7778;
        private int state { get; set; }
        private int localPort;
        private String name;
        private Point position;
        private NodeType nodeType;
        private Process processHandle;
        private int subnetwork;

        public Node(Point point, NodeType n, String name, int localPort, int mPort, int cPort)
        {
            nodeType = n;
            if (n.Equals(NodeType.CLIENT))
            {
                this.Name = name;
                this.LocalPort = localPort;
                this.Position = point;
                this.ManagmentPort = mPort;
                this.ControlPort = cPort;
                String parameters = name + " " + this.LocalPort + " " + this.ManagmentPort;
                ProcessStartInfo startInfo = new ProcessStartInfo("ClientNode.exe");
                startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                startInfo.Arguments = parameters;

                this.ProcessHandle = Process.Start(startInfo);
            }
            else if (n.Equals(NodeType.NETWORK))
            {
                this.Name = name;
                this.LocalPort = localPort;
                this.Position = point;

                String parameters = name + " " + this.LocalPort + " " + this.ManagmentPort + " " + this.ControlPort;
                ProcessStartInfo startInfo = new ProcessStartInfo("NetNode.exe");
                startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                startInfo.Arguments = parameters;

                this.ProcessHandle = Process.Start(startInfo);
            }
        }

        public Node(Node n) : this(n.Position, n.Type, n.Name, n.LocalPort, n.ManagmentPort, n.ControlPort)
        {
            
        }

        public enum NodeType
        {
            CLIENT,
            NETWORK
        }

        public Point Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public int LocalPort
        {
            get
            {
                return localPort;
            }

            set
            {
                localPort = value;
            }
        }

        public NodeType Type
        {
            get
            {
                return nodeType;
            }

            set
            {
                nodeType = value;
            }
        }

        public Process ProcessHandle
        {
            get
            {
                return processHandle;
            }

            set
            {
                processHandle = value;
            }
        }

        public int ControlPort { get; private set; }
    }
}
