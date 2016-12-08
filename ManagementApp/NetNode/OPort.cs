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
        private STM1 currentFrame = new STM1();

        public OPort(int port)
        {
            this.port = port;
        }

        public void addToOutQueue(VirtualContainer4 container)
        {
            this.currentFrame.vc3List.Clear();
            this.currentFrame.vc4 = null;
            this.currentFrame.vc4 = container;
            this.output.Enqueue(this.currentFrame);
        }
        public void addToTempQueue(VirtualContainer3 container, int pos)
        {
            int i = 0;
            if(pos == 13)
            {
                i = 3;
            }
            else if(i == 12)
            {
                i = 2;
            }
            else{
                i = 1;
            }
            if (i != 0)
            {
                this.currentFrame.vc3List[i - 1] = container;
            }
        }
        public void addToOutQueue()
        {
            if (this.currentFrame.vc3List.Count != 0)
            {
                this.output.Enqueue(this.currentFrame);
            }
        }
    }
}
