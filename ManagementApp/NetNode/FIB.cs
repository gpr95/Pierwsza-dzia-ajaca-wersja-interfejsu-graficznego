using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetNode
{
    //forwarding information table: destination mask oport
    class FIB
    {
        public int oport;
        public string destination;
        public int mask;
        public FIB(string destination, int mask, int oport)
        {
            this.destination = destination;
            this.mask = mask;
            this.oport = oport;
        }
    }
}
