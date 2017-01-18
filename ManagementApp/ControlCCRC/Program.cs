using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC
{
    class Program
    {
        static void Main(string[] args)
        {
            /**
             * DOMAIN:
                LISTENER_RC_for_LRM[0], 
	            LISTENER_RC_forLOWER_RC[1], 
	            LISTENER_CC_forLOWER_CC[2] ,
	            NCC_PORT[3] 
            * SUBNETWORK:
                LISTENER_RC_for_LRM[0], 
	            LISTENER_RC_forLOWER_RC[1], 
	            LISTENER_CC_forLOWER_CC[2] ,
                UPPER_RC_PORT[3], 
                UPPER_CC_PORT[4]] 
            */
            string[] rcArgs = null;
            if(args.Length == 4)
                rcArgs = new string[] { args[0], args[1] }; // DOMAIN [listen LRM , listen RC]
            if (args.Length == 5)
                rcArgs = new string[] { args[0], args[1], args[3] }; // SUBNETWORK [listen LRM , listen RC , connect up RC] 

            string[] ccArgs = null;
            if (args.Length == 4)
                ccArgs = new string[] { args[2], args[3]}; // DOMAIN [listen CC, connect NCC]
            if (args.Length == 5)
                ccArgs = new string[] { args[2], args[3], args[3] }; // SUBNETWORK [listen CC, connect up CC, JUST_FLAG]

            RoutingController rc = new RoutingController(rcArgs);
            ConnectionController cc = new ConnectionController(ccArgs);
        }
    }
}
