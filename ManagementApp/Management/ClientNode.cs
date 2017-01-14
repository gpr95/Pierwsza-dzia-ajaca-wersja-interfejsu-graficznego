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
        }

        public ClientNode(ClientNode cnode) : this(cnode.Name, cnode.LocalPort) { }
    }
}
