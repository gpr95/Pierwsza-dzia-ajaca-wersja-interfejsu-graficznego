using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNode
{
    
    class STM
    {
        private string type;

    }

    class STM1 :STM
    {
        public VirtualContainer4 vc4;
    }
    class STM2 : STM
    {
        public VirtualContainer44c vc44c;
    }



    class VirtualContainer4 
    {
        // defines if container has low level containers 0 - NO, 1 - YES
        public int POH;
        public string sourceAddress { get; set; }
        public string message { get; set; }
        public int port { get; set; }
    }

    class VirtualContainer44c : VirtualContainer4
    {
        public List<VirtualContainer4> C44c;

        public VirtualContainer44c(int POH)
        {
            this.POH = POH;
        }
    }

    
}
