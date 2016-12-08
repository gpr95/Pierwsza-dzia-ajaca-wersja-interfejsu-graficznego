using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    [Serializable()]
    public class ConnectionProperties
    {
        private int virtualPortFrom;
        private int virtualPortTo;
        private int localPortFrom;
        private int localPortTo;
        public ConnectionProperties()
        {

        }
        public ConnectionProperties(int lFrom, int vFrom, int lTo, int vTo)
        {
            LocalPortFrom = lFrom;
            VirtualPortFrom = vFrom;
            LocalPortTo = lTo;
            VirtualPortTo = vTo;
        }
        public int VirtualPortFrom
        {
            get
            {
                return virtualPortFrom;
            }

            set
            {
                virtualPortFrom = value;
            }
        }

        public int VirtualPortTo
        {
            get
            {
                return virtualPortTo;
            }

            set
            {
                virtualPortTo = value;
            }
        }

        public int LocalPortFrom
        {
            get
            {
                return localPortFrom;
            }

            set
            {
                localPortFrom = value;
            }
        }

        public int LocalPortTo
        {
            get
            {
                return localPortTo;
            }

            set
            {
                localPortTo = value;
            }
        }


    }
}
