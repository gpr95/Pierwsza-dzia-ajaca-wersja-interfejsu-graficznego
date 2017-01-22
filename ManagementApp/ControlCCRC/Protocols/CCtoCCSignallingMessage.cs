using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    class CCtoCCSignallingMessage
    {
        private bool lastCC;

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
    }
}
