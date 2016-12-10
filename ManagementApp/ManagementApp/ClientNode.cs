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
    public class ClientNode : Node
    {
        public ClientNode(int x, int y, String name, int localPort)
        { 
            String parameters = name + " " + this.localPort + " " + this.ManagmentPort;

            this.Name = name;
            this.localPort = localPort;
            this.processHandle = System.Diagnostics.Process.Start("ClientNode.exe", parameters);
            Position = new Point(x, y);
        }
    }
}
