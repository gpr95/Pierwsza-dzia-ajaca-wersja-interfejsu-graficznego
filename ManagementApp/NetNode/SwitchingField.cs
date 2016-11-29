using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientNode;

namespace NetNode
{
    //switching field that have FIB and commutate frame from inport to outport
    class SwitchingField
    {
        private List<FIB> fib = new List<FIB>();

        public SwitchingField()
        {
            this.fib.Add(new FIB("192.168.1.0", 24, 0));
            this.fib.Add(new FIB("192.168.2.0", 24, 0));
            this.fib.Add(new FIB("192.168.3.0", 24, 0));
            this.fib.Add(new FIB("192.169.0.0", 16, 0));

        }

        public int commuteFrame(ClientNode.Frame frame, string destination)
        {
            int oport;
            foreach (var row in fib)
            {
                string dest = row.destination;
                //TODO check subnetworks
                if (dest == destination)
                {
                    oport = row.mask;
                    Console.WriteLine("commutate frame to output port " + oport);
                    return oport;
                }
            }
            return -1;
        }
    }
}
