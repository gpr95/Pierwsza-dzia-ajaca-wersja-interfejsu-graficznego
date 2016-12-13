using System;
using System.Diagnostics;
using System.Drawing;

namespace ManagementApp
{

    [Serializable()]
    public class NetNode : Node
    {
        public NetNode(int x , int y,String name, int localPort)
        {
            this.Name = name;
            this.LocalPort = localPort;
            this.Position = new Point(x, y);

            String parameters = name + " " + this.LocalPort + " " + this.ManagmentPort;
            ProcessStartInfo startInfo = new ProcessStartInfo("NetNode.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = parameters;

            this.processHandle = Process.Start(startInfo);
        }

        public NetNode(NetNode nnode) : this(nnode.Position.X, nnode.Position.Y, nnode.Name, nnode.LocalPort) { }
    }
}
