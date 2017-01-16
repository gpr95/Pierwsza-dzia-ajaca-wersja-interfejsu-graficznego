using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNode
{
    public class ControlPacket
    {
        public string virtualInterface;
        public int FLAG = 0;
        public string resourceIdentifier;
        public int virtualPort;
        public int slot;

        public ControlPacket(string virtualInterface, int FLAG, string resourceIdentifier)
        {
            this.virtualInterface = virtualInterface;
            this.FLAG = FLAG;
            this.resourceIdentifier = resourceIdentifier;
        }

       

    }
}
