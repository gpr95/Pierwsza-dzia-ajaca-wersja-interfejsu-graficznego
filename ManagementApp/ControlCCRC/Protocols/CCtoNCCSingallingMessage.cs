using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    class CCtoNCCSingallingMessage
    {
        // NCC - set me connection between nodeFrom and nodeTo with rate{1,2,3}
        public const int NCC_SET_CONNECTION = 0;
        // CC - connection setted
        public const int CC_CONFIRM = 1;
        // CC - unable to set
        public const int CC_REJECT = 2;


        private int state;
        private String nodeFrom;
        private String nodeTo;
        private int rate;

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
