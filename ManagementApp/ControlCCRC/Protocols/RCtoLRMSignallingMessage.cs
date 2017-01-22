using Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    class RCtoLRMSignallingMessage
    {
        public const int SENDTOPOLOGY = 0;
        public const int SENDDELETED = 1;
        public const int SENDCONFIRMATION = 2;
        public const int ALLOCATERES = 3;
        public const int INSERTFIB = 4;

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
