using System;
using System.Collections.Generic;

namespace Management
{
    public class ManagmentProtocol
    {
        public static readonly int WHOIS = 0;
        public static readonly int ROUTINGTABLES = 1;
        public static readonly int POSSIBLEDESITATIONS = 2;
        public static readonly int ROUTINGENTRY = 3;
        public static readonly int CONFIRMATION = 4;
        public static readonly int INTERFACEINFORMATION = 5;
        public static readonly int CLEARTABLE = 6;
        public static readonly int GETTABLE = 7;
        public static readonly int TOOTHERNCC = 8;
        public static readonly int SOFTPERNAMENT = 9;
        public static readonly int RELEASESOFTPERNAMENT = 10;

        private int state;
        private int port;
        private String[] message;
        private List<FIB> routingTable;
        private FIB routingEntry;
        private String name;
        private Dictionary<String, int> possibleDestinations;
        private Dictionary<int, String> interfaces;
        private List<int> connectionToOtherNcc;
        private String nodeStart;
        private String nodeEnd;
        private int speed;
        private String connection;

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

        public List<int> ConnectionToOtherNcc
        {
            get
            {
                return connectionToOtherNcc;
            }

            set
            {
                connectionToOtherNcc = value;
            }
        }

        public string NodeStart
        {
            get
            {
                return nodeStart;
            }

            set
            {
                nodeStart = value;
            }
        }

        public string NodeEnd
        {
            get
            {
                return nodeEnd;
            }

            set
            {
                nodeEnd = value;
            }
        }

        public int Speed
        {
            get
            {
                return speed;
            }

            set
            {
                speed = value;
            }
        }

        public string Connection
        {
            get
            {
                return connection;
            }

            set
            {
                connection = value;
            }
        }
    }
}
