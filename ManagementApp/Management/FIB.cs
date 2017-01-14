using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Management
{
    //forwarding information table: destination mask oport
    public class FIB
    {
        //Nie ma to jak enkapsulacja
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
            return "In: " + iport + " Slot: " + in_cont + " Out: " + oport + " Slot: " + out_cont;
        }

        public FIB reverse()
        {
            return new FIB(this.oport, this.out_cont, this.iport, this.in_cont);
        }
    }
}