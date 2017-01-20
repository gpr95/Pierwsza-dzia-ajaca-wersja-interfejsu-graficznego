using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Management;

namespace ControlCCRC
{
    public class ControlProtocol
    {
        public static readonly int SENDTOPOLOGY = 0;
        public static readonly int SENDDELETED = 1;
        public static readonly int SENDCONFIRMATION = 2;
        public static readonly int ALLOCATERES = 3;
        public static readonly int INSERTFIB = 4;

        public int state;
        public string topology;
        public string topologyDeleted;
        public string allocationConf;
        public string allocateNo;
        public FIB fib;

        public int State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
            }
        }
    }
}
