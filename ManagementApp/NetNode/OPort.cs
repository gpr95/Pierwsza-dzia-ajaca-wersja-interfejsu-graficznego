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
            this.currentFrame.vc4 = container;
            this.output.Enqueue(currentFrame);
            this.currentFrame.vc4 = null;
        }
        public void addToTempQueue(VirtualContainer3 container, int pos)
        {
            //TODO pakownie w STM1
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
            if (this.currentFrame.vc3List[0] != null && this.currentFrame.vc3List[1] != null && this.currentFrame.vc3List[2] != null)
            {
                this.output.Enqueue(this.currentFrame);
                this.currentFrame.vc3List[0] = null;
                this.currentFrame.vc3List[1] = null;
                this.currentFrame.vc3List[2] = null;
            }
        }
    }
}
