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
        public int in_cont;
        public int oport;
        public int out_cont;
        public FIB(int in_cont, int oport, int out_cont)
        {
            this.in_cont = in_cont;
            this.oport = oport;
            this.out_cont = out_cont;
        }
    }
}
