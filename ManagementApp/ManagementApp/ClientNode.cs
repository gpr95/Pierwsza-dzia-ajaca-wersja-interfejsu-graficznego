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
                //String parameters = null;
                //string[] parameters = new string[] { name, this.CloudCablePort.ToString(), this.ManagmentPort.ToString()};
                this.localPort = localPort;
                String parameters = name + " " + this.localPort + " " + this.ManagmentPort;
                //TODO Starting ClientNodes with constructor
                this.processHandle = System.Diagnostics.Process.Start("ClientNode.exe", parameters);
                Position = new Point(x, y);
                this.Name = name;
            }
    }
}
