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

        public int commuteFrame(STM1 frame, string destination)
        {
            int oport = -1;
            foreach (var row in fib)
            {
                string dest = row.destination;
                //TODO check subnetworks
                string[] ipTableBytes = dest.Split('.');
                string[] ipDestBytes = destination.Split('.');
                if(row.mask < 32)
                {
                    if(row.mask == 8 || row.mask == 16 || row.mask == 24)
                    {
                        for(int i=0;i<row.mask/8;i++)
                        {
                            if(ipDestBytes[i] == ipTableBytes[i])
                            {
                                oport = row.oport;
                                Console.WriteLine("commutate frame to output port " + oport);
                                return oport;
                            }
                        }
                    }
                }
                if (dest == destination)
                {
                    oport = row.oport;
                    Console.WriteLine("commutate frame to output port " + oport);
                    return oport;
                }
            }
            return oport;
        }
    }
}