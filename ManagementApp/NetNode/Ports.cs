using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetNode
{
    class Ports
    {
        public List<IPort> iports = new List<IPort>();
        public List<OPort> oports = new List<OPort>();
        public Ports()
        {
            for (int i = 0; i <= 3; i++)
            {
                this.iports.Add(new IPort(i));
                this.oports.Add(new OPort(i));
            }
        }
    }
}
