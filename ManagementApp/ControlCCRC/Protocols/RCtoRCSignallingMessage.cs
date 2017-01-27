using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    class RCtoRCSignallingMessage
    {
        public const int COUNT_ALL_PATHS=0;
        public const int COUNTED_ALL_PATHS_CONFIRM = 1;
        public const int COUNTED_ALL_PATHS_REFUSE = 2;
        private int state;



        private String identifier;

        // state 0
        private List<String> allUpperNodesToCountWeights;
        private int rateToCountWeights;

        // state 1
        private List<Dictionary<String, String>> fromTo;
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

        public List<Dictionary<string, string>> FromTo
        {
            get
            {
                return fromTo;
            }

            set
            {
                fromTo = value;
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
    }
}
