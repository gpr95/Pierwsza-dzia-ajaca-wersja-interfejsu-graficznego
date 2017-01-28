using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    class RCtoRCSignallingMessage
    {
        // lower identifier sending identifier RC_ID to upper RC (identifier)
        public const int RC_FROM_SUBNETWORK_INIT = 0;
        // upper RC request for couning his all paths (allUpperNodesToCountWeights, rateToCountWeights)
        public const int COUNT_ALL_PATHS_REQUEST = 1;
        public const int COUNTED_ALL_PATHS_CONFIRM = 2;
        public const int COUNTED_ALL_PATHS_REFUSE = 3;
        private int state;



        private String identifier;

        // state 0
        private List<String> allUpperNodesToCountWeights;
        private int rateToCountWeights;

        // state 1
        Dictionary<String, Dictionary<String, int>> nodeConnectionsAndWeights;
        Dictionary<String, String> associatedNodesInSubnetwork;
        private List<int> pathWeight;

        public string Identifier
        {
            get
            {
                return identifier;
            }

            set
            {
                identifier = value;
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

        public List<string> AllUpperNodesToCountWeights
        {
            get
            {
                return allUpperNodesToCountWeights;
            }

            set
            {
                allUpperNodesToCountWeights = value;
            }
        }

        public List<int> PathWeight
        {
            get
            {
                return pathWeight;
            }

            set
            {
                pathWeight = value;
            }
        }

        public int RateToCountWeights
        {
            get
            {
                return rateToCountWeights;
            }

            set
            {
                rateToCountWeights = value;
            }
        }

        public Dictionary<string, Dictionary<string, int>> NodeConnectionsAndWeights
        {
            get
            {
                return nodeConnectionsAndWeights;
            }

            set
            {
                nodeConnectionsAndWeights = value;
            }
        }

        public Dictionary<string, string> AssociatedNodesInSubnetwork
        {
            get
            {
                return associatedNodesInSubnetwork;
            }

            set
            {
                associatedNodesInSubnetwork = value;
            }
        }
    }
}
