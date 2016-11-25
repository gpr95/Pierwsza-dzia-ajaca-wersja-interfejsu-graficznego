using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientNode;

namespace NetNode
{
    //input port
    class IPort
    {
        public int port;
        public Queue<Packet> input = new Queue<Packet>();
        private TcpListener listener;

        public IPort(int port)
        {
            this.port = port;
        }

        public void addToInQueue(ClientNode.Packet packet)
        {
            input.Enqueue(packet);
        }
    }
}
