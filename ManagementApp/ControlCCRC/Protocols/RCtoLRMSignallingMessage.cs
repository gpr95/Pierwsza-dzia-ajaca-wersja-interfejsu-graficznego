using Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC.Protocols
{
    class RCtoLRMSignallingMessage
    {
        // my node name
        public const int LRM_INIT = 0;
        // new connected node
        public const int LRM_TOPOLOGY_ADD = 1;
        // who died from connected nodes
        public const int LRM_TOPOLOGY_DELETE = 2;

        private int state;
        private int connectedNodePort;
        private String connectedNode;
        private String nodeName;


      

        public int ConnectedNodePort
        {
            get
            {
                return connectedNodePort;
            }

            set
            {
                connectedNodePort = value;
            }
        }

        public string ConnectedNode
        {
            get
            {
                return connectedNode;
            }

            set
            {
                connectedNode = value;
            }
        }

        public string NodeName
        {
            get
            {
                return nodeName;
            }

            set
            {
                nodeName = value;
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
    }
}
