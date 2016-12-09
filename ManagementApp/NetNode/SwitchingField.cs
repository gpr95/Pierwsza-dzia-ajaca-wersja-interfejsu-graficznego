﻿using System;
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

        public int commuteContainer(VirtualContainer4 container, int iport)
        {
            int out_pos = 1;
            if (container != null)
            {
                //mamy do czynienia z vc4
                foreach (var row in fib)
                {
                    if (row.iport == iport && row.in_cont == 1)
                    {
                        out_pos = row.oport;
                        Console.WriteLine("Commuting container from:" + row.iport + " to " + row.oport);
                        return out_pos;
                    }
                }
            }
            return out_pos;
        }
        public int[] commuteContainer(VirtualContainer3 container, int iport, int pos)
        {
            int[] out_pos = { -1, -1 };
            if (container != null)
            {
                //mamy do czynienia z vc3
                foreach (var row in fib)
                {
                    if (row.iport == iport && row.in_cont == pos)
                        {
                            out_pos[0] = row.oport;
                            out_pos[1] = row.out_cont;
                            Console.WriteLine("Commuting container from:" + row.iport + " "+ row.in_cont + "to " + row.oport + " " + row.oport);
                            return out_pos;
                        }
                }
            }
            return out_pos;
        }
        public static void addToSwitch(FIB row)
        {
            fib.Add(row);
            Console.WriteLine("New fib row added");
            foreach(var temp in fib)
            {
                Console.WriteLine(temp.iport + " " + temp.in_cont + " " + temp.oport + " " + temp.out_cont);
            }
        }
    }
}