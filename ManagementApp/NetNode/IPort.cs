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
        public Queue<STM1> input = new Queue<STM1>();

        public IPort(int port)
        {
            this.port = port;
        }

        public void addToInQueue(STM1 frame)
        {
            //TODO rozpakowac STM1 i zostawic tylko VC i je dorzucac pokolei do kolejki
            foreach(var container in frame)
            {
                input.Enqueue(container);
            }
            //input.Enqueue(frame);
        }
    }
}
