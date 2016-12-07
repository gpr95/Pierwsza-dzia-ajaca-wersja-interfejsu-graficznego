using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientNode;
using ManagementApp;

namespace NetNode
{
    //switching field that have FIB and commutate frame from inport to outport
    class SwitchingField
    {
        public static List<FIB> fib = new List<FIB>();

        public int[] commuteContainer(VirtualContainer4 container)
        {
            int[] out_pos = { -1, -1 };
            if (container != null)
            {
                //mamy do czynienia z vc4
                foreach (var row in fib)
                {
                    if (row.in_cont == 1)
                    {
                        out_pos[0] = row.oport;
                        out_pos[1] = row.out_cont;
                        return out_pos;
                    }
                }
            }
            return out_pos;
        }
        public int[] commuteContainer(VirtualContainer3 container, int pos)
        {
            int[] out_pos = { -1, -1 };
            if (container != null)
            {
                //mamy do czynienia z vc3
                foreach (var row in fib)
                {
                    if (row.in_cont == pos)
                        {
                            out_pos[0] = row.oport;
                            out_pos[1] = row.out_cont;
                            return out_pos;
                        }
                }
            }
            return out_pos;
        }
        public static void addToSwitch(FIB row)
        {
            fib.Add(row);
        }
    }
}