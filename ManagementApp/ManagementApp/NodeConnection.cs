using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    public class NodeConnection : ContainerElement
    {
        private Node from, to;
        public NodeConnection(Node from, Node to, String name)
        {
            this.From = from;
            this.To = to;
            Start = from.Position;
            End = to.Position;
            this.Name = name;
        }

        public Node From
        {
            get
            {
                return from;
            }

            set
            {
                from = value;
            }
        }

        public Node To
        {
            get
            {
                return to;
            }

            set
            {
                to = value;
            }
        }
    }
}
