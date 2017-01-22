using Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    class CCtoCCSignallingMessage
    {
        // nothing
        public const int CC_LOW_INIT = 0;
        // lower confirmed path set
        public const int CC_LOW_CONFIRM = 1;
        // lower reject path set
        public const int CC_LOW_REJECT = 2;
        // upper cc changing fibs in lower cc
        public const int CC_UP_FIB_CHANGE = 3;


        private int state;
        // from last CC
        private bool lastCC;
        // sended FIB table
        private List<FIB> fib_table;

        public bool LastCC
        {
            get
            {
                return lastCC;
            }

            set
            {
                lastCC = value;
            }
        }

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

        public List<FIB> Fib_table
        {
            get
            {
                return fib_table;
            }

            set
            {
                fib_table = value;
            }
        }
    }
}
