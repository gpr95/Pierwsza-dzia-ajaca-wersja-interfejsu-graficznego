using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    public class CCtoNCCSingallingMessage
    {
        public const int INIT_FROM_CC = 0;
        // NCC - set me connection between nodeFrom and nodeTo with rate{1,2,3}
        public const int NCC_SET_CONNECTION = 1;
        // CC - connection setted
        public const int CC_CONFIRM = 2;
        // CC - unable to set
        public const int CC_REJECT = 3;
        // CC sending border node address to NCC
        public const int BORDER_NODE = 4;



        private int state;

        // state 1
        private String nodeFrom;
        private String nodeTo;
        private int rate;
        private int requestID;

        // state 4
        private String borderNode;
        private int borderDomain;

        // state 2
        private int vc11;
        private int vc12;
        private int vc13;

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

       

        public int Vc12
        {
            get
            {
                return vc12;
            }

            set
            {
                vc12 = value;
            }
        }

        public int Vc11
        {
            get
            {
                return vc11;
            }

            set
            {
                vc11 = value;
            }
        }

        public int Vc13
        {
            get
            {
                return vc13;
            }

            set
            {
                vc13 = value;
            }
        }

        public int RequestID
        {
            get
            {
                return requestID;
            }

            set
            {
                requestID = value;
            }
        }

        public string BorderNode
        {
            get
            {
                return borderNode;
            }

            set
            {
                borderNode = value;
            }
        }

        public int BorderDomain
        {
            get
            {
                return borderDomain;
            }

            set
            {
                borderDomain = value;
            }
        }
    }
}
