using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClientNode;

namespace NetNode
{
    //output port
    class OPort
    {
        private int port;
        public Queue<Packet> output = new Queue<Packet>();

        public OPort(int port)
        {
            this.port = port;
        }

        public void addToOutQueue(ClientNode.Packet packet)
        {
            this.output.Enqueue(packet);
        }
    }
}
