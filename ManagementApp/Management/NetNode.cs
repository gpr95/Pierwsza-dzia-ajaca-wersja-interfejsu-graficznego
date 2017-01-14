using System;
using System.Diagnostics;
using System.Drawing;

namespace Management
{

    [Serializable()]
    public class NetNode : Node
    {
        public NetNode(String name, int localPort)
        {
            this.Name = name;
            this.LocalPort = localPort;

            String parameters = name + " " + this.LocalPort + " " + this.ManagmentPort;
            ProcessStartInfo startInfo = new ProcessStartInfo("NetNode.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = parameters;

            this.ProcessHandle = Process.Start(startInfo);
        }

        public NetNode(NetNode nnode) : this(nnode.Name, nnode.LocalPort) { }
    }
}
