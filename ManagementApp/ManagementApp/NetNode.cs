using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementApp
{

    [Serializable()]
    public class NetNode : Node
    {
        public NetNode(int x , int y,String name, int localPort)
        {
            this.localPort = localPort;
            Position = new Point(x, y);
            String parameters = name + " " + this.localPort + " " + this.ManagmentPort;
            this.processHandle = System.Diagnostics.Process.Start("NetNode.exe", parameters);
            this.name = name;
        }
    }
}
