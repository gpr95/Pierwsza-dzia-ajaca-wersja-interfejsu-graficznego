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
            this.Name = name;
            this.localPort = localPort;
            String parameters = name + " " + this.localPort + " " + this.ManagmentPort;
            this.processHandle = System.Diagnostics.Process.Start("ClientNode.exe", parameters);
            Position = new Point(x, y);
        }

        public ClientNode(ClientNode cnode) : this(cnode.Position.X, cnode.Position.Y, cnode.Name, cnode.LocalPort)
        {
        }
    }
}
