using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementApp
{
    public class NodeConnection
    {
        private Point start;
        private Point end;
        private String name;
        private Node from, to;
        private int virtualPortFrom;
        private int virtualPortTo;
        private int localPortFrom;
        private int localPortTo;

        public NodeConnection(Node from, int virtualPortFrom, Node to, int virtualPortTo, String name)
        {
            this.virtualPortFrom = virtualPortFrom;
            this.virtualPortTo = virtualPortTo;
            this.From = from;
            this.To = to;
            this.Name = name;

            localPortFrom = from.LocalPort;
            localPortTo = to.LocalPort;
            Start = from.Position;
            End = to.Position;
        }

        public Point Start
        {
            get
            {
                return start;
            }

            set
            {
                start = value;
            }
        }

        public Point End
        {
            get
            {
                return end;
            }

            set
            {
                end = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
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

        public int VirtualPortFrom
        {
            get {  return virtualPortFrom; }
        }

        public int VirtualPortTo
        {
            get {  return virtualPortTo; }
        }

        public int LocalPortFrom
        {
            get { return localPortFrom; }
        }

        public int LocalPortTo
        {
            get { return localPortTo; }
        }
    }
}
