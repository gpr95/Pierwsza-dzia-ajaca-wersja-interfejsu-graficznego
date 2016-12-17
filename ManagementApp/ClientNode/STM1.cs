using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientWindow
{
    
    public class STM1
    {
        //RSOH
        //MSOH
        public VirtualContainer4 vc4;
       // public VirtualContainer3[] vc3List = new VirtualContainer3[3];//sa trzy kontenery vc3 a nie dwa
       // public Dictionary<int, VirtualContainer3> vc3List = new Dictionary<int, VirtualContainer3>();

        public STM1()
        {
            //stub
        }
        public STM1(int POH, string C4)
        {
            this.vc4 = new VirtualContainer4(POH, C4);
        }
        public STM1(Dictionary<int, VirtualContainer3> vc3List)
        {
            this.vc4 = new VirtualContainer4(vc3List);
            
        }

        public STM1(int POH, string C4, Dictionary<int, VirtualContainer3> vc3List)
        {
            this.vc4 = new VirtualContainer4(POH, C4);
            this.vc4.vc3List = vc3List;
        }


    }

   



    public class VirtualContainer3
    {
        public int POH;
        public string C3;

        public VirtualContainer3()
        {
            //asdad
        }
        public VirtualContainer3(int POH, string C3)
        {
            this.POH = POH;
            this.C3 = C3;
        }
    }

    public class VirtualContainer4
    {
        public int POH;
        public string C4;
        public Dictionary<int, VirtualContainer3> vc3List = new Dictionary<int, VirtualContainer3>();

        public VirtualContainer4()
        {
           //asd
        }
        public VirtualContainer4(int POH, string C4)
        {
            this.POH = POH;
            this.C4 = C4;
        }

        public VirtualContainer4(Dictionary<int, VirtualContainer3> vc3List)
        {
            this.vc3List = vc3List;
        }
    }

    
}
