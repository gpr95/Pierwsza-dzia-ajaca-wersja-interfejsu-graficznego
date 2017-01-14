using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientWindow
{
    public class Signal
    {
        
        public int port;
        public STM1 stm1;
        public string lrmProtocol;

        public Signal()
        {
            this.port = 0;
            this.stm1 = null;
            this.lrmProtocol = null;
        }

        public Signal(int port, STM1 stm1)
        {
            this.port = port;
            this.stm1 = new STM1(stm1.vc4.POH, stm1.vc4.C4, stm1.vc4.vc3List);
        }
        public Signal(int port, string lrmProtocol)
        {
            this.port = port;
            this.lrmProtocol = lrmProtocol;
        }
    }
}
