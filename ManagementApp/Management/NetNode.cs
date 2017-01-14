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
        }

        public NetNode(NetNode nnode) : this(nnode.Name, nnode.LocalPort) { }
    }
}
