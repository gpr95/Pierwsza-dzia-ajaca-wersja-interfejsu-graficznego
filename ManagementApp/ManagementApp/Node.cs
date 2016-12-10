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
        protected int state { get; set; }
        protected int localPort;
        protected int ManagmentPort = 7777;
        protected String name;
        protected Point position;
        [NonSerialized]
        protected Thread threadHandle;
        [NonSerialized]
        protected TcpClient tcpClient;
        [NonSerialized]
        protected BinaryWriter socketWriter;
        [NonSerialized]
        protected Process processHandle;


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

        public Thread ThreadHandle
        {
            get
            {
                return threadHandle;
            }

            set
            {
                threadHandle = value;
            }
        }

        public TcpClient TcpClient
        {
            get
            {
                return tcpClient;
            }

            set
            {
                tcpClient = value;
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

        public BinaryWriter SocketWriter
        {
            get
            {
                return socketWriter;
            }

            set
            {
                socketWriter = value;
            }
        }
    }
}
