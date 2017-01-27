using ClientWindow;
using ControlCCRC.Protocols;
using Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlCCRC
{
    class Program
    {
        /**
             * DOMAIN:
                LISTENER_RCandCC_for_LRM_AND_RC_AND_CC[0],  
                SUBNETWORK_ID[1],
	            NCC_PORT[2],
                DOMAIN_FLAG[3]
            * SUBNETWORK:
                LISTENER_RCandCC_for_LRM_AND_RC_AND_CC[0], 
                SUBNETWORK_ID[1],
                UPPER_RCandCC_PORT[2], 
            */
        static void Main(string[] args)
        {
            Dictionary<String, ListenerHandler> socketHandler = new Dictionary<string, ListenerHandler>();
            int domainId = Int32.Parse(args[1]);

            string rcId = "RC" + domainId;
            string ccId = "CC" + domainId;

            string[] rcArgs = new string[] { };
            if (args.Length == 4)
                rcArgs = new string[] { rcId }; // DOMAIN [RC_ID]
            else if (args.Length == 3)
                rcArgs = new string[] { rcId, args[2]}; // SUBNETWORK [RC_ID, connect up RC] 
            else
                errorWriter("[ERROR] Wrong aguments.");

            string[] ccArgs = new string[] { };
            if (args.Length == 4)
                ccArgs = new string[] { ccId, args[2]}; // DOMAIN [CC_ID, connect NCC]
            else if (args.Length == 3)
                ccArgs = new string[] { ccId, args[2], args[2]}; // SUBNETWORK [CC_ID, connect up CC, flag]
            else
                errorWriter("[ERROR] Wrong aguments.");

            RoutingController rc = new RoutingController(rcArgs);
            ConnectionController cc = new ConnectionController(ccArgs);

            rc.setCCHandler(cc);
            cc.setRCHandler(rc);
            rc.setSocketHandler(socketHandler);
            cc.setSocketHandler(socketHandler);

            // LISTENER[0]
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(args[0]));
            listener.Start();

            Boolean noError = true;
            while (noError)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ListenerHandler thread = new ListenerHandler(client, rc, cc,ref  socketHandler);
                }
                catch (SocketException ex)
                {
                    Program.errorWriter("[ERROR] Socket failed. Listener.");
                    noError = false;
                }
            }
        }

        private static void errorWriter(String msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.White;

            Console.Write("#" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "#:" + msg);
            Console.Write(Environment.NewLine);
        }
    }
}
