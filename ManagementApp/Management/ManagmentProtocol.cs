using System;
using System.Collections.Generic;

namespace Management
{
    public class ManagmentProtocol
    {
        private static readonly int wHOIS = 0;
        private static readonly int rOUTINGTABLES = 1;
        private static readonly int pOSSIBLEDESITATIONS = 2;
        private static readonly int rOUTINGENTRY = 3;
        private static readonly int cONFIRMATION = 4;
        private static readonly int iNTERFACEINFORMATION = 5;
        private static readonly int cLEARTABLE = 6;
        private static readonly int gETTABLE = 7;

        private int state;
        private int port;
        private String[] message;
        private List<FIB> routingTable;
        private FIB routingEntry;
        private String name;
        private Dictionary<String, int> possibleDestinations;
        private Dictionary<int, String> interfaces;

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

        public static int INTERFACEINFORMATION
        {
            get
            {
                return iNTERFACEINFORMATION;
            }
        }

        public Dictionary<int, string> Interfaces
        {
            get
            {
                return interfaces;
            }

            set
            {
                interfaces = value;
            }
        }

        public Dictionary<string, int> PossibleDestinations
        {
            get
            {
                return possibleDestinations;
            }

            set
            {
                possibleDestinations = value;
            }
        }

        public static int CLEARTABLE
        {
            get
            {
                return cLEARTABLE;
            }
        }

        public static int GETTABLE
        {
            get
            {
                return gETTABLE;
            }
        }
    }
}
