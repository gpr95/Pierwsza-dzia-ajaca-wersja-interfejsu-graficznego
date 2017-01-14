using System;
using System.Diagnostics;

namespace Management
{
    [Serializable()]
    public class ClientNode : Node
    {
        public ClientNode(String name, int localPort)
        { 
            this.Name = name;
            this.LocalPort = localPort;

            String parameters = name + " " + this.LocalPort + " " + this.ManagmentPort;
            ProcessStartInfo startInfo = new ProcessStartInfo("ClientNode.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            startInfo.Arguments = parameters;

            this.ProcessHandle = Process.Start(startInfo);
        }

        public ClientNode(ClientNode cnode) : this(cnode.Name, cnode.LocalPort) { }
    }
}
