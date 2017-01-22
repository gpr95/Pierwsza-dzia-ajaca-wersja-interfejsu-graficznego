using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC
{
    class Program
    {
        /**
             * DOMAIN:
                LISTENER_RC_for_LRM_AND_RC[0],  
	            LISTENER_CC_forLOWER_CC[1] ,
	            NCC_PORT[2] 
            * SUBNETWORK:
                LISTENER_RC_for_LRM_AND_RC[0], 
	            LISTENER_CC_forLOWER_CC[1] ,
                UPPER_RC_PORT[2], 
                UPPER_CC_PORT[3]] 
            */
        static void Main(string[] args)
        { 
            string[] rcArgs = null;
            if(args.Length == 3)
                rcArgs = new string[] { args[0]}; // DOMAIN [listen LRM_AND_RC]
            if (args.Length == 4)
                rcArgs = new string[] { args[0], args[2] }; // SUBNETWORK [listen LRM_AND_RC , connect up RC] 

            string[] ccArgs = null;
            if (args.Length == 3)
                ccArgs = new string[] { args[1], args[2]}; // DOMAIN [listen CC, connect NCC]
            if (args.Length == 4)
                ccArgs = new string[] { args[1], args[3], args[3] }; // SUBNETWORK [listen CC, connect up CC, JUST_FLAG]

            RoutingController rc = new RoutingController(rcArgs);
            ConnectionController cc = new ConnectionController(ccArgs);

            rc.setCCHandler(cc);
            cc.setRCHandler(rc);
        }
    }
}
