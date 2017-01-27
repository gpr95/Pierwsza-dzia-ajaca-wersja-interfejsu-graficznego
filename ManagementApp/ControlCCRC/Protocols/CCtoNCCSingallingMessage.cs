using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    class CCtoNCCSingallingMessage
    {
        public const int INIT_FROM_CC = 0;
        // NCC - set me connection between nodeFrom and nodeTo with rate{1,2,3}
        public const int NCC_SET_CONNECTION = 1;
        // CC - connection setted
        public const int CC_CONFIRM = 2;
        // CC - unable to set
        public const int CC_REJECT = 3;



        private int state;

        // state 1
        private String nodeFrom;
        private String nodeTo;
        private int rate;

        // state 2
        private int vc3;

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
    }
}
