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
        public Queue<Frame> output = new Queue<Frame>();

        public OPort(int port)
        {
            this.port = port;
        }

        public void addToOutQueue(ClientNode.Frame frame)
        {
            this.output.Enqueue(frame);
        }
    }
}
