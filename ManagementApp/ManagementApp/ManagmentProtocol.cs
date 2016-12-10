using System;
using System.Collections.Generic;

namespace ManagementApp
{
    public class ManagmentProtocol
    {
        private static readonly int wHOIS = 0;
        private static readonly int rOUTINGTABLES = 1;
        private static readonly int pOSSIBLEDESITATIONS = 2;
        private static readonly int rOUTINGENTRY = 3;
        private static readonly int cONFIRMATION = 4;

        private int state;
        private int port;
        private String[] message;
        private List<FIB> routingTable;
        private FIB routingEntry;
        private String name;
        public Dictionary<String, int> possibleDestinations;

        public static int WHOIS
        {
            get
            {
                return wHOIS;
            }
        }

        public static int ROUTINGTABLES
        {
            get
            {
                return rOUTINGTABLES;
            }
        }

        public static int POSSIBLEDESITATIONS
        {
            get
            {
                return pOSSIBLEDESITATIONS;
            }
        }

        public static int CONFIRMATION
        {
            get
            {
                return cONFIRMATION;
            }
        }

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

        public string[] Message
        {
            get
            {
                return message;
            }

            set
            {
                message = value;
            }
        }

        public List<FIB> RoutingTable
        {
            get
            {
                return routingTable;
            }

            set
            {
                routingTable = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                port = value;
            }
        }

        public FIB RoutingEntry
        {
            get
            {
                return routingEntry;
            }

            set
            {
                routingEntry = value;
            }
        }

        public static int ROUTINGENTRY
        {
            get
            {
                return rOUTINGENTRY;
            }
        }
    }
}
