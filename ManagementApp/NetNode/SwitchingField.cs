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
        public List<FIB> fib = new List<FIB>();

        public SwitchingField()
        {
            this.fib.Add(new FIB(1, 0, 1));
            this.fib.Add(new FIB(1.1, 1, 1.2));
            this.fib.Add(new FIB(1.2, 1, 1.3));
            this.fib.Add(new FIB(1.3, 1, 1.1));

        }

        public int commuteContainer(VC container)
        {
            foreach (var row in fib)
            {
                int oport = row.oport;
                double out_cont = row.out_cont;

                if(container.cont_no == row.in_cont)
                {
                    Console.WriteLine("commutate frame to output port " + oport);
                    container.cont_no = out_cont;
                    return oport;
                }
            }
            return -1;
        }
    }
}