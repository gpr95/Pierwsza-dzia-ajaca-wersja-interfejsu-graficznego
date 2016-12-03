using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementApp
{

    public class NetNode : Node
    {
        public NetNode(int x , int y,String name, int localPort)
        {
            Position = new Point(x, y);
            //TODO Starting NettNodes with constructor
            //this.processHandle = System.Diagnostics.Process.Start("NetworkNode.exe", parameters);
            this.name = name;
            this.localPort = localPort;
        }
    }
}
