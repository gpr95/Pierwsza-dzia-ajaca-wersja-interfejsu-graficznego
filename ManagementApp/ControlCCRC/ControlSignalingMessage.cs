using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCCRC
{
    class ControlSignalingMessage
    {

        private Header headerField;
        private String clientAddress;
        private List<String> connectedNodes;
        private String whoDied;

        public enum Header
        {
            INIT, TOPOLOGY, SOMEONE_DIED
        }

     

        public Header HeaderField
        {
            get
            {
                return headerField;
            }

            set
            {
                headerField = value;
            }
        }

        public string ClientAddress
        {
            get
            {
                return clientAddress;
            }

            set
            {
                clientAddress = value;
            }
        }

        public List<string> ConnectedNodes
        {
            get
            {
                return connectedNodes;
            }

            set
            {
                connectedNodes = value;
            }
        }

        public string WhoDied
        {
            get
            {
                return whoDied;
            }

            set
            {
                whoDied = value;
            }
        }
    }

   
}
