using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    class ManagmentProtocol
    {
        private static readonly int whoIs = 0;
        private static readonly int routingTables = 1;
        private static readonly int possibleDestinations = 2;
        private static readonly int cONFIRMATION = 3;

        private int state;
        private String[] message;
        private List<List<String>> routingTable;
        private String name;

        public int WHOIS
        {
            get
            {
                return whoIs;
            }
        }

        public static int ROUTINGTABLES
        {
            get
            {
                return routingTables;
            }
        }

        public static int POSSIBLEDESTINATIONS
        {
            get
            {
                return possibleDestinations;
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

        public static int CONFIRMATION
        {
            get
            {
                return cONFIRMATION;
            }
        }
    }
}
