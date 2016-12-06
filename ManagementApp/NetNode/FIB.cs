using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetNode
{
    class FIB
    {
        public int iport;
        public int in_cont;
        public int oport;
        public int out_cont;
        public FIB(int iport, int in_cont, int oport, int out_cont)
        {
            this.iport = iport;
            this.in_cont = in_cont;
            this.oport = oport;
            this.out_cont = out_cont;
        }

        public String toString()
        {
            return "In: " + iport + " Szcz: " + in_cont + " Out: " + oport + " Szcz: " + out_cont;
        }
    }
}
