using System;
using System.Diagnostics;
using System.Drawing;

namespace ManagementApp
{
    [Serializable()]
    public class ClientNode : Node
    {
        public ClientNode(int x, int y, String name, int localPort)
        { 
            this.Name = name;
            this.LocalPort = localPort;
            this.Position = new Point(x, y);

            String parameters = name + " " + this.LocalPort + " " + this.ManagmentPort;
            ProcessStartInfo startInfo = new ProcessStartInfo("ClientNode.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = parameters;

            this.processHandle = Process.Start(startInfo);
        }

        public ClientNode(ClientNode cnode) : this(cnode.Position.X, cnode.Position.Y, cnode.Name, cnode.LocalPort) { }
    }
}
