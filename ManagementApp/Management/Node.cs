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

namespace Management
{
    public class Node
    {
        protected int ManagmentPort = 7777;
        protected int state;
        protected int localPort;
        protected String name;
        protected Thread threadHandle;
        protected TcpClient tcpClient;
        protected BinaryWriter socketWriter;
        protected Process processHandle;

        public int State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
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
    }
}
