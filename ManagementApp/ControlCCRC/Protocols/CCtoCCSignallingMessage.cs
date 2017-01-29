using Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    public class CCtoCCSignallingMessage
    {
        // lower confirmed path set
        public const int CC_LOW_CONFIRM = 0;
        // lower reject path set
        public const int CC_LOW_REJECT = 1;
        // upper cc changing fibs in lower cc
        public const int CC_UP_FIB_CHANGE = 2;
        // middle cc
        public const int CC_MIDDLE_INIT = 3;
        // cc requests to build path between two nodes
        public const int CC_BUILD_PATH_REQUEST = 4;

        public const int FIB_SETTING_TOP_BOTTOM = 5;

        // release
        public const int REALEASE_TOP_BOTTOM = 5;


        private int state;
        // from last CC
        private bool lastCC;
        // sended FIB table
        private List<FIB> fib_table;
        // middle CC id
        private String identifier;

        // state 4
        private String nodeFrom;
        private String nodeTo;
        private int rate;
        private int requestId;

        private int vc1 =1;
        private int vc2 =1;
        private int vc3 =1;

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

        public string Identifier
        {
            get
            {
                return identifier;
            }

            set
            {
                identifier = value;
            }
        }

        public string NodeFrom
        {
            get
            {
                return nodeFrom;
            }

            set
            {
                nodeFrom = value;
            }
        }

        public string NodeTo
        {
            get
            {
                return nodeTo;
            }

            set
            {
                nodeTo = value;
            }
        }

        public int Rate
        {
            get
            {
                return rate;
            }

            set
            {
                rate = value;
            }
        }

        public int Vc1
        {
            get
            {
                return vc1;
            }

            set
            {
                vc1 = value;
            }
        }

        public int Vc2
        {
            get
            {
                return vc2;
            }

            set
            {
                vc2 = value;
            }
        }

        public int Vc3
        {
            get
            {
                return vc3;
            }

            set
            {
                vc3 = value;
            }
        }

        public int RequestId
        {
            get
            {
                return requestId;
            }

            set
            {
                requestId = value;
            }
        }
    }
}
