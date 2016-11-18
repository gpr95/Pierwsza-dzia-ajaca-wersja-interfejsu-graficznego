using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetNode
{
    //forwarding information table: iport destination oport
    class FIB
    {
        public int iport, oport;
        public string destination;
        public FIB(int iport, string destination, int oport)
        {
            this.iport = iport;
            this.destination = destination;
            this.oport = oport;
        }
    }
}
