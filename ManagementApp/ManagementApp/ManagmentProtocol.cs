using System;
using NetNode;
using System.Collections.Generic;

namespace ManagementApp
{
    class ManagmentProtocol
    {
        private static readonly int wHOIS = 0;
        private static readonly int rOUTINGTABLES = 1;
        private static readonly int pOSSIBLEDESITATIONS = 2;
        private static readonly int cONFIRMATION = 3;

        private int state;
        private String[] message;
        private List<List<String>> routingTable;
        private String name;
        //private List<FIB> listOfFibs;

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

        public List<List<string>> RoutingTable
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
    }
}
