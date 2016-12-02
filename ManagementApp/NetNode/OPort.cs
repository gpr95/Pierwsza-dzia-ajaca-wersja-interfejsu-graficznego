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
        public Queue<STM1> output = new Queue<STM1>();

        public OPort(int port)
        {
            this.port = port;
        }

        public void addToOutQueue(VC container)
        {
            //TODO pakownie w STM1
            STM1 frame = new STM1();
            this.output.Enqueue(frame);
        }
    }
}
