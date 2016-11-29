using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNode
{
    
    class STM1
    {
        //RSOH
        //MSOH
        private string type;
        public VirtualContainer4 VC4;

    }

   



    class VirtualContainer3
    {
        // defines if container has low level containers 0 - NO, 1 - YES
        public int POH;
        public string sourceAddress { get; set; }
        public string message { get; set; }
        public int port { get; set; }
    }

    class VirtualContainer4 : VirtualContainer3
    {
        public List<VirtualContainer3> C4;

        public VirtualContainer4(int POH)
        {
            this.POH = POH;
        }
    }

    
}
