using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientNode;

namespace NetNode
{
    //switching field that have FIB and commutate packet from inport to outport
    class SwitchingField
    {
        private List<FIB> fib = new List<FIB>();
        public SwitchingField()
        {
            fib.Add(new FIB(0, "192.168.1.10", 0));
            fib.Add(new FIB(0, "192.168.1.11", 1));
            fib.Add(new FIB(0, "192.168.1.12", 2));
            fib.Add(new FIB(0, "192.168.56.1", 3));
            fib.Add(new FIB(1, "192.168.1.10", 0));
            fib.Add(new FIB(1, "192.168.1.11", 1));
            fib.Add(new FIB(1, "192.168.1.12", 2));
            fib.Add(new FIB(1, "192.168.1.13", 3));
            fib.Add(new FIB(2, "192.168.1.10", 0));
            fib.Add(new FIB(2, "192.168.1.11", 1));
            fib.Add(new FIB(2, "192.168.1.12", 2));
            fib.Add(new FIB(2, "192.168.1.13", 3));
            fib.Add(new FIB(3, "192.168.1.10", 0));
            fib.Add(new FIB(3, "192.168.1.11", 1));
            fib.Add(new FIB(3, "192.168.1.12", 2));
            fib.Add(new FIB(3, "192.168.1.13", 3));
        }

        public int commutePacket(ClientNode.Packet packet, int iport, string destination)
        {
            int oport = fib.Find(c => c.iport == iport && c.destination == destination).oport;
            Console.WriteLine("commutate packet from " + iport + " to output port " + oport);
            return oport;
        }
    }
}
