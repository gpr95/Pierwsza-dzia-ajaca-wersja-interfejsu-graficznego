using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetNode
{
    class Resource
    {
        public int port;
        public int no_vc3;
        public bool status;

        public Resource(int port, int no_vc3, bool status)
        {
            this.port = port;
            this.no_vc3 = no_vc3;
            this.status = status;
        }

        public String toString()
        {
            return "port: " + port + " no_vc3: " + no_vc3 + " status: " + status;
        }
    }
}